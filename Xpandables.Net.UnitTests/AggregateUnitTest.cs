﻿
/************************************************************************************************************
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
************************************************************************************************************/
using System.Text.Json.Serialization;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Aggregates;
using Xpandables.Net.Aggregates.Defaults;
using Xpandables.Net.Aggregates.DomainEvents;
using Xpandables.Net.Aggregates.IntegrationEvents;
using Xpandables.Net.Collections;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Operations;
using Xpandables.Net.Operations.Messaging;
using Xpandables.Net.Primitives;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.UnitTests;
public sealed class AggregateUnitTest
{
    [Theory]
    [InlineData("FirstName", "LastName")]
    public async Task CreatePersonAndBeContactAsync(string firstName, string lastName)
    {
        IServiceProvider serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddXHandlers(options => options.UsePersistence().UseOperationResultContext())
            .AddXDispatcher()
            .AddXPersistenceCommandHandler(_ => ct => ValueTask.FromResult(OperationResults.Ok().Build()))
            .AddXAggregateStoreDefault()
            .AddXOperationResultContext()
            .AddXTransientPublisher()
            .AddXIntegrationEventSourcing()
            .AddXIntegrationEventOutbox()
            .AddXDomainEventStore<DomainEventRecord, EventStoreTest>()
            .AddXIntegrationEventStore<NotificationStoreText>()
            .BuildServiceProvider();

        // person id
        Guid personId = Guid.NewGuid();

        // get the dispatcher
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();

        // create person
        var createCommand = new CreatePersonRequestCommand(personId, firstName, lastName);
        OperationResult createResult = await dispatcher.SendAsync(createCommand);

        createResult.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        createResult.IsSuccess.Should().BeTrue();
        createResult.Headers.Should().Contain(x => x.Key == nameof(PersonId));

        Guid guid = Guid.Parse(createResult.Headers[nameof(PersonId)]!.Value.Values.First());

        // unable to create the same person
        createResult = await dispatcher.SendAsync(createCommand);

        createResult.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
        createResult.IsSuccess.Should().BeFalse();

        // request contact
        var contactCommand = new SendContactRequestCommand(guid, Guid.NewGuid());
        IOperationResult contactResult = await dispatcher.SendAsync(contactCommand);

        contactResult.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        contactResult.IsSuccess.Should().BeTrue();

        // unable to request the same contact twice
        contactResult = await dispatcher.SendAsync(contactCommand);

        contactResult.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        contactResult.IsSuccess.Should().BeFalse();

        // read person
        var store = serviceProvider.GetRequiredService<IAggregateStore<Person, PersonId>>();
        Person person = (await store.ReadAsync(PersonId.CreateInstance(guid))).Result;
        person.Should().NotBeNull();
        person!.ContactIds.Should().HaveCount(1);
    }
}

public sealed class NotificationStoreText : Disposable, IIntegrationEventStore
{
    readonly record struct IntegrationEventRecord(IIntegrationEvent Event, string? Exception, string Status);
    private static readonly Dictionary<Guid, IntegrationEventRecord> _store = [];

    public ValueTask AppendAsync(
        IIntegrationEvent @event,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        _store[@event.Id] = new(@event, default, EntityStatus.ACTIVE);

        return ValueTask.CompletedTask;
    }

    public IAsyncEnumerable<IIntegrationEvent> ReadAsync(
        Pagination pagination,
        CancellationToken _ = default)
    {
        return _store.Values
            .Select(s => s.Event)
            .ToAsyncEnumerable();
    }

    public async ValueTask DeleteAsync(
        Guid eventId,
        CancellationToken _ = default)
    {
        if (!_store.TryGetValue(eventId, out IntegrationEventRecord _))
            throw new InvalidOperationException($"Unable to find integration event with id : {eventId}");

        _store.Remove(eventId);

        await ValueTask.CompletedTask.ConfigureAwait(false);
    }
}
public sealed class EventStoreTest : Disposable, IDomainEventStore<DomainEventRecord>
{
    private static readonly Dictionary<Guid, List<IDomainEvent<PersonId>>> _store = [];

    public ValueTask AppendAsync<TAggregateId>(
        IDomainEvent<TAggregateId> @event,
        CancellationToken cancellationToken = default)
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (!_store.TryGetValue(@event.AggregateId, out _))
            _store.Add(@event.AggregateId, []);

        _store[@event.AggregateId].Add((IDomainEvent<PersonId>)@event);

