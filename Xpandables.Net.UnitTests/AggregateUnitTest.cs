
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
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;
using Xpandables.Net.Primitives.Collections;
using Xpandables.Net.Primitives.Converters;
using Xpandables.Net.Primitives.Text;
using Xpandables.Net.Repositories;

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
            .AddXEventDomainHandlers()
            .AddXDispatcher()
            .AddXPersistenceCommandHandler()
            .AddXAggregateStore()
            .AddXUnitOfWorkAggregate<PersonUnitOfWork>()
            .AddXOperationResultFinalizer()
            .AddXEventDomainPublisher()
            .AddXEventNotificationPublisher()
            .AddXEventDomainDuplicateDecorator()
            .AddXEventDomainStore<EventStoreTest>()
            .AddXEventNotificationStore<NotificationStoreText>();

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
        IAggregateStore<Person, PersonId> store = serviceProvider
            .GetRequiredService<IAggregateStore<Person, PersonId>>();

        IOperationResult<Person> personResult = await store
            .ReadAsync(PersonId.Create(guid));

        Assert.True(personResult.IsSuccess);
        Person person = personResult.Result;

        person.ContactIds
            .Should()
            .HaveCount(1);
    }
}

public sealed class NotificationStoreText : Disposable, IEventNotificationStore
{
    readonly record struct NotificationRecord(
        IEventNotification Event, string? Exception, string Status);

    private static readonly Dictionary<Guid, NotificationRecord> _store = [];

    public ValueTask AppendAsync(
        IEventNotification @event,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        _store[@event.Id] = new(@event, default, EntityStatus.ACTIVE);

        return ValueTask.CompletedTask;
    }

    public IAsyncEnumerable<IEventNotification> ReadAsync(
        IEventFilter filter,
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
public sealed class EventStoreTest : Disposable, IEventDomainStore
{
    private static readonly Dictionary
        <Guid, List<IEventDomain<PersonId>>> _store = [];

    public ValueTask AppendAsync<TAggregateId>(
        IEventDomain<TAggregateId> @event,
        CancellationToken cancellationToken = default)
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (!_store.TryGetValue(@event.AggregateId, out _))
            _store.Add(@event.AggregateId, []);

        _store[@event.AggregateId].Add((IEventDomain<PersonId>)@event);

        return ValueTask.CompletedTask;
    }

    public IAsyncEnumerable<IEventDomain<TAggregateId>> ReadAsync<TAggregateId>(
        TAggregateId aggregateId,
        CancellationToken _ = default)
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        return _store.Values
            .SelectMany(x => x)
            .OfType<IEventDomain<TAggregateId>>()
            .Where(e => e.AggregateId == aggregateId.Value)
            .Select(s => s)
            .ToAsyncEnumerable();
    }

    public IAsyncEnumerable<IEventDomain<TAggregateId>> ReadAsync<TAggregateId>(
        IEventFilter filter,
        CancellationToken cancellationToken = default)
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<IEventDomain<TAggregateId>> query = _store.Values
            .SelectMany(x => x)
            .ToList()
            .OfType<IEventDomain<TAggregateId>>()
            .AsQueryable();

        if (filter.EventTypeName is not null)
            query = query.Where(e => e.GetType().Name == filter.EventTypeName);

        if (filter.AggregateIdTypeName is not null)
            query = query.Where(e => typeof(TAggregateId).Name == filter.AggregateIdTypeName);

        if (filter.DataCriteria is not null)
        {
            Func<System.Text.Json.JsonDocument, bool> dataCriteria = filter.DataCriteria.Compile();
            query = query.Where(e => e.ToJsonDocument(JsonSerializerDefaultOptions.OptionPropertyNameCaseInsensitiveTrue, default), j => dataCriteria(j));
        }

        return query
            .Select(s => s)
            .OfType<IEventDomain<TAggregateId>>()
            .ToAsyncEnumerable();
    }
}
public readonly record struct CreatePersonRequestCommand(
    Guid Id, string FirstName, string LastName)
    : ICommand, IPersistenceDecorator, IOperationFinalizerDecorator;
