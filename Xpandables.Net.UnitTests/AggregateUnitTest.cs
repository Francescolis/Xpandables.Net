
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
using Xpandables.Net.Aggregates.DomainEvents;
using Xpandables.Net.Aggregates.Notifications;
using Xpandables.Net.Commands;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;
using Xpandables.Net.Primitives.Collections;
using Xpandables.Net.Primitives.Converters;
using Xpandables.Net.Repositories;
using Xpandables.Net.Transactions;

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
            .AddXCommandQueryHandlers(options =>
                options
                .UsePersistence()
                .UseOperationFinalizer())
            .AddXDispatcher()
            .AddXPersistenceCommandHandler()
            .AddXAggregateStore()
            .AddXAggregateStoreTransactional()
            .AddXAggregateTransactional<PersonTransactional>()
            .AddXOperationResultFinalizer()
            .AddXDomainEventPublisher()
            .AddXNotificationPublisher()
            .AddXDomainEventStore<EventStoreTest>()
            .AddXNotificationStore<NotificationStoreText>();

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
            .Be(System.Net.HttpStatusCode.BadRequest);

        contactResult.IsSuccess
            .Should()
            .BeFalse();

        // read person
        IAggregateStore<Person, PersonId> store = serviceProvider
            .GetRequiredService<IAggregateStore<Person, PersonId>>();

        IOperationResult<Person> personResult = await store
            .ReadAsync(PersonId.CreateInstance(guid));

        Assert.True(personResult.IsSuccess);
        Person person = personResult.Result;

        person.ContactIds
            .Should()
            .HaveCount(1);
    }
}

public sealed class NotificationStoreText : Disposable, INotificationStore
{
    readonly record struct NotificationRecord(
        INotification Event, string? Exception, string Status);

    private static readonly Dictionary<Guid, NotificationRecord> _store = [];

    public ValueTask AppendAsync(
        INotification @event,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        _store[@event.Id] = new(@event, default, EntityStatus.ACTIVE);

        return ValueTask.CompletedTask;
    }

    public IAsyncEnumerable<INotification> ReadAsync(
        INotificationFilter filter,
        CancellationToken _ = default)
    {
        return _store.Values
            .Where(e => e.Exception == null && e.Status == EntityStatus.ACTIVE)
            .Select(s => s.Event)
            .ToAsyncEnumerable();
    }

    public async ValueTask AppendCloseAsync(
     Guid eventId,
     Exception? exception = default,
     CancellationToken cancellationToken = default)
    {
        if (!_store.TryGetValue(eventId, out NotificationRecord _))
            throw new InvalidOperationException(
                $"Unable to find notification with id : {eventId}");

        _store[eventId] = _store[eventId] with
        {
            Exception = exception?.ToString(),
            Status = EntityStatus.DELETED
        };

        await ValueTask.CompletedTask.ConfigureAwait(false);
    }
}
public sealed class EventStoreTest : Disposable, IDomainEventStore
{
    private static readonly Dictionary
        <Guid, List<IDomainEvent<PersonId>>> _store = [];

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

    public IAsyncEnumerable<IDomainEvent<TAggregateId>> ReadAsync<TAggregateId>(
        IDomainEventFilter filter,
        CancellationToken cancellationToken = default)
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        throw new NotImplementedException();
    }
}
public readonly record struct CreatePersonRequestCommand(
    Guid Id, string FirstName, string LastName)
    : ICommand, IPersistenceDecorator, IOperationFinalizerDecorator;
public sealed class CreatePersonRequestCommandHandler(
    IAggregateStoreTransactional<Person, PersonId> aggregateStore,
    IOperationFinalizer resultContext) :
    ICommandHandler<CreatePersonRequestCommand>
{
    private readonly IAggregateStore<Person, PersonId> _aggregateStore
        = aggregateStore
        ?? throw new ArgumentNullException(nameof(aggregateStore));
    private readonly IOperationFinalizer _resultContext = resultContext
        ?? throw new ArgumentNullException(nameof(resultContext));

    public async ValueTask<IOperationResult> HandleAsync(
        CreatePersonRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        PersonId personId = PersonId.CreateInstance(command.Id);

        IOperationResult<Person> testResult = await _aggregateStore
            .ReadAsync(personId, cancellationToken)
            .ConfigureAwait(false);

        if (testResult.IsSuccess)
            return OperationResults
                .Conflict()
                .WithError(nameof(PersonId), "Person already exist")
                .Build();

        Person person = Person
            .Create(personId, command.FirstName, command.LastName);

        _resultContext.Finalizer = op => op.IsSuccess switch
            {
                true => OperationResults
                 .Ok()
                 .WithHeader(nameof(PersonId), personId)
                 .Build(),
                _ => op
            };

        return await _aggregateStore
            .AppendAsync(person, cancellationToken)
            .ConfigureAwait(false) is { IsFailure: true } failureOperation
            ? failureOperation
            : OperationResults.Ok().Build();
    }
}

