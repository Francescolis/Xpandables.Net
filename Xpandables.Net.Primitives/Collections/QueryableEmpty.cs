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
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Xpandables.Net.Primitives.Collections;

/// <summary>
/// Represents an empty, read-only implementation of the <see cref="IQueryable{T}"/> interface.
/// </summary>
/// <remarks>This class provides a convenient way to represent an empty queryable sequence of a specified type.
/// All enumeration and query operations on instances of this type will yield no results. It is useful in scenarios
/// where an empty <see cref="IQueryable{T}"/> is required, such as default return values or placeholder query sources.</remarks>
/// <typeparam name="T">The type of the elements in the queryable sequence.</typeparam>
public sealed class QueryableEmpty<T> : IQueryable<T>
{
    private readonly List<T> _empty = [];

    /// <summary>
    /// Gets the type of the elements contained in the sequence.
    /// </summary>
    public Type ElementType => typeof(T);

    /// <summary>
    /// Gets an expression that represents the current instance as a constant value.
    /// </summary>
    public Expression Expression => Expression.Constant(this);

    /// <summary>
    /// Gets the query provider that is associated with this data source.
    /// </summary>
    /// <remarks>The query provider enables the construction and execution of queries against the underlying
    /// data source. This property is typically used when implementing LINQ query operations.</remarks>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public IQueryProvider Provider => new EnumerableQuery<T>(_empty);

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<T> GetEnumerator() => _empty.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _empty.GetEnumerator();
}
