
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
using Xpandables.Net.Aggregates;
using Xpandables.Net.Api.Domains.Events;
using Xpandables.Net.Api.Primitives;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Api.Domains;

public sealed class Person : Aggregate
{
    private readonly List<ContactId> _contactIds = [];
    public FirstName FirstName { get; private set; }
    public LastName LastName { get; private set; }
    public IReadOnlyCollection<ContactId> ContactIds => _contactIds.AsReadOnly();


    public static Person Create(PersonId personId, FirstName firstName, LastName lastName)
    {
        Person person = new();

        PersonCreateRequested requested = new()
        {
            AggregateId = personId,
            FirstName = firstName,
            LastName = lastName
        };

        person.PushEvent(requested);

        return person;
    }
    public IOperationResult BeContact(ContactId contactId)
    {
        if (_contactIds.Contains(contactId))
            return OperationResults
                .Conflict()
                .WithError(nameof(contactId), "Contact already exists")
                .Build();

        PushEvent(new BeContactRequested(this, contactId));

        return OperationResults
            .Ok()
            .Build();
    }

    private Person()
    {
        On<PersonCreateRequested>(@event =>
        {
            AggregateId = @event.AggregateId;
            FirstName = @event.FirstName;
            LastName = @event.LastName;
        });

        On<BeContactRequested>(@event =>
            _contactIds.Add(@event.ContactId));
    }
}
