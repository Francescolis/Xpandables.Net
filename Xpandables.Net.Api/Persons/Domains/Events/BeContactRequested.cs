
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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Xpandables.Net.Aggregates;
using Xpandables.Net.Decorators;
using Xpandables.Net.Events;
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Api.Persons.Domains.Events;

public sealed record BeContactRequested : EventDomain<Person>, IEventDuplicateDecorator
{
    [JsonConstructor]
    private BeContactRequested() { }

    [SetsRequiredMembers]
    public BeContactRequested(Person person, ContactId contactId) : base(person)
        => ContactId = contactId;
    public required ContactId ContactId { get; init; }

    public IEventFilter Filter() => new EntityEventDomainFilter
    {
        EventTypeName = nameof(BeContactRequested),
        KeyId = AggregateId,
        Paging = Pagination.With(0, 1),
        DataCriteria = x
            => x.RootElement
                .GetProperty(nameof(ContactId))
                    .GetGuid() == ContactId.Value
    };

    public IOperationResult OnFailure() => OperationResults
            .Conflict()
            .WithError(nameof(ContactId), "Can not be contact more than once")
            .Build();
}
