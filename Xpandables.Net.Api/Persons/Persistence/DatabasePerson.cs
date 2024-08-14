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
namespace Xpandables.Net.Api.Persons.Persistence;

public sealed class DatabasePerson
{
    private static readonly HashSet<EntityPerson> _store = [];

    public Task InsertAsync(EntityPerson entity)
    {
        _store.Add(entity);
        return Task.CompletedTask;
    }

    public EntityPerson? FindById(Guid keyId)
        => _store.FirstOrDefault(x => x.KeyId == keyId);

    public Task RemoveAsync(Guid keyId)
    {
        var entity = FindById(keyId);
        if (entity is not null)
        {
            _store.Remove(entity);
        }
        return Task.CompletedTask;
    }
}
