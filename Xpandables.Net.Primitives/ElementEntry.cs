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
namespace Xpandables.Net.Primitives;

/// <summary>
/// An element for a collection.
/// </summary>
/// <param name="Key">The key associated with the element.</param>
/// <param name="Values">The collection of strings that represents the values.</param>
/// <exception cref="ArgumentNullException">The <paramref name="Key"/> is null.</exception>
/// <exception cref="ArgumentNullException">The <paramref name="Values"/> is null.</exception>
public readonly record struct ElementEntry(string Key, IReadOnlyCollection<string> Values)
{
    /// <summary>
    /// Defines the error key for errors without key.
    /// </summary>
    public const string UndefinedKey = "UndefinedKey";

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementEntry"/> with the specified key and the values.
    /// </summary>
    /// <param name="key">The key associated with the key.</param>
    /// <param name="values">The string that represents the values.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="values"/> is empty.</exception>
    public ElementEntry(string key, params string[] values)
        : this(
              key ?? throw new ArgumentNullException(nameof(key)),
              (values?.Length ?? 0) == 0
              ? throw new ArgumentOutOfRangeException(nameof(values), $"{nameof(values)} can not be empty.")
              : values!.ToList())
    { }
}
