
/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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

using Xpandables.Net.Primitives;

namespace Xpandables.Net.Converters;

/// <summary>
/// Provides extension methods for configuring property conversions.
/// </summary>
public static class ConverterExtensions
{
    /// <summary>
    /// Configures the property to use a <see cref="JsonDocumentValueConverter"/> 
    /// for conversion.
    /// </summary>
    /// <param name="builder">The property builder to configure.</param>
    /// <returns>The configured property builder.</returns>
    public static PropertyBuilder<JsonDocument> HasJsonDocumentConversion(
        this PropertyBuilder<JsonDocument> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.HasConversion<JsonDocumentValueConverter>();
    }

    /// <summary>
    /// Configures the property to use a value comparer for <see cref="JsonDocument"/> instances.
    /// </summary>
    /// <remarks>This method sets a custom value comparer to handle comparisons of <see cref="JsonDocument"/>
    /// values, ensuring proper equality checks and change tracking for properties of this type.</remarks>
    /// <param name="builder">The <see cref="PropertyBuilder{TProperty}"/> for the property being configured.</param>
    public static void HasJsonDocumentComparer(
        this PropertyBuilder<JsonDocument> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Metadata.SetValueComparer(new JsonDocumentValueComparer());
    }


    /// <summary>
    /// Configures the property to use a <see cref="PrimitiveValueConverter{TPrimitive, TValue}"/> 
    /// for conversion.
    /// </summary>
    /// <typeparam name="TPrimitive">The primitive type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="builder">The property builder to configure.</param>
    /// <returns>The configured property builder.</returns>
    public static PropertyBuilder<TPrimitive> HasPrimitiveConversion<TPrimitive, TValue>(
        this PropertyBuilder<TPrimitive> builder)
        where TPrimitive : struct, IPrimitive<TPrimitive, TValue>
        where TValue : notnull
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.HasConversion<PrimitiveValueConverter<TPrimitive, TValue>>();
    }

    /// <summary>
    /// Configures the property to use a <see cref="ReadOnlyMemoryToByteArrayConverter"/> 
    /// for conversion.
    /// </summary>
    /// <param name="builder">The property builder to configure.</param>
    /// <returns>The configured property builder.</returns>
    public static PropertyBuilder<ReadOnlyMemory<byte>> HasReadOnlyMemoryToByteArrayConversion(
        this PropertyBuilder<ReadOnlyMemory<byte>> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.HasConversion<ReadOnlyMemoryToByteArrayConverter>();
    }
}
