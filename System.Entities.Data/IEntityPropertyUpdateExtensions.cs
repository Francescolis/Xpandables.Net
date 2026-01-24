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
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore.Query;

namespace System.Entities.Data;

/// <summary>
/// EF Core implementation of <see cref="IPropertyUpdateApplicator{TSource, TBuilder}"/>.
/// </summary>
internal sealed class EfCorePropertyUpdateApplicator<TSource>
    : IPropertyUpdateApplicator<TSource, UpdateSettersBuilder<TSource>>
    where TSource : class
{
    public static readonly EfCorePropertyUpdateApplicator<TSource> Instance = new();

    public void ApplyComputed<TProperty>(
        UpdateSettersBuilder<TSource> builder,
        Expression<Func<TSource, TProperty>> propertyExpression,
        Expression<Func<TSource, TProperty>> valueExpression)
        => builder.SetProperty(propertyExpression, valueExpression);

    public void ApplyConstant<TProperty>(
        UpdateSettersBuilder<TSource> builder,
        Expression<Func<TSource, TProperty>> propertyExpression,
        TProperty value)
        => builder.SetProperty(propertyExpression, value);
}

/// <summary>
/// Provides extension methods for converting collections of entity property updates
/// to the format required by EF Core's ExecuteUpdate APIs.
/// </summary>
/// <remarks>
/// This implementation is fully AOT-compliant as it avoids dynamic code generation
/// and MakeGenericMethod calls at runtime.
/// </remarks>
public static class IEntityPropertyUpdateExtensions
{
    /// <summary>
    /// <see cref="IEntityPropertyUpdate{TSource}"/> extensions.
    /// </summary>
    /// <typeparam name="TSource">The type of the entity being updated.</typeparam>
    extension<TSource>(IEnumerable<IEntityPropertyUpdate<TSource>> updates)
        where TSource : class
    {
        /// <summary>
        /// Builds an <see cref="Action{T}"/> that applies all configured property updates
        /// to a given <see cref="UpdateSettersBuilder{TSource}"/> instance.
        /// </summary>
        /// <remarks>
        /// This method is AOT-compliant and does not use dynamic code generation
        /// or MakeGenericMethod at runtime.
        /// </remarks>
        /// <returns>An action that applies all updates to the builder.</returns>
        public Action<UpdateSettersBuilder<TSource>> ToSetPropertyCalls()
        {
            ArgumentNullException.ThrowIfNull(updates);

            var list = updates as IReadOnlyList<IEntityPropertyUpdate<TSource>> ?? [.. updates];
            if (list.Count == 0)
            {
                return static _ => { };
            }

            // Capture list in closure - polymorphic dispatch handles type safety
            return builder =>
            {
                var applicator = EfCorePropertyUpdateApplicator<TSource>.Instance;
                foreach (var update in list)
                {
                    update.Apply(builder, applicator);
                }
            };
        }
    }
}
