
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

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Xpandables.Net.Events.Converters;

/// <summary>
/// Converts a <see cref="JsonDocument"/> to a <see cref="string"/> and vice versa.
/// </summary>
public sealed class JsonDocumentConverter : ValueConverter<JsonDocument, string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonDocumentConverter"/> class.
    /// </summary>
    public JsonDocumentConverter() :
        base(
        jsonDocument => jsonDocument.RootElement.GetRawText(),
        json => JsonDocument.Parse(json, default))
    { }
}
