
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

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace System.Entities.Data.Converters;

/// <summary>
/// Converts a <see cref="JsonDocument"/> to a <see cref="string"/> and vice versa.
/// </summary>
public sealed class JsonDocumentValueConverter : ValueConverter<JsonDocument, string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonDocumentValueConverter"/> class.
    /// </summary>
    public JsonDocumentValueConverter() :
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
public sealed class JsonDocumentValueComparer : ValueComparer<JsonDocument>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonDocumentValueComparer"/> class.
    /// </summary>
    public JsonDocumentValueComparer() :
        base(
        (d1, d2) => ReferenceEquals(d1, d2) ||
            (d1 != null && d2 != null
            && d1.RootElement.GetRawText() == d2.RootElement.GetRawText()),
        d => d == null ? 0 : d.RootElement.GetRawText().GetHashCode(),
        d => d == null ? JsonDocument.Parse("{}", default) : JsonDocument.Parse(d.RootElement.GetRawText(), default))
    { }
}