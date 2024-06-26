﻿/*******************************************************************************
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
namespace Xpandables.Net.Primitives;

/// <summary>
/// Provides a set of static methods for <see cref="ElementCollection"/>
/// </summary>
public static class ElementCollectionExtensions
{
    /// <summary>
    /// Converts the enumerable collection of <see cref="ElementEntry"/>s 
    /// to <see cref="ElementCollection"/>.
    /// </summary>
    /// <param name="entries">The source collection.</param>
    /// <returns>A <see cref="ElementCollection"/> that contains values 
    /// of <see cref="ElementEntry"/> selected from the collection.</returns>
    public static ElementCollection ToElementCollection(
        this IEnumerable<ElementEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        return ElementCollection.With(entries.ToArray());
    }
}
