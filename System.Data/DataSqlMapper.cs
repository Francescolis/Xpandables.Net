/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
********************************************************************************/
using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data;

/// <summary>
/// Provides functionality to map data from a database record to a specified result type.
/// </summary>
/// <remarks>Implements the IDataSqlMapper interface to enable custom mapping of database records to objects. Use this
/// class to convert data retrieved from a DbDataReader into strongly typed results for application use.
/// <para>Reflection metadata (properties, constructors, setter delegates) is cached per type on first use,
/// eliminating per-row reflection overhead.</para></remarks>
public sealed class DataSqlMapper : IDataSqlMapper
{
    private static readonly ConcurrentDictionary<LambdaExpression, Delegate> _compiledSelectors = new(ReferenceEqualityComparer.Instance);
    private static readonly ConcurrentDictionary<Type, TypeMetadata> _typeMetadataCache = new();
    private static readonly ConcurrentDictionary<PropertyInfo, Action<object, object?>> _setterCache = new();
    private static readonly ConcurrentDictionary<ConstructorInfo, Func<object?[], object>> _ctorDelegateCache = new();

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("Trimming", "IL2091:Target generic argument does not have matching annotations", Justification = "Selector mapping requires dynamic access to entity members.")]
    public TResult MapToResult<TData, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] TResult>(
        IDataSpecification<TData, TResult> specification,
        DbDataReader reader)
        where TData : class
    {
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(reader);

        if (specification.Selector is not Expression<Func<TData, TResult>> typedSelector)
        {
            if (specification.Selector.Parameters.Count != 1)
			{
				throw new InvalidOperationException("Selector must have a single parameter.");
			}

			ParameterExpression parameter = specification.Selector.Parameters[0];
            if (!parameter.Type.IsAssignableTo(typeof(TData)))
			{
				throw new InvalidOperationException("Selector parameter type must match the data type.");
			}

			Expression body = specification.Selector.Body;
            if (body.Type != typeof(TResult))
            {
                body = Expression.Convert(body, typeof(TResult));
            }

            typedSelector = Expression.Lambda<Func<TData, TResult>>(body, parameter);
        }

        TData entity = MapToResult<TData>(reader);
        if (typedSelector.Body is ParameterExpression && typeof(TResult) == typeof(TData))
        {
            return (TResult)(object)entity;
        }

		Func<TData, TResult> projector = CompileSelector(typedSelector);
        return projector(entity);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("Trimming", "IL2072:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    private static Func<TData, TResult> CompileSelector<TData, TResult>(Expression<Func<TData, TResult>> selector)
        where TData : class
        => (Func<TData, TResult>)_compiledSelectors.GetOrAdd(
            selector,
            static expr => ((Expression<Func<TData, TResult>>)expr).Compile());

    /// <inheritdoc/>
    public TResult MapToResult<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] TResult>(DbDataReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

		Type resultType = typeof(TResult);

        if (IsScalarType(resultType))
        {
            return MapScalar<TResult>(reader, resultType);
        }

		TypeMetadata metadata = GetOrCreateTypeMetadata(resultType);
		Dictionary<string, int> columns = BuildColumnLookup(reader);

        if (metadata.HasParameterlessConstructor)
        {
			TResult? instance = Activator.CreateInstance<TResult>();
            PopulateProperties(instance!, metadata, reader, columns, paramNames: null);
            return instance!;
        }

		ConstructorInfo? constructor = SelectConstructor(metadata, columns);
        if (constructor is null)
        {
            throw new InvalidOperationException(
                $"No suitable public constructor was found for '{resultType.FullName}'.");
        }

        var result = (TResult)CreateWithConstructor(constructor, reader, columns);

		HashSet<string> ctorParamNames = GetCachedCtorParamNames(constructor);

        PopulateProperties(result!, metadata, reader, columns, ctorParamNames);
        return result;
    }

    private static bool IsScalarType(Type type)
    {
        if (type == typeof(string))
		{
			return true;
		}

		Type underlying = Nullable.GetUnderlyingType(type) ?? type;

        return underlying.IsPrimitive
               || underlying.IsEnum
               || underlying == typeof(Guid)
               || underlying == typeof(DateTime)
               || underlying == typeof(DateTimeOffset)
               || underlying == typeof(decimal);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("Trimming", "IL2072:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    private static TResult MapScalar<TResult>(DbDataReader reader, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type resultType)
    {
		object value = reader.GetValue(0);
        if (value == DBNull.Value)
		{
			return default!;
		}

		object? converted = value.ChangeTypeNullable(resultType, CultureInfo.InvariantCulture);
        return (TResult)converted!;
    }

    private static Dictionary<string, int> BuildColumnLookup(DbDataReader reader)
    {
        var columns = new Dictionary<string, int>(reader.FieldCount, StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < reader.FieldCount; i++)
        {
			string name = reader.GetName(i);
            columns.TryAdd(name, i);
        }
        return columns;
    }

    private static ConstructorInfo? SelectConstructor(TypeMetadata metadata, Dictionary<string, int> columns)
    {
        ConstructorInfo? best = null;
		int bestScore = -1;

        foreach (ConstructorInfo ctor in metadata.Constructors)
        {
			ParameterInfo[] parameters = ctor.GetParameters();
            if (parameters.Length == 0)
			{
				continue;
			}

			bool matches = true;
            foreach (ParameterInfo param in parameters)
            {
                if (param.Name is null || !columns.ContainsKey(param.Name))
                {
                    matches = false;
                    break;
                }
            }

            if (matches && parameters.Length > bestScore)
            {
                best = ctor;
                bestScore = parameters.Length;
            }
        }

        return best;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("Trimming", "IL2072:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    private static object CreateWithConstructor(ConstructorInfo constructor, DbDataReader reader, Dictionary<string, int> columns)
    {
		ParameterInfo[] parameters = constructor.GetParameters();
		object?[] args = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
			ParameterInfo param = parameters[i];
			int ordinal = columns[param.Name!];
			object value = reader.GetValue(ordinal);
            args[i] = value.ChangeTypeNullable(param.ParameterType, CultureInfo.InvariantCulture);
        }

		Func<object?[], object> factory = GetCompiledCtorFactory(constructor);
        return factory(args);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("Trimming", "IL2072:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    private static void PopulateProperties(
        object instance,
        TypeMetadata metadata,
        DbDataReader reader,
        Dictionary<string, int> columns,
        HashSet<string>? paramNames)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
			string columnName = reader.GetName(i);
            if (!metadata.PropertyByName.TryGetValue(columnName, out PropertyInfo? property))
			{
				continue;
			}

			if (!property.CanWrite)
			{
				continue;
			}

			if (paramNames is not null && paramNames.Contains(property.Name))
			{
				continue;
			}

			object value = reader.GetValue(i);
            if (value == DBNull.Value)
			{
				continue;
			}

			object? convertedValue = value.ChangeTypeNullable(property.PropertyType, CultureInfo.InvariantCulture);
			Action<object, object?> setter = GetCompiledSetter(property);
            setter(instance, convertedValue);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    private static Action<object, object?> GetCompiledSetter(PropertyInfo property)
    {
        return _setterCache.GetOrAdd(property, static prop =>
        {
			ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
			ParameterExpression value = Expression.Parameter(typeof(object), "value");
			UnaryExpression castInstance = Expression.Convert(instance, prop.DeclaringType!);
			UnaryExpression castValue = Expression.Convert(value, prop.PropertyType);
			BinaryExpression setExpr = Expression.Assign(Expression.Property(castInstance, prop), castValue);
            return Expression.Lambda<Action<object, object?>>(setExpr, instance, value).Compile();
        });
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070:DynamicallyAccessedMembers on Type.GetProperties",
        Justification = "Callers guarantee the type has the required annotations.")]
    [UnconditionalSuppressMessage("Trimming", "IL2067:DynamicallyAccessedMembers",
        Justification = "Callers guarantee the type has the required annotations.")]
    private static TypeMetadata GetOrCreateTypeMetadata(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties
            | DynamicallyAccessedMemberTypes.PublicConstructors
            | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
    {
        return _typeMetadataCache.GetOrAdd(type, static t =>
        {
			PropertyInfo[] properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var propertyByName = new Dictionary<string, PropertyInfo>(properties.Length, StringComparer.OrdinalIgnoreCase);
            foreach (PropertyInfo prop in properties)
            {
                propertyByName.TryAdd(prop.Name, prop);
            }

			ConstructorInfo[] constructors = t.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
			bool hasParameterlessCtor = t.GetConstructor(Type.EmptyTypes) is not null;

            return new TypeMetadata(properties, propertyByName, constructors, hasParameterlessCtor);
        });
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    private static Func<object?[], object> GetCompiledCtorFactory(ConstructorInfo constructor)
    {
        return _ctorDelegateCache.GetOrAdd(constructor, static ctor =>
        {
			ParameterExpression argsParam = Expression.Parameter(typeof(object?[]), "args");
			ParameterInfo[] ctorParams = ctor.GetParameters();
            var argExpressions = new Expression[ctorParams.Length];
            for (int i = 0; i < ctorParams.Length; i++)
            {
				BinaryExpression index = Expression.ArrayIndex(argsParam, Expression.Constant(i));
                argExpressions[i] = Expression.Convert(index, ctorParams[i].ParameterType);
            }
			NewExpression newExpr = Expression.New(ctor, argExpressions);
			UnaryExpression body = Expression.Convert(newExpr, typeof(object));
            return Expression.Lambda<Func<object?[], object>>(body, argsParam).Compile();
        });
    }

    private static readonly ConcurrentDictionary<ConstructorInfo, HashSet<string>> _ctorParamNamesCache = new();

    private static HashSet<string> GetCachedCtorParamNames(ConstructorInfo constructor)
    {
        return _ctorParamNamesCache.GetOrAdd(constructor, static ctor =>
        {
			ParameterInfo[] parameters = ctor.GetParameters();
            var names = new HashSet<string>(parameters.Length, StringComparer.OrdinalIgnoreCase);
            foreach (ParameterInfo p in parameters)
            {
                if (p.Name is not null)
				{
					names.Add(p.Name);
				}
			}
            return names;
        });
    }

    private sealed record TypeMetadata(
        PropertyInfo[] Properties,
        Dictionary<string, PropertyInfo> PropertyByName,
        ConstructorInfo[] Constructors,
        bool HasParameterlessConstructor);
}
