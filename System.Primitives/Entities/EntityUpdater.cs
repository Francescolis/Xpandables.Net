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
using System.Linq.Expressions;

namespace System.Entities;

/// <summary>
/// Provides static methods for creating entity updaters that specify property updates using either computed or constant
/// values.
/// </summary>
/// <remarks>EntityUpdater is typically used in scenarios where you need to construct update operations for
/// entities in a type-safe manner, such as building update expressions for data access layers or ORMs. The methods
/// allow you to define which properties to update and how their new values are determined, supporting both constant
/// assignments and computed expressions. All methods require non-null arguments and will throw an ArgumentNullException
/// if any required parameter is null.</remarks>
public abstract class EntityUpdater
{
    /// <summary>
    /// Creates a new updater with a property set to a computed value.
    /// <example>
    /// <code>
    /// var updater = Updater&lt;User&gt;.SetProperty(u =&gt; u.LastUpdated, u =&gt; DateTime.UtcNow)
    ///                              .SetProperty(u =&gt; u.Status, "Active");
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="TSource">The type of the entity being updated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being updated.</typeparam>
    /// <param name="propertyExpression">An expression that selects the property to update.</param>
    /// <param name="valueExpression">An expression that computes the new value for the property.</param>
    /// <returns>A new <see cref="EntityUpdater{TSource}"/> instance with the specified property update.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public static EntityUpdater<TSource> SetProperty<TSource, TProperty>(
        Expression<Func<TSource, TProperty>> propertyExpression,
        Expression<Func<TSource, TProperty>> valueExpression)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(propertyExpression);
        ArgumentNullException.ThrowIfNull(valueExpression);

        EntityUpdater<TSource> updater = new();
        return updater.SetProperty(propertyExpression, valueExpression);
    }

    /// <summary>
    /// Creates a new updater with a property set to a constant value.
    /// <example>
    /// <code>
    /// var updater = Updater&lt;User&gt;.SetProperty(u =&gt; u.Status, "Active")
    ///                              .SetProperty(u =&gt; u.IsEnabled, true);
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="TSource">The type of the entity being updated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being updated.</typeparam>
    /// <param name="propertyExpression">An expression that selects the property to update.</param>
    /// <param name="value">The constant value to set for the property.</param>
    /// <returns>A new <see cref="EntityUpdater{TSource}"/> instance with the specified property update.</returns>
    /// <exception cref="ArgumentNullException">Thrown when propertyExpression is null.</exception>
    public static EntityUpdater<TSource> SetProperty<TSource, TProperty>(
        Expression<Func<TSource, TProperty>> propertyExpression,
        TProperty value)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(propertyExpression);

        EntityUpdater<TSource> updater = new();
        return updater.SetProperty(propertyExpression, value);
    }

}

/// <summary>
/// Provides a fluent API for building update operations on entities.
/// </summary>
/// <typeparam name="TSource">The type of the entity being updated.</typeparam>
/// <remarks>This class allows for the construction of multiple property updates in a fluent manner,
/// which can then be used with bulk update operations. It maintains a collection of property-value
/// expressions that can be applied to database update operations.</remarks>
public sealed class EntityUpdater<TSource> : EntityUpdater
    where TSource : class
{
    private readonly List<IEntityPropertyUpdate<TSource>> _updates;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityUpdater{TSource}"/> class.
    /// </summary>
    public EntityUpdater()
    {
        _updates = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityUpdater{TSource}"/> class with an initial property update.
    /// </summary>
    /// <param name="update">The initial property update expression.</param>
    public EntityUpdater(IEntityPropertyUpdate<TSource> update)
    {
        ArgumentNullException.ThrowIfNull(update);
        _updates = [update];
    }

    /// <summary>
    /// Gets the collection of property update expressions.
    /// </summary>
    /// <remarks>This property is used internally by repository implementations to access the
    /// accumulated property updates for bulk operations.</remarks>
    public IReadOnlyList<IEntityPropertyUpdate<TSource>> Updates => _updates.AsReadOnly();

    /// <summary>
    /// Adds another property update with a computed value to the current updater.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being updated.</typeparam>
    /// <param name="propertyExpression">An expression that selects the property to update.</param>
    /// <param name="valueExpression">An expression that computes the new value for the property.</param>
    /// <returns>The same <see cref="EntityUpdater{TSource}"/> instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public EntityUpdater<TSource> SetProperty<TProperty>(
        Expression<Func<TSource, TProperty>> propertyExpression,
        Expression<Func<TSource, TProperty>> valueExpression)
    {
        ArgumentNullException.ThrowIfNull(propertyExpression);
        ArgumentNullException.ThrowIfNull(valueExpression);

        var update = new PropertyUpdate<TSource, TProperty>(propertyExpression, valueExpression);
        _updates.Add(update);

        return this;
    }

    /// <summary>
    /// Adds another property update with a constant value to the current updater.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being updated.</typeparam>
    /// <param name="propertyExpression">An expression that selects the property to update.</param>
    /// <param name="value">The constant value to set for the property.</param>
    /// <returns>The same <see cref="EntityUpdater{TSource}"/> instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when propertyExpression is null.</exception>
    public EntityUpdater<TSource> SetProperty<TProperty>(
        Expression<Func<TSource, TProperty>> propertyExpression,
        TProperty value)
    {
        ArgumentNullException.ThrowIfNull(propertyExpression);

        var constantExpression = Expression.Constant(value, typeof(TProperty));
        var update = new PropertyUpdate<TSource, TProperty>(propertyExpression, constantExpression);
        _updates.Add(update);

        return this;
    }
}
