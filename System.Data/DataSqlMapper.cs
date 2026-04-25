/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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
using System.ComponentModel.DataAnnotations.Schema;
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

		// Server-side evaluation: the SQL already projected the result columns
		// with proper aliases. Map directly to TResult without creating an
		// intermediate TData entity.
		if (specification.SelectorEvaluation == SelectorEvaluation.Server)
		{
			// Scalar member projection (e.g., p => p.Name): read the column by name
			// since the result set may contain it at any ordinal.
			if (IsScalarType(typeof(TResult))
				&& specification.Selector.Body is MemberExpression scalarMember)
			{
				string columnName = scalarMember.Member is PropertyInfo scalarProp
					&& scalarProp.GetCustomAttribute<ColumnAttribute>() is { Name: { } attrName }
					? attrName
					: scalarMember.Member.Name;
				return MapScalarByColumnName<TResult>(reader, columnName);
			}

			return MapToResult<TResult>(reader);
		}

		// Client-side evaluation: all entity columns were selected.
		// Materialize entities first, then apply the selector in memory.
		// This supports extension methods, complex transformations, and
		// any projection that cannot be translated to SQL.

		// Multi-parameter selector (joins): materialize each entity from the
		// flat reader and invoke the selector with all of them.
		if (specification.Selector.Parameters.Count > 1)
		{
			return MaterializeMultiParameterSelector<TData, TResult>(specification.Selector, reader);
		}

		if (specification.Selector is not Expression<Func<TData, TResult>> typedSelector)
		{
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

		// Identity short-circuit: no projection needed
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
			// For value types, keep the instance boxed so PopulateProperties
			// can mutate properties in place via Expression.Unbox.
			if (resultType.IsValueType)
			{
				object boxed = Activator.CreateInstance(resultType)!;
				PopulateProperties(boxed, metadata, reader, paramNames: null);
				return (TResult)boxed;
			}

			TResult? instance = Activator.CreateInstance<TResult>();
			PopulateProperties(instance!, metadata, reader, paramNames: null);
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

		if (resultType.IsValueType)
		{
			object boxed = result!;
			PopulateProperties(boxed, metadata, reader, ctorParamNames);
			return (TResult)boxed;
		}

		PopulateProperties(result!, metadata, reader, ctorParamNames);
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

	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Scalar types used at this call site do not require constructor metadata.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "Scalar types used at this call site do not require constructor metadata.")]
	[UnconditionalSuppressMessage("Trimming", "IL2072:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "Scalar types used at this call site do not require constructor metadata.")]
	[UnconditionalSuppressMessage("Trimming", "IL2091:DynamicallyAccessedMembers", Justification = "Scalar types do not require constructor annotations.")]
	private static TResult MapScalarByColumnName<TResult>(DbDataReader reader, string columnName)
	{
		int ordinal = reader.GetOrdinal(columnName);
		object value = reader.GetValue(ordinal);
		if (value == DBNull.Value)
		{
			return default!;
		}

		return value.ChangeTypeNullable<TResult>(CultureInfo.InvariantCulture)!;
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
				if (param.Name is null)
				{
					matches = false;
					break;
				}

				// Match by column name directly, or by property name → ColumnAttribute name.
				if (!columns.ContainsKey(param.Name)
					&& !(metadata.PropertyByName.TryGetValue(param.Name, out PropertyInfo? prop)
						 && prop.GetCustomAttribute<ColumnAttribute>() is { Name: { } attrName }
						 && columns.ContainsKey(attrName)))
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
	[UnconditionalSuppressMessage("Trimming", "IL2075:DynamicallyAccessedMembers on Type.GetProperty",
		Justification = "Constructor parameter resolution requires property lookup for ColumnAttribute fallback.")]
	private static object CreateWithConstructor(ConstructorInfo constructor, DbDataReader reader, Dictionary<string, int> columns)
	{
		ParameterInfo[] parameters = constructor.GetParameters();
		object?[] args = new object?[parameters.Length];

		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterInfo param = parameters[i];

			// Match by parameter name directly, or fall back to ColumnAttribute name
			// (mirrors the logic in SelectConstructor).
			if (!columns.TryGetValue(param.Name!, out int ordinal))
			{
				PropertyInfo? prop = constructor.DeclaringType?
					.GetProperty(param.Name!, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

				if (prop?.GetCustomAttribute<ColumnAttribute>() is { Name: { } attrName }
					&& columns.TryGetValue(attrName, out ordinal))
				{
					// resolved via ColumnAttribute
				}
				else
				{
					throw new InvalidOperationException(
						$"Constructor parameter '{param.Name}' could not be resolved to a column.");
				}
			}

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
		HashSet<string>? paramNames)
	{
		for (int i = 0; i < reader.FieldCount; i++)
		{
			string columnName = reader.GetName(i);

			// Try property name first, then fall back to ColumnAttribute name.
			if (!metadata.PropertyByName.TryGetValue(columnName, out PropertyInfo? property)
				&& !metadata.PropertyByColumnName.TryGetValue(columnName, out property))
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

			// For value types, use Unbox to get a managed pointer into the box,
			// allowing in-place mutation. Convert would copy the struct out of
			// the box and discard changes.
			Expression castInstance = prop.DeclaringType!.IsValueType
				? Expression.Unbox(instance, prop.DeclaringType!)
				: Expression.Convert(instance, prop.DeclaringType!);

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
			var propertyByColumnName = new Dictionary<string, PropertyInfo>(properties.Length, StringComparer.OrdinalIgnoreCase);
			foreach (PropertyInfo prop in properties)
			{
				propertyByName.TryAdd(prop.Name, prop);

				// Also index by ColumnAttribute.Name so that the mapper can resolve
				// database column names that differ from the CLR property name.
				ColumnAttribute? columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
				string columnName = columnAttr?.Name ?? prop.Name;
				propertyByColumnName.TryAdd(columnName, prop);
			}

			ConstructorInfo[] constructors = t.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
			bool hasParameterlessCtor = t.IsValueType || t.GetConstructor(Type.EmptyTypes) is not null;

			return new TypeMetadata(properties, propertyByName, propertyByColumnName, constructors, hasParameterlessCtor);
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

	private static readonly ConcurrentDictionary<LambdaExpression, Delegate> _multiParamSelectorCache = new(ReferenceEqualityComparer.Instance);

	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
	[UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
	[UnconditionalSuppressMessage("Trimming", "IL2072:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
	[UnconditionalSuppressMessage("Trimming", "IL2070:DynamicallyAccessedMembers on Type.GetProperties", Justification = "<Pending>")]
	[UnconditionalSuppressMessage("Trimming", "IL2067:DynamicallyAccessedMembers", Justification = "<Pending>")]
	private static TResult MaterializeMultiParameterSelector<TData, TResult>(
		LambdaExpression selector,
		DbDataReader reader)
		where TData : class
	{
		// Build the column lookup once
		Dictionary<string, int> columns = BuildColumnLookup(reader);

		// Materialize each parameter's entity from the flat reader
		var args = new object?[selector.Parameters.Count];
		for (int i = 0; i < selector.Parameters.Count; i++)
		{
			Type paramType = selector.Parameters[i].Type;

			if (IsScalarType(paramType))
			{
				// Find the first column that matches and read it
				object value = reader.GetValue(0);
				args[i] = value == DBNull.Value ? null : value.ChangeTypeNullable(paramType, CultureInfo.InvariantCulture);
			}
			else
			{
				TypeMetadata metadata = GetOrCreateTypeMetadata(paramType);

				// Check if any columns match this entity's properties
				bool hasAnyColumn = false;
				for (int c = 0; c < reader.FieldCount; c++)
				{
					string colName = reader.GetName(c);
					if (metadata.PropertyByName.ContainsKey(colName) || metadata.PropertyByColumnName.ContainsKey(colName))
					{
						hasAnyColumn = true;
						break;
					}
				}

				if (!hasAnyColumn)
				{
					args[i] = paramType.IsValueType ? Activator.CreateInstance(paramType) : null;
					continue;
				}

				object instance = Activator.CreateInstance(paramType)!;
				PopulateProperties(instance, metadata, reader, paramNames: null);
				args[i] = instance;
			}
		}

		// Compile and invoke the selector with all materialized entities
		Delegate compiled = _multiParamSelectorCache.GetOrAdd(selector, static expr => expr.Compile());
		return (TResult)compiled.DynamicInvoke(args)!;
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
		Dictionary<string, PropertyInfo> PropertyByColumnName,
		ConstructorInfo[] Constructors,
		bool HasParameterlessConstructor);
}
