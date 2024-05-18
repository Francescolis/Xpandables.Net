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

namespace Xpandables.Net.Aspects;

/// <summary>
/// Represents a marker interface that allows the class implementation to be 
/// recognized as an aspect visitor.
/// </summary>
public interface IAspectVisitor : IAspect
{
    /// <summary>
    /// Declares a Visit operation.
    /// This method will do the actual job of visiting the specified element.
    /// </summary>
    /// <param name="element">Element to be visited. It must implement 
    /// the <see cref="IAspectVisitable"/> or 
    /// <see cref="IAspectVisitable{TVisitable}"/> interface.</param>
    /// <exception cref="ArgumentNullException">
    /// The <paramref name="element"/> is null.</exception>
    /// <exception cref="ArgumentException">The <paramref name="element"/> 
    /// must implement <see cref="IAspectVisitable"/>.</exception>
    void Visit(object element);
}

/// <summary>
/// Represents a marker interface that allows the class implementation to be 
/// recognized as an aspect visitor.
/// </summary>
public interface IAspectVisitor<in TElement> : IAspectVisitor
    where TElement : class, IAspectVisitable
{
    /// <summary>
    /// Declares a Visit operation.
    /// This method will do the actual job of visiting the specified element.
    /// </summary>
    /// <param name="element">Element to be visited.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="element"/> is null.</exception>
    void Visit(TElement element);

    void IAspectVisitor.Visit(object element)
    {
        ArgumentNullException.ThrowIfNull(element);

        if (element is not TElement visitable)
            return;

        Visit(visitable);
    }
}


/// <summary>
/// Represents a marker interface that allows the class implementation to be 
/// recognized as an aspect visitable.
/// Defines an Accept operation that takes a visitor as an argument.
/// Visitor design pattern allows you to add new behaviors 
/// to an existing object without changing the object structure.
/// </summary>
public interface IAspectVisitable
{
    /// <summary>
    /// Defines the Accept operation.
    /// When overridden in derived class, this method will 
    /// accept the specified visitor.
    /// The default behavior just call the visit method of 
    /// the visitor on the current instance.
    /// </summary>
    /// <param name="visitor">The visitor to be applied on.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="visitor"/> is null.</exception>
    public virtual void Accept(IAspectVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);

        visitor.Visit(this);
    }
}

/// <summary>
/// Defines an Accept operation that takes a visitor as an argument.
/// Visitor design pattern allows you to add new behaviors 
/// to an existing object without changing the object structure.
/// The implementation must be thread-safe when working 
/// in a multi-threaded environment.
/// </summary>
/// <typeparam name="TVisitable">The type of the object to act on.</typeparam>
public interface IAspectVisitable<out TVisitable> : IAspectVisitable
    where TVisitable : class, IAspectVisitable
{
    /// <summary>
    /// Defines the Accept operation.
    /// When overridden in derived class, this method 
    /// will accept the specified visitor.
    /// The default behavior just call the visit method 
    /// of the visitor on the current instance.
    /// </summary>
    /// <param name="visitor">The visitor to be applied on.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="visitor"/> is null.</exception>
    public virtual void Accept(
        IAspectVisitor<TVisitable> visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);

        visitor.Visit((TVisitable)this);
    }
}