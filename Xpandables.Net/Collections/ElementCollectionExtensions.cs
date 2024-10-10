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
namespace Xpandables.Net.Collections;
/// <summary>
/// Provides extension methods for converting collections of 
/// <see cref="ElementEntry"/> to <see cref="ElementCollection"/>.
/// </summary>
public static class ElementCollectionExtensions
{
    /// <summary>
    /// Converts an <see cref="IEnumerable{ElementEntry}"/> to 
    /// an <see cref="ElementCollection"/>.
    /// </summary>
    /// <param name="entries">The collection of <see cref="ElementEntry"/>
    /// to convert.</param>
    /// <returns>An <see cref="ElementCollection"/> containing the provided 
    /// entries.</returns>
    public static ElementCollection ToElementCollection(
        this IEnumerable<ElementEntry> entries)
        => ElementCollection.With(entries.ToArray());

    /// <summary>
    /// Converts an <see cref="ElementCollection"/> to a dictionary where the 
    /// keys are the element keys and the values are arrays of element values.
    /// </summary>
    /// <param name="elementCollection">The <see cref="ElementCollection"/> 
    /// to convert.</param>
    /// <returns>A dictionary with keys and values from the 
    /// <see cref="ElementCollection"/>.</returns>
    public static IDictionary<string, string[]> ToElementDictionary(
        this ElementCollection elementCollection)
        => elementCollection
            .ToDictionary(entry => entry.Key, entry => entry.Values.ToArray());
}
