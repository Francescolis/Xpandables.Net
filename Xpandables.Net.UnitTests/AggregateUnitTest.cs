
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
using Xpandables.Net.Aspects;
using Xpandables.Net.Commands;
using Xpandables.Net.Decorators;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Operations;
using Xpandables.Net.Optionals;
using Xpandables.Net.Primitives;
using Xpandables.Net.Primitives.Collections;
using Xpandables.Net.Primitives.Converters;
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
            .Configure<EventOptions>(EventOptions.Default)
            .AddXAggregateCommandHandlers()
            .AddXCommandQueryHandlers(options =>
                options
                .UsePersistence()
                .UseOperationFinalizer())
            .AddXEventHandlers()
            .AddXDispatcher()
            .AddXPersistenceCommandHandler()
            .AddXAggregateStore()
            .AddXUnitOfWorkAggregate<PersonUnitOfWork>()
            .AddXOperationResultFinalizer()
            .AddXEventPublisher()
            .AddXEventDuplicateDecorator()
            .AddXEventDomainStore()
            .AddXEventIntegrationStore()
            .AddXOnAspects(typeof(CommandHandlerAggregateDecorator<,>).Assembly)
            .AddXAspectBehaviors();

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
        IAggregateStore<Person> store = serviceProvider
            .GetRequiredService<IAggregateStore<Person>>();

        IOperationResult<Person> personResult = await store
            .ReadAsync(PersonId.Create(guid));

        Assert.True(personResult.IsSuccess);
        Person person = personResult.Result;

        person.ContactIds
            .Should()
            .HaveCount(1);
    }
}

public sealed class CreatePersonRequestCommand(
    Guid aggregateId,
    string firstName,
    string lastName)
    : ICommand<Person>
{
    public Guid KeyId { get; init; } = aggregateId;
    public string FirstName { get; init; } = firstName;
    public string LastName { get; init; } = lastName;
    public Optional<Person> Aggregate { get; set; } = Optional.Empty<Person>();
}

[AspectAggregate<CreatePersonRequestCommand, Person>(ContinueWhenNotFound = true)]
public sealed class CreatePersonRequestCommandHandler :
    ICommandHandler<CreatePersonRequestCommand, Person>
{
    public ValueTask<IOperationResult> HandleAsync(
        CreatePersonRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Aggregate.IsNotEmpty)
        {
            return ValueTask.FromResult(OperationResults
                .Conflict()
                .WithError(nameof(PersonId), "Person already exist")
                .Build());
        }

        command.Aggregate = Person
            .Create(command.KeyId, command.FirstName, command.LastName);

        return ValueTask.FromResult(OperationResults
            .Ok()
            .WithHeader(nameof(PersonId), command.KeyId.ToString())
            .Build());
    }
}

public sealed class SendContactRequestCommand(
    Guid aggregateId, Guid receiverId) :
    ICommand<Person>
{
    public Guid KeyId { get; init; } = aggregateId;
    public Guid ReceiverId { get; init; } = receiverId;
    public Optional<Person> Aggregate { get; set; } = Optional.Empty<Person>();
}

[AspectAggregate<SendContactRequestCommand, Person>]
public sealed class SendContactRequestAggregateCommandHandler :
    ICommandHandler<SendContactRequestCommand, Person>
{
    public ValueTask<IOperationResult> HandleAsync(
        SendContactRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        Assert.True(command.Aggregate.IsNotEmpty);

        ContactId receivedId = new(command.ReceiverId);
        IOperationResult result = command.Aggregate.Value.BeContact(receivedId);

        return ValueTask.FromResult(result);
    }
}

public sealed class ContactCreatedDomainEventHandler
    (IEventIntegrationStore eventIntegrationStore)
    : IEventHandler<PersonCreatedDomainEvent>
{
    public async ValueTask<IOperationResult> HandleAsync(
        PersonCreatedDomainEvent @event,
        CancellationToken cancellationToken = default)
    {
        PersonCreatedEventIntegration integrationEvent =
            new(@event.AggregateId, @event.FirstName, @event.LastName);
        return await eventIntegrationStore
            .AppendAsync(integrationEvent, cancellationToken)
            .ToOperationResultAsync();
    }
}

public sealed class ContactRequestSentDomainEventHandler
    (IEventIntegrationStore eventIntegrationStore)
    : IEventHandler<ContactRequestSentDomainEvent>
{
    public async ValueTask<IOperationResult> HandleAsync(
        ContactRequestSentDomainEvent @event,
        CancellationToken cancellationToken = default)
    {
        ContactRequestSentEventIntegration integration =
            new(@event.AggregateId, @event.FullName, @event.ContactId.Value);
        return await eventIntegrationStore
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

    [JsonIgnore]
    public IEventFilter? Filter => new EventFilter
    {
        EventTypeName = nameof(PersonCreatedDomainEvent),
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

    [JsonIgnore]
    public IEventFilter? Filter => new EventFilter
    {
        EventTypeName = nameof(ContactRequestSentDomainEvent),
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
    private static readonly HashSet<IEventEntity> _store = [];
    public PersonUnitOfWork() => _ = _events ??= [];

    [ThreadStatic]
#pragma warning disable CS8618 
    // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private static HashSet<IEventEntity> _events;
#pragma warning restore CS8618 
    // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public IRepository<TEntity> GetRepository<TEntity>()
        where TEntity : class
        => new RepositoryPerson<TEntity>(_store);

    public IRepositoryRead<TEntity> GetRepositoryRead<TEntity>()
        where TEntity : class
        => new RepositoryPerson<TEntity>(_store);

    public IRepositoryWrite<TEntity> GetRepositoryWrite<TEntity>()
        where TEntity : class
        => new RepositoryPerson<TEntity>(_events);

    public async ValueTask PersistAsync(
        CancellationToken cancellationToken = default)
    {
        int count = _events.Count;
        if (_events.Count != 0)
        {
            _events.ForEach(e => _store.Add(e));
            _events.Clear();
        }

        await ValueTask.CompletedTask;
    }
}

public sealed class RepositoryPerson<TEvent>(HashSet<IEventEntity> events) :
    RepositoryBase<TEvent>
    where TEvent : class
{
    private readonly HashSet<IEventEntity> _events = events;

    public override ValueTask InsertAsync(
        TEvent entity,
        CancellationToken cancellationToken = default)
    {
        _events.Add((IEventEntity)entity);
        return ValueTask.CompletedTask;
    }

    public override IAsyncEnumerable<TResult> FetchAsync<TResult>(
        IEntityFilter<TEvent, TResult> filter,
        CancellationToken cancellationToken = default) => filter
            .GetQueryableFiltered(_events.OfType<TEvent>().AsQueryable())
            .ToAsyncEnumerable();
}