public readonly record struct SendContactRequestCommand(
    Guid SenderId, Guid ReceiverId) :
    ICommand;

public sealed class SendContactRequestCommandHandler
    (IAggregateStoreTransactional<Person, PersonId> aggregateStore) :
    ICommandHandler<SendContactRequestCommand>
{
    public async ValueTask<IOperationResult> HandleAsync(
        SendContactRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        PersonId personId = PersonId.CreateInstance(command.SenderId);

        IOperationResult<Person> personResult = await aggregateStore
            .ReadAsync(personId, cancellationToken)
            .ConfigureAwait(false);

        if (!personResult.IsSuccess)
            return OperationResults
                 .NotFound()
                 .WithError(nameof(PersonId), "Person not found")
                 .Build();

        ContactId receivedId = new(command.ReceiverId);
        IOperationResult operationContact = personResult.Result
            .BeContact(receivedId);

        if (operationContact.IsFailure)
            return operationContact;

        return await aggregateStore
            .AppendAsync(personResult.Result, cancellationToken)
            .ConfigureAwait(false) is { IsFailure: true } failureOperation
            ? failureOperation
            : OperationResults.Ok().Build();
    }
}

public sealed class ContactCreatedDomainEventHandler
    (INotificationStore notificationStore)
    : IDomainEventHandler<PersonCreatedDomainEvent, PersonId>
{
    public async ValueTask<IOperationResult> HandleAsync(
        PersonCreatedDomainEvent @event,
        CancellationToken cancellationToken = default)
    {
        PersonCreatedNotification integrationEvent =
            new(@event.AggregateId, @event.FirstName, @event.LastName);
        return await notificationStore
            .AppendAsync(integrationEvent, cancellationToken)
            .ToOperationResultAsync();
    }
}

public sealed class ContactRequestSentDomainEventHandler
    (INotificationStore notificationStore)
    : IDomainEventHandler<ContactRequestSentDomainEvent, PersonId>
{
    public async ValueTask<IOperationResult> HandleAsync(
        ContactRequestSentDomainEvent @event,
        CancellationToken cancellationToken = default)
    {
        ContactRequestSentNotification integration =
            new(@event.AggregateId, @event.FullName, @event.ContactId.Value);
        return await notificationStore
            .AppendAsync(integration, cancellationToken)
            .ToOperationResultAsync();
    }
}

[PrimitiveJsonConverter]
public readonly record struct PersonId(Guid Value) : IAggregateId<PersonId>
{
    public static PersonId CreateInstance(Guid value) => new(value);
    public static PersonId DefaultInstance() => CreateInstance(Guid.Empty);

    public static implicit operator Guid(PersonId self) => self.Value;

    public static implicit operator string(PersonId self)
        => self.Value.ToString();
    public static implicit operator PersonId(Guid value) => new(value);
}

[PrimitiveJsonConverter]
public readonly record struct ContactId(Guid Value) : IPrimitive<Guid>;

public sealed record PersonCreatedDomainEvent : DomainEvent<Person, PersonId>
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
}

public sealed record ContactRequestSentDomainEvent :
    DomainEvent<Person, PersonId>
{
    private ContactRequestSentDomainEvent() { }

    public ContactRequestSentDomainEvent(
        Person person, ContactId contactId) : base(person)
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
    string LastName) : Notification;

public sealed record ContactRequestSentNotification(
    Guid SenderId,
    string SenderName,
    Guid ReceiverId) : Notification;

public sealed class Person : Aggregate<PersonId>, ITransactionDecorator
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

    public IOperationResult BeContact(ContactId contactId)
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

        On<ContactRequestSentDomainEvent>(@event
            => _contactIds.Add(@event.ContactId));
    }
}

public sealed class PersonTransactional :
    Transactional, IAggregateTransactional
{
    protected override ValueTask BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }

    protected override ValueTask CompleteTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }
}