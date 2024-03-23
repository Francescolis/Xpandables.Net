
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
using System.Text.Json;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Xpandables.Net.Converters;
/// <summary>
/// Provides with converter extensions.
/// </summary>
public static class ConverterExtensions
{
    /// <summary>
    /// Configures the property so that the property <see cref="DateOnly"/> 
    /// value is converted to 
    /// <see cref="DateTime"/> before writing to the database a
    /// nd converted back when reading from the database.
    /// </summary>
    /// <param name="builder">The property builder instance </param>
    /// <returns>The same builder instance so that 
    /// multiple configuration calls can be chained.</returns>
    public static PropertyBuilder HasDateOnlyConversion(
        this PropertyBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.HasConversion<DateOnlyConverter, DateOnlyComparer>();
    }

    /// <summary>
    /// Configures the property so that the property 
    /// <see cref="TimeOnly"/> value is converted to 
    /// <see cref="TimeSpan"/> before writing to the 
    /// database and converted back when reading from the database.
    /// </summary>
    /// <param name="builder">The property builder instance </param>
    /// <returns>The same builder instance so that 
    /// multiple configuration calls can be chained.</returns>
    public static PropertyBuilder HasTimeOnlyConversion(
        this PropertyBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.HasConversion<TimeOnlyConverter, TimeOnlyComparer>();
    }

    /// <summary>
    /// Configures the property so that the property 
    /// value is converted to/from JSON.
    /// </summary>
    /// <param name="builder">The property builder instance </param>
    /// <returns>The same builder instance so that 
    /// multiple configuration calls can be chained.</returns>
    public static PropertyBuilder HasJsonConversion<TProperty>(
        this PropertyBuilder builder)
        where TProperty : notnull
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.HasConversion<JsonPropertyConverter<TProperty>>();
    }

    /// <summary>
    /// Configures the property so that the type 
    /// <see cref="JsonDocument"/> value is converted to/from JSON.
    /// </summary>
    /// <param name="builder">The property builder instance </param>
    /// <returns>The same builder instance so that 
    /// multiple configuration calls can be chained.</returns>
    public static PropertyBuilder HasJsonDocumentConversion(
        this PropertyBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.HasConversion<JsonDocumentConverter>();
    }
}
