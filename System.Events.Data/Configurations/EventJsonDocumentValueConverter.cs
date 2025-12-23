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
using System.Text.Json;

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace System.Events.Data.Configurations;

/// <summary>
/// Converts a <see cref="JsonDocument"/> to a <see cref="string"/> and vice versa.
/// </summary>
public sealed class EventJsonDocumentValueConverter : ValueConverter<JsonDocument, string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventJsonDocumentValueConverter"/> class.
    /// </summary>
    public EventJsonDocumentValueConverter() :
        base(
        jsonDocument => jsonDocument == null ? "" : jsonDocument.RootElement.GetRawText(),
        json => string.IsNullOrWhiteSpace(json) ? JsonDocument.Parse("{}", default) : JsonDocument.Parse(json, default))
    { }
}

/// <summary>
/// Provides a comparer for <see cref="JsonDocument"/> instances that compares their contents based on the raw JSON text
/// of their root elements.
/// </summary>
/// <remarks>This comparer determines equality by comparing the raw JSON text of the root elements of two <see
/// cref="JsonDocument"/> instances. It also provides a hash code based on the raw JSON text of the root element and
/// supports creating a deep copy of a <see cref="JsonDocument"/> instance.</remarks>
public sealed class EventJsonDocumentValueComparer : ValueComparer<JsonDocument>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventJsonDocumentValueComparer"/> class.
    /// </summary>
    public EventJsonDocumentValueComparer() :
        base(
        (d1, d2) => ReferenceEquals(d1, d2) ||
            (d1 != null && d2 != null
            && d1.RootElement.GetRawText() == d2.RootElement.GetRawText()),
        d => d == null ? 0 : d.RootElement.GetRawText().GetHashCode(),
        d => d == null ? JsonDocument.Parse("{}", default) : JsonDocument.Parse(d.RootElement.GetRawText(), default))
    { }
}

/// <summary>
/// Provides extension methods for configuring Entity Framework Core property mappings for properties of type
/// JsonDocument used in event storage scenarios.
/// </summary>
/// <remarks>These extension methods enable correct value conversion and comparison for JsonDocument properties
/// when using Entity Framework Core, ensuring that such properties are persisted and tracked accurately. Use these
/// methods when mapping event payloads or metadata represented as JsonDocument to database columns.</remarks>
public static class EventJsonDocumentExtensions
{
    /// <summary>
    /// Configures the property to use a <see cref="EventJsonDocumentValueConverter"/> 
    /// for conversion.
    /// </summary>
    /// <param name="builder">The property builder to configure.</param>
    /// <returns>The configured property builder.</returns>
    public static PropertyBuilder<JsonDocument> HasEventJsonDocumentConversion(
        this PropertyBuilder<JsonDocument> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.HasConversion<EventJsonDocumentValueConverter>();
    }

    /// <summary>
    /// Configures the property to use a value comparer for <see cref="JsonDocument"/> instances.
    /// </summary>
    /// <remarks>This method sets a custom value comparer to handle comparisons of <see cref="JsonDocument"/>
    /// values, ensuring proper equality checks and change tracking for properties of this type.</remarks>
    /// <param name="builder">The <see cref="PropertyBuilder{TProperty}"/> for the property being configured.</param>
    public static void HasEventJsonDocumentComparer(
        this PropertyBuilder<JsonDocument> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Metadata.SetValueComparer(new EventJsonDocumentValueComparer());
    }
}