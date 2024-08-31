
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

using Xpandables.Net.Api.Persons.Persistence;
using Xpandables.Net.Distribution;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Api.Persons.Domains.Events;

public sealed class BeContactCompletedHandler(DatabasePerson database) :
    IEventHandler<BeContactCompleted>
{
    public async Task<IOperationResult> HandleAsync(
        BeContactCompleted @event,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        EntityPerson? entity = database.FindById(@event.PersonId);

        if (entity is not null)
        {
            EntityPerson? contact = database.FindById(@event.ContactId);
            if (contact is not null)
            {
                entity.Contacts.Add(contact);
                return OperationResults
                    .Ok()
                    .Build();
            }
        }

        return OperationResults
            .BadRequest()
            .WithError(nameof(@event.ContactId), "Person or Contact not found")
            .WithError(nameof(@event.PersonId), "Person or Contact not found")
            .Build();
    }
}