public sealed class CreatePersonRequestCommandHandler(
    IAggregateStore<Person, PersonId> aggregateStore,
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
        PersonId personId = PersonId.Create(command.Id);

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
    (IAggregateStore<Person, PersonId> aggregateStore) :
    ICommandHandler<SendContactRequestCommand>
{
    public async ValueTask<IOperationResult> HandleAsync(
        SendContactRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        PersonId personId = PersonId.Create(command.SenderId);

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
    (IEventNotificationStore notificationStore)
    : IEventDomainHandler<PersonCreatedDomainEvent, PersonId>
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
    (IEventNotificationStore notificationStore)
    : IEventDomainHandler<ContactRequestSentDomainEvent, PersonId>
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
    public static PersonId Create(Guid value) => new(value);
    public static PersonId Default() => Create(Guid.Empty);

    public static implicit operator Guid(PersonId self) => self.Value;

    public static implicit operator string(PersonId self)
        => self.Value.ToString();
    public static implicit operator PersonId(Guid value) => new(value);
}

[PrimitiveJsonConverter]
public readonly record struct ContactId(Guid Value) : IPrimitive<Guid>;

public sealed record PersonCreatedDomainEvent :
    EventDomain<Person, PersonId>, IEventDomainDuplicate
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

    [JsonIgnore]
    public IEventFilter? Filter => new EventFilter
    {
        EventTypeName = nameof(PersonCreatedDomainEvent),
        AggregateIdTypeName = nameof(PersonId),
        Pagination = Pagination.With(0, 1),
        DataCriteria = x
            => x.RootElement
                    .GetProperty(nameof(FirstName))
                    .GetString() == FirstName ||
                x.RootElement
                .GetProperty(nameof(LastName))
                    .GetString() == LastName

    };

    [JsonIgnore]
    public IOperationResult OnFailure => OperationResults
            .Conflict()
            .WithError(nameof(FirstName), "Duplicate found")
            .WithError(nameof(LastName), "Duplicate found")
            .Build();
}

public sealed record ContactRequestSentDomainEvent :
    EventDomain<Person, PersonId>, IEventDomainDuplicate
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

    [JsonIgnore]
    public IEventFilter? Filter => new EventFilter
    {
        EventTypeName = nameof(PersonCreatedDomainEvent),
        AggregateIdTypeName = nameof(PersonId),
        Pagination = Pagination.With(0, 1),
        DataCriteria = x
            => x.RootElement
                    .GetProperty(nameof(FullName))
                    .GetString() == FullName ||
                x.RootElement
                .GetProperty(nameof(ContactId))
                    .GetGuid() == ContactId.Value

    };

    [JsonIgnore]
    public IOperationResult OnFailure => OperationResults
            .Conflict()
            .WithError(nameof(FullName), "Duplicate found")
            .WithError(nameof(ContactId), "Duplicate found")
            .Build();
}

public sealed record PersonCreatedNotification(
    Guid PersonId,
    string FirstName,
    string LastName) : EventNotification;

public sealed record ContactRequestSentNotification(
    Guid SenderId,
    string SenderName,
    Guid ReceiverId) : EventNotification;

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
        //if (_contactIds.Contains(contactId))
        //    return OperationResults
        //        .BadRequest()
        //        .WithError(nameof(ContactId), "The contact already exist.")
        //        .Build();

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

public sealed class PersonUnitOfWork : Disposable, IUnitOfWork
{
    public ValueTask<int> PersistAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(0);
    }

    IRepository<TEntity> IUnitOfWork.GetRepository<TEntity>()
    {
        throw new NotImplementedException();
    }

    IRepositoryRead<TEntity> IUnitOfWork.GetRepositoryRead<TEntity>()
    {
        throw new NotImplementedException();
    }

    IRepositoryWrite<TEntity> IUnitOfWork.GetRepositoryWrite<TEntity>()
    {
        throw new NotImplementedException();
    }
}