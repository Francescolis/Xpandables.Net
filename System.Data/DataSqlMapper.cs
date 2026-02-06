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
using System.Reflection;

namespace System.Data;

/// <summary>
/// Provides functionality to map data from a database record to a specified result type.
/// </summary>
/// <remarks>Implements the ISqlMapper interface to enable custom mapping of database records to objects. Use this
/// class to convert data retrieved from a DbDataReader into strongly typed results for application use.</remarks>
public sealed class DataSqlMapper : IDataSqlMapper
{
    /// <inheritdoc/>
    public TResult Map<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] TResult>(DbDataReader reader)
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
