
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
namespace Xpandables.Net.Visitors;

/// <summary>
/// Allows an application author to apply the visitor pattern.
/// </summary>
public interface IVisitor
{
    /// <summary>
    /// Gets the zero-base order in which the visitor will be applied.
    /// The default value is zero.
    /// </summary>
    public virtual int Order => 0;

    /// <summary>
    /// Declares a Visit operation.
    /// This method will do the actual job of visiting the specified element.
    /// </summary>
    /// <param name="element">Element to be visited. It must implement 
    /// the <see cref="IVisitable"/> or 
    /// <see cref="IVisitable{TVisitable}"/> interface.</param>
    /// <exception cref="ArgumentNullException">
    /// The <paramref name="element"/> is null.</exception>
    /// <exception cref="ArgumentException">The <paramref name="element"/> 
    /// must implement <see cref="IVisitable"/>.</exception>
    ValueTask VisitAsync(object element);
}

/// <summary>
/// Allows an application author to apply 
/// the visitor pattern : The generic Visitor definition.
/// The implementation must be thread-safe when 
/// working in a multi-threaded environment.
/// </summary>
/// <typeparam name="TElement">Type of element to be visited.</typeparam>
public interface IVisitor<in TElement> : IVisitor
    where TElement : notnull, IVisitable
{
    /// <summary>
    /// Declares a Visit operation.
    /// This method will do the actual job of visiting the specified element.
    /// </summary>
    /// <param name="element">Element to be visited.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="element"/> is null.</exception>
    ValueTask VisitAsync(TElement element);

    ValueTask IVisitor.VisitAsync(object element)
    {
        ArgumentNullException.ThrowIfNull(element);
        if (element is not TElement visitable)
            return ValueTask.CompletedTask;

        return VisitAsync(visitable);
    }
}