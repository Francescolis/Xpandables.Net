
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
namespace Xpandables.Net.Visitors;

/// <summary>
/// The composite visitor used to wrap all visitors for a specific visitable type.
/// </summary>
/// <typeparam name="TElement">Type of the element to be visited</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="CompositeVisitor{TElement}"/> 
/// class with a collection of visitors.
/// </remarks>
/// <param name="visitors">The collection of visitors for a specific type.</param>
public sealed class CompositeVisitor<TElement>(IEnumerable<IVisitor<TElement>> visitors)
    : ICompositeVisitor<TElement>
    where TElement : notnull, IVisitable
{
    private readonly IEnumerable<IVisitor<TElement>> _visitorInstances = visitors;

    /// <summary>
    /// Asynchronously applies all found visitors to the element according to the visitor order.
    /// </summary>
    /// <param name="element">The element to be visited.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="element"/> is null.</exception>
    public async ValueTask VisitAsync(TElement element)
    {
        ArgumentNullException.ThrowIfNull(element);

        foreach (var visitor in _visitorInstances.OrderBy(o => o.Order))
        {
            await visitor.VisitAsync(element).ConfigureAwait(false);
        }
    }
}
