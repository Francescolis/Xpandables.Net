
/************************************************************************************************************
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
************************************************************************************************************/
using System.Text.Json;

namespace Xpandables.Net.Primitives;

/// <summary>
/// Provides a set of static (Shared in Visual Basic) methods for <see cref="JsonSerializerOptions"/>.
/// </summary>
public static class JsonSerializerDefaultOptions
{
    /// <inheritdoc/>
    public static JsonSerializerOptions OptionPropertyNameCaseInsensitiveTrue => new() { PropertyNameCaseInsensitive = true };

    /// <inheritdoc/>
    public static JsonSerializerOptions OptionDefaultWeb => new(JsonSerializerDefaults.Web);

    /// <inheritdoc/>
    public static JsonSerializerOptions Options => new();
}