        return ValueTask.CompletedTask;
    }

    public IAsyncEnumerable<IDomainEvent<TAggregateId>> ReadAsync<TAggregateId>(
        TAggregateId aggregateId,
        CancellationToken _ = default)
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        return _store.Values
            .SelectMany(x => x)
            .OfType<IDomainEvent<TAggregateId>>()
            .Where(e => e.AggregateId == aggregateId.Value)
            .Select(s => s)
            .ToAsyncEnumerable();
    }

    public IAsyncEnumerable<TResult> ReadAsync<TResult>(
        IDomainEventFilter<DomainEventRecord, TResult> filter, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
public readonly record struct CreatePersonRequestCommand(Guid Id, string FirstName, string LastName)
    : ICommand, IPersistenceDecorator, IOperationResultDecorator;
public sealed class CreatePersonRequestCommandHandler(
    IAggregateStore<Person, PersonId> aggregateStore,
    IOperationResultContext resultContext) : ICommandHandler<CreatePersonRequestCommand>
{
    private readonly IAggregateStore<Person, PersonId> _aggregateStore = aggregateStore
        ?? throw new ArgumentNullException(nameof(aggregateStore));
    private readonly IOperationResultContext _resultContext = resultContext
        ?? throw new ArgumentNullException(nameof(resultContext));

    public async ValueTask<OperationResult> HandleAsync(
        CreatePersonRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        PersonId personId = PersonId.CreateInstance(command.Id);
        Person? test = (await _aggregateStore.ReadAsync(personId, cancellationToken).ConfigureAwait(false)).Result;
        if (test is not null)
            return OperationResults
                .Conflict()
                .WithError(nameof(PersonId), "Person already exist")
                .Build();

        Person person = Person.Create(personId, command.FirstName, command.LastName);

        _resultContext.OnSuccess = OperationResults
            .Ok()
            .WithHeader(nameof(PersonId), personId)
            .Build();

        await _aggregateStore.AppendAsync(person, cancellationToken).ConfigureAwait(false);
        return OperationResults.Ok().Build();
    }
}

public readonly record struct SendContactRequestCommand(Guid SenderId, Guid ReceiverId) : ICommand, IPersistenceDecorator;
public sealed class SendContactRequestCommandHandler
    (IAggregateStore<Person, PersonId> aggregateStore) : ICommandHandler<SendContactRequestCommand>
{
    public async ValueTask<OperationResult> HandleAsync(
        SendContactRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        PersonId personId = PersonId.CreateInstance(command.SenderId);
        Person? person = (await aggregateStore.ReadAsync(personId, cancellationToken).ConfigureAwait(false)).Result;
        if (person is null)
            return OperationResults
                 .NotFound()
                 .WithError(nameof(PersonId), "Person not found")
                 .Build();

        ContactId receivedId = new(command.ReceiverId);
        OperationResult operationContact = person.BeContact(receivedId);
        if (operationContact.IsFailure)
            return operationContact;

        await aggregateStore.AppendAsync(person, cancellationToken).ConfigureAwait(false);
        return OperationResults.Ok().Build();
    }
}

public sealed class ContactCreatedDomainEventHandler
    (IIntegrationEventSourcing eventSourcing) : IDomainEventHandler<PersonCreatedDomainEvent, PersonId>
{
    public ValueTask<OperationResult> HandleAsync(
        PersonCreatedDomainEvent @event,
        CancellationToken cancellationToken = default)
    {
        PersonCreatedNotification integrationEvent = new(@event.AggregateId, @event.FirstName, @event.LastName);
        eventSourcing.Append(integrationEvent);
        return ValueTask.FromResult(OperationResults.Ok().Build());
    }
}

public sealed class ContactRequestSentDomainEventHandler
    (IIntegrationEventSourcing eventSourcing) : IDomainEventHandler<ContactRequestSentDomainEvent, PersonId>
{
    public ValueTask<OperationResult> HandleAsync(
        ContactRequestSentDomainEvent @event,
        CancellationToken cancellationToken = default)
    {
        ContactRequestSentNotification integration = new(@event.AggregateId, @event.FullName, @event.ContactId.Value);
        eventSourcing.Append(integration);
        return ValueTask.FromResult(OperationResults.Ok().Build());
    }
}

[PrimitiveJsonConverter]
public readonly record struct PersonId(Guid Value) : IAggregateId<PersonId>
{
    public static PersonId CreateInstance(Guid value) => new(value);
    public static PersonId DefaultInstance() => CreateInstance(Guid.Empty);

    public static implicit operator Guid(PersonId self) => self.Value;

    public static implicit operator string(PersonId self) => self.Value.ToString();
}

[PrimitiveJsonConverter]
public readonly record struct ContactId(Guid Value) : IPrimitive<Guid>;

public sealed record PersonCreatedDomainEvent : DomainEvent<Person, PersonId>
{
    [JsonConstructor]
    private PersonCreatedDomainEvent() { }
    public PersonCreatedDomainEvent(Person person, string firstName, string lastName) : base(person)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
}

public sealed record ContactRequestSentDomainEvent : DomainEvent<Person, PersonId>
{
    private ContactRequestSentDomainEvent() { }

    public ContactRequestSentDomainEvent(Person person, ContactId contactId) : base(person)
    {
        FullName = $"{person.FirstName} {person.LastName}";
        ContactId = contactId;
    }

    public string FullName { get; init; } = default!;
    public ContactId ContactId { get; init; } = default!;
}

public sealed record PersonCreatedNotification(
    Guid PersonId,
    string FirstName,
    string LastName) : IntegrationEvent;

public sealed record ContactRequestSentNotification(
    Guid SenderId,
    string SenderName,
    Guid ReceiverId) : IntegrationEvent;

public sealed class Person : Aggregate<PersonId>
{
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    private readonly List<ContactId> _contactIds = [];
    public IReadOnlyCollection<ContactId> ContactIds => _contactIds.AsReadOnly();
    public static Person Create(PersonId id, string firstName, string lastName)
    {
        Person person = new() { AggregateId = id };
        PersonCreatedDomainEvent @event = new(person, firstName, lastName);
        person.PushEvent(@event);
        return person;
    }

    public OperationResult BeContact(ContactId contactId)
    {
        if (_contactIds.Contains(contactId))
            return OperationResults
                .BadRequest()
                .WithError(nameof(ContactId), "The contact already exist.")
                .Build();

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

        On<ContactRequestSentDomainEvent>(@event => _contactIds.Add(@event.ContactId));
    }
}