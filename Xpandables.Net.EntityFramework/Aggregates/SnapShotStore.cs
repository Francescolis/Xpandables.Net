﻿
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
using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Xpandables.Net.Aggregates.SnapShots;
using Xpandables.Net.Optionals;
using Xpandables.Net.Primitives.I18n;
using Xpandables.Net.Primitives.Text;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// <see cref="ISnapShotStore"/> implementation.
/// </summary>
///<inheritdoc/>
public sealed class SnapShotStore(
    DataContextDomain dataContext,
    JsonSerializerOptions serializerOptions) : ISnapShotStore
{
    ///<inheritdoc/>
    public async ValueTask AppendAsync(
        SnapShotDescriptor descriptor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        EntitySnapShot entity = EntitySnapShot
                .ToEntitySnapShot(descriptor, serializerOptions);
        _ = await dataContext.SnapShots
            .AddAsync(entity, cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public async ValueTask<Optional<T>> ReadAsync<T>(
        Guid objectId,
        CancellationToken cancellationToken = default)
        where T : class, IOriginator
    {
        ArgumentNullException.ThrowIfNull(objectId);

        T instance = (T?)Activator.CreateInstance(typeof(T), true)
            ?? throw new InvalidOperationException(
                I18nXpandables.AggregateFailedToCreateInstance
                    .StringFormat(typeof(T).GetNameWithoutGenericArity()));

        string objectTypeName = instance.GetTypeName();
        using EntitySnapShot? entity = await dataContext.SnapShots
            .AsNoTracking()
            .Where(x => x.ObjectId == objectId
                && x.ObjectTypeName == objectTypeName)
            .OrderByDescending(o => o.Version)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            return Optional.Empty<T>();

        if (EntitySnapShot
            .ToMemento(entity, serializerOptions) is not IMemento memento)
            return Optional.Empty<T>();

        instance.SetMemento(memento);
        return instance;
    }
}