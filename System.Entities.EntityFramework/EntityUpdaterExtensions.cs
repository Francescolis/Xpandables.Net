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

using Microsoft.EntityFrameworkCore.Query;

namespace System.Entities.EntityFramework;

/// <summary>
/// Provides extension methods for the EntityUpdater type to facilitate dynamic property update operations.
/// </summary>
/// <remarks>Updated for EF Core 10 : returns an <see cref="Action{T}"/> over <see cref="UpdateSettersBuilder{TSource}"/>,
/// which is what ExecuteUpdate/ExecuteUpdateAsync expects now.</remarks>
public static class EntityUpdaterExtensions
{
    /// <summary>
    /// Builds an update setters action for EF Core 10 RC1 ExecuteUpdate APIs.
    /// </summary>
    /// <typeparam name="TSource">The type of the entity being updated.</typeparam>
    /// <param name="updater">The entity updater containing the updates to apply.</param>
    /// <returns>An action that applies all configured property updates to a <see cref="UpdateSettersBuilder{TSource}"/>.</returns>
    public static Action<UpdateSettersBuilder<TSource>> ToSetPropertyCalls<TSource>(this EntityUpdater<TSource> updater)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(updater);
        return updater.Updates.ToSetPropertyCalls();
    }
}
