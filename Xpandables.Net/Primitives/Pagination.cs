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
using System.Runtime.InteropServices;

namespace Xpandables.Net.Primitives;

/// <summary>
/// Contains the pagination definition.
/// </summary>
/// <param name="Index">The page index.</param>
/// <param name="Size">The page size.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct Pagination(int Index, int Size)
{
    /// <summary>
    /// Creates a new instance of <see cref="Pagination"/>.
    /// </summary>
    /// <param name="index">The page index.</param>
    /// <param name="size">The page size.</param>
    /// <returns>An instance of <see cref="Pagination"/> with expected values.</returns>
    public static Pagination With(int index, int size) => new(index, size);

    /// <summary>
    /// Provides with a default instance of <see cref="Pagination"/> 
    /// that contains zero index and 50 size.
    /// </summary>
    /// <returns></returns>
    public static Pagination DefaultInstance() => With(0, 50);
}