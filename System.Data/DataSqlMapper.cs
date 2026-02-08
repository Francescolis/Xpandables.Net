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
/// class to convert data retrieved from a DbDataReader into strongly typed results for application use.</remarks>
public sealed class DataSqlMapper : IDataSqlMapper
{
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
                throw new InvalidOperationException("Selector must have a single parameter.");

            var parameter = specification.Selector.Parameters[0];
            if (!parameter.Type.IsAssignableTo(typeof(TData)))
                throw new InvalidOperationException("Selector parameter type must match the data type.");

            var body = specification.Selector.Body;
            if (body.Type != typeof(TResult))
            {
                body = Expression.Convert(body, typeof(TResult));
            }

            typedSelector = Expression.Lambda<Func<TData, TResult>>(body, parameter);
        }

        var entity = MapToResult<TData>(reader);
        if (typedSelector.Body is ParameterExpression && typeof(TResult) == typeof(TData))
        {
            return (TResult)(object)entity;
        }

        var projector = CompileSelector(typedSelector);
        return projector(entity);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("Trimming", "IL2072:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    private static Func<TData, TResult> CompileSelector<TData, TResult>(Expression<Func<TData, TResult>> selector)
        where TData : class
        => selector.Compile(preferInterpretation: true);

    /// <inheritdoc/>
    public TResult MapToResult<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] TResult>(DbDataReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        var resultType = typeof(TResult);

        if (IsScalarType(resultType))
        {
            return MapScalar<TResult>(reader, resultType);
        }

        var columns = BuildColumnLookup(reader);

        var parameterlessCtor = resultType.GetConstructor(Type.EmptyTypes);
        if (parameterlessCtor != null)
        {
            var instance = Activator.CreateInstance<TResult>();
            PopulateProperties(instance!, resultType, reader, columns, paramNames: null);
            return instance!;
        }

        var constructor = SelectConstructor(resultType, columns);
        if (constructor is null)
        {
            throw new InvalidOperationException(
                $"No suitable public constructor was found for '{resultType.FullName}'.");
        }

        var result = (TResult)CreateWithConstructor(constructor, reader, columns);

        var ctorParamNames = constructor
            .GetParameters()
            .Select(p => p.Name!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        PopulateProperties(result!, resultType, reader, columns, ctorParamNames);
        return result;
    }

    private static bool IsScalarType(Type type)
    {
        if (type == typeof(string))
            return true;

        var underlying = Nullable.GetUnderlyingType(type) ?? type;

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
        var value = reader.GetValue(0);
        if (value == DBNull.Value)
            return default!;

        var converted = value.ChangeTypeNullable(resultType, CultureInfo.CurrentCulture);
        return (TResult)converted!;
    }

    private static Dictionary<string, int> BuildColumnLookup(DbDataReader reader)
    {
        var columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < reader.FieldCount; i++)
        {
            var name = reader.GetName(i);
            if (!columns.ContainsKey(name))
            {
                columns[name] = i;
            }
        }
        return columns;
    }

    private static ConstructorInfo? SelectConstructor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type resultType, Dictionary<string, int> columns)
    {
        ConstructorInfo? best = null;
        var bestScore = -1;

        foreach (var ctor in resultType.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
        {
            var parameters = ctor.GetParameters();
            if (parameters.Length == 0)
                continue;

            var matches = true;
            foreach (var param in parameters)
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
        var parameters = constructor.GetParameters();
        var args = new object?[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            var ordinal = columns[param.Name!];
            var value = reader.GetValue(ordinal);
            args[i] = value.ChangeTypeNullable(param.ParameterType, CultureInfo.CurrentCulture);
        }

        return constructor.Invoke(args);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("Trimming", "IL2072:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    private static void PopulateProperties(
        object instance,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type resultType,
        DbDataReader reader,
        Dictionary<string, int> columns,
        HashSet<string>? paramNames)
    {
        var properties = resultType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        for (var i = 0; i < reader.FieldCount; i++)
        {
            var columnName = reader.GetName(i);
            var property = FindProperty(properties, columnName);

            if (property is null || !property.CanWrite)
                continue;

            if (paramNames != null && paramNames.Contains(property.Name))
                continue;

            var value = reader.GetValue(i);
            if (value == DBNull.Value)
                continue;

            var convertedValue = value.ChangeTypeNullable(property.PropertyType, CultureInfo.CurrentCulture);
            property.SetValue(instance, convertedValue);
        }
    }

    private static PropertyInfo? FindProperty(PropertyInfo[] properties, string columnName)
    {
        foreach (var prop in properties)
        {
            if (string.Equals(prop.Name, columnName, StringComparison.OrdinalIgnoreCase))
                return prop;
        }
        return null;
    }
}
