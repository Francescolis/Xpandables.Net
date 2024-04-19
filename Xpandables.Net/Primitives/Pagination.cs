
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
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Xpandables.Net.Primitives;

/// <summary>
/// Represents a pagination definition.
/// </summary>
[StructLayout(LayoutKind.Auto)]
[DebuggerDisplay("Index = {Index}, Size = {Size}")]
public readonly record struct Pagination
{
    /// <summary>
    /// Gets the page index.
    /// </summary>
    [Required]
    public required int Index { get; init; }

    /// <summary>
    /// Gets the page size.
    /// </summary>
    [Required]
    public required int Size { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Pagination"/> structure.
    /// </summary>
    /// <param name="index">The page index.</param>
    /// <param name="size">The page size.</param>
    [SetsRequiredMembers]
    public Pagination(int index, int size)
        => (Index, Size) = (index, size);

    /// <summary>
    /// Creates a new instance of <see cref="Pagination"/>.
    /// </summary>
    /// <param name="index">The page index.</param>
    /// <param name="size">The page size.</param>
    /// <returns>An instance of <see cref="Pagination"/> 
    /// with expected values.</returns>
    public static Pagination With(int index, int size) => new(index, size);

    /// <summary>
    /// Provides with a default instance of <see cref="Pagination"/> 
    /// that contains zero index and 50 size.
    /// </summary>
    /// <returns></returns>
    public static Pagination DefaultInstance() => With(0, 50);
}