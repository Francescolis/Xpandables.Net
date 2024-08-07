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
using System.Text.Json.Serialization;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Aggregates;
using Xpandables.Net.Commands;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Events;
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;
using Xpandables.Net.Primitives.Collections;
using Xpandables.Net.Primitives.Converters;

namespace Xpandables.Net.UnitTests;
public sealed class AggregateUnitTest
{
    [Theory]
    [InlineData("FirstName", "LastName")]
    public async Task CreatePersonAndBeContactAsync(
        string firstName, string lastName)
    {
        IServiceCollection serviceDescriptors = new ServiceCollection()
            .AddLogging()
            .Configure<EventOptions>(EventOptions.Default)
            .AddXAggregateCommandHandlers()
            .AddXCommandQueryHandlers(options =>
                options
                .UseOperationFinalizer())
            .AddXEventHandlers()
            .AddXDispatcher()
            .AddXAggregateAccessor()
            .AddXRepositoryEvent<RepositoryPerson>()
            .AddXOperationResultFinalizer()
            .AddXEventPublisher()
            .AddXEventDuplicateDecorator()
            .AddXEventStore()
            .AddXAggregateCommandDecorator();

        IServiceProvider serviceProvider = serviceDescriptors
            .BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = false,
                ValidateScopes = false
            });

        // person id
        Guid personId = Guid.NewGuid();

        // get the dispatcher
        IDispatcher dispatcher = serviceProvider
            .GetRequiredService<IDispatcher>();

        // create person
        CreatePersonRequestCommand createCommand =
            new(personId, firstName, lastName);

        IOperationResult createResult = await dispatcher
            .SendAsync(createCommand);

        createResult.StatusCode
            .Should()
            .Be(System.Net.HttpStatusCode.OK);

        createResult.IsSuccess
            .Should()
            .BeTrue();

        createResult.Headers
            .Should()
            .Contain(x => x.Key == nameof(PersonId));

        Guid guid = Guid.Parse(
            createResult.Headers[nameof(PersonId)]!.Value.Values.First());

        // unable to create the same person
        createResult = await dispatcher.SendAsync(createCommand);

        createResult.StatusCode
            .Should()
            .Be(System.Net.HttpStatusCode.Conflict);

        createResult.IsSuccess
            .Should()
            .BeFalse();

        // request contact
        SendContactRequestCommand contactCommand =
            new(guid, Guid.NewGuid());

        IOperationResult contactResult = await dispatcher
            .SendAsync(contactCommand);

        contactResult.StatusCode
            .Should()
            .Be(System.Net.HttpStatusCode.OK);

        contactResult.IsSuccess
            .Should()
            .BeTrue();

        // unable to request the same contact twice
        contactResult = await dispatcher
            .SendAsync(contactCommand);

        contactResult.StatusCode
            .Should()
            .Be(System.Net.HttpStatusCode.Conflict);

        contactResult.IsSuccess
            .Should()
            .BeFalse();

        // read person
        IAggregateAccessor<Person> store = serviceProvider
            .GetRequiredService<IAggregateAccessor<Person>>();

        IOperationResult<Person> personResult = await store
            .PeekAsync(PersonId.Create(guid));

        Assert.True(personResult.IsSuccess);
        Person person = personResult.Result;

        person.ContactIds
            .Should()
            .HaveCount(1);
    }
}

public sealed record CreatePersonRequestCommand(
    Guid AggregateId,
    string FirstName,
    string LastName) : Command<Person>(AggregateId)
{
    public override bool ContinueWhenNotFound => true;
}

public sealed class CreatePersonRequestCommandHandler :
    ICommandHandler<CreatePersonRequestCommand, Person>
{
    public Task<IOperationResult> HandleAsync(
        CreatePersonRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Aggregate.IsNotEmpty)
        {
            return Task.FromResult(OperationResults
                .Conflict()
                .WithError(nameof(PersonId), "Person already exist")
                .Build());
        }

        command.Aggregate = Person
            .Create(command.KeyId, command.FirstName, command.LastName);

        return Task.FromResult(OperationResults
            .Ok()
            .WithHeader(nameof(PersonId), command.KeyId.ToString())
            .Build());
    }
}

public sealed record SendContactRequestCommand(
    Guid AggregateId, Guid ReceiverId) : Command<Person>(AggregateId);

public sealed class SendContactRequestAggregateCommandHandler :
    ICommandHandler<SendContactRequestCommand, Person>
{
    public Task<IOperationResult> HandleAsync(
        SendContactRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        Assert.True(command.Aggregate.IsNotEmpty);

        ContactId receivedId = new(command.ReceiverId);
        IOperationResult result = command.Aggregate.Value.BeContact(receivedId);

        return Task.FromResult(result);
    }
}

public sealed class ContactCreatedDomainEventHandler
    (IEventStore eventStore)
    : IEventHandler<PersonCreatedDomainEvent>
{
    public async Task<IOperationResult> HandleAsync(
        PersonCreatedDomainEvent @event,
        CancellationToken cancellationToken = default)
    {
        PersonCreatedEventIntegration integrationEvent =
            new(@event.AggregateId, @event.FirstName, @event.LastName);

        return await eventStore
            .AppendAsync(integrationEvent, cancellationToken)
            .ToOperationResultAsync();
    }
}

public sealed class ContactRequestSentDomainEventHandler
    (IEventStore eventStore)
    : IEventHandler<ContactRequestSentDomainEvent>
{
    public async Task<IOperationResult> HandleAsync(
        ContactRequestSentDomainEvent @event,
        CancellationToken cancellationToken = default)
    {
        ContactRequestSentEventIntegration integration =
            new(@event.AggregateId, @event.FullName, @event.ContactId.Value);

        return await eventStore
            .AppendAsync(integration, cancellationToken)
            .ToOperationResultAsync();
    }
}

public sealed record PersonCreatedDomainEvent :
    EventDomain<Person>, IEventDuplicate
{
    [JsonConstructor]
    private PersonCreatedDomainEvent() { }
    public PersonCreatedDomainEvent(
        Person person, string firstName, string lastName)
        : base(person)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;

    public IEventFilter? Filter() => new EntityEventDomainFilter
    {
        EventTypeName = nameof(PersonCreatedDomainEvent),
        Paging = Pagination.With(0, 1),
        DataCriteria = x
            => x.RootElement
                    .GetProperty(nameof(FirstName))
                    .GetString() == FirstName ||
                x.RootElement
                .GetProperty(nameof(LastName))
                    .GetString() == LastName

    };
    public IOperationResult OnFailure() => OperationResults
            .Conflict()
            .WithError(nameof(FirstName), "Duplicate found")
            .WithError(nameof(LastName), "Duplicate found")
            .Build();
}

public sealed record ContactRequestSentDomainEvent :
    EventDomain<Person>, IEventDuplicate
{
    [JsonConstructor]
    private ContactRequestSentDomainEvent() { }

    public ContactRequestSentDomainEvent(
        Person person, ContactId contactId) : base(person)
    {
        FullName = $"{person.FirstName} {person.LastName}";
        ContactId = contactId;
    }

    public string FullName { get; init; } = default!;
    public ContactId ContactId { get; init; } = default!;

    public IEventFilter? Filter() => new EntityEventDomainFilter
    {
        EventTypeName = nameof(ContactRequestSentDomainEvent),
        Paging = Pagination.With(0, 1),
        DataCriteria = x
            => x.RootElement
                    .GetProperty(nameof(FullName))
                    .GetString() == FullName ||
                x.RootElement
                .GetProperty(nameof(ContactId))
                    .GetGuid() == ContactId.Value
    };

    public IOperationResult OnFailure() => OperationResults
            .Conflict()
            .WithError(nameof(FullName), "Duplicate found")
            .WithError(nameof(ContactId), "Duplicate found")
            .Build();
}

public sealed record PersonCreatedEventIntegration(
    Guid PersonId,
    string FirstName,
    string LastName) : EventIntegration;

public sealed record ContactRequestSentEventIntegration(
    Guid SenderId,
    string SenderName,
    Guid ReceiverId) : EventIntegration;

public sealed class Person : Aggregate, ITransactionDecorator
{
    private readonly List<ContactId> _contactIds = [];
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public IReadOnlyCollection<ContactId> ContactIds => _contactIds.AsReadOnly();
    public static Person Create(PersonId id, string firstName, string lastName)
    {
        Person person = new() { AggregateId = id };
        PersonCreatedDomainEvent @event = new(person, firstName, lastName);
        person.PushEvent(@event);
        return person;
    }

    public IOperationResult BeContact(ContactId contactId)
    {
        ContactRequestSentDomainEvent @event = new(this, contactId);
        PushEvent(@event);

        return OperationResults
            .Ok()
            .Build();
    }

    private Person()
    {
        On<PersonCreatedDomainEvent>(@event =>
        {
            FirstName = @event.FirstName;
            LastName = @event.LastName;
        });

        On<ContactRequestSentDomainEvent>(@event
            => _contactIds.Add(@event.ContactId));
    }
}

[PrimitiveJsonConverter]
public readonly record struct PersonId(Guid Value) : IAggregateId<PersonId>
{
    public static PersonId Create(Guid value) => new(value);
    public static PersonId Default() => Create(Guid.Empty);

    public static implicit operator Guid(PersonId self) => self.Value;

    public static implicit operator string(PersonId self)
        => self.Value.ToString();
    public static implicit operator PersonId(Guid value) => new(value);
}

[PrimitiveJsonConverter]
public readonly record struct ContactId(Guid Value) :
    IPrimitive<ContactId, Guid>
{
    public static ContactId Create(Guid value) => new(value);
    public static ContactId Default() => Create(Guid.Empty);

    public static implicit operator Guid(ContactId self) => self.Value;

    public static implicit operator string(ContactId self)
        => self.Value.ToString();
    public static implicit operator ContactId(Guid value) => new(value);

}

public sealed class RepositoryPerson : IRepositoryEvent
{
    private static readonly HashSet<IEntityEvent> _store = [];
    private static readonly HashSet<IEntityEvent> _events = [];

    public Task InsertAsync(
        IEntityEvent entity,
        CancellationToken cancellationToken = default)
    {
        _events.Add(entity);
        return Task.CompletedTask;
    }

    public IAsyncEnumerable<IEntityEvent> FetchAsync(
        IEventFilter eventFilter,
        CancellationToken cancellationToken = default)
        => eventFilter
            .ApplyQueryable(_store.AsQueryable());

    public Task MarkEventsAsPublishedAsync(
        Guid eventId,
        Exception? exception = null,
        CancellationToken cancellationToken = default)
    {
        IEntityEventIntegration? entity = _store
            .OfType<IEntityEventIntegration>()
            .FirstOrDefault(x => x.Id == eventId);

        if (entity is not null)
        {
            entity.ErrorMessage = exception?.ToString();
        }

        return Task.CompletedTask;
    }
    public Task PersistAsync(
        CancellationToken cancellationToken = default)
    {
        _events.ForEach(e => _store.Add(e));
        _events.Clear();
        return Task.CompletedTask;
    }
}