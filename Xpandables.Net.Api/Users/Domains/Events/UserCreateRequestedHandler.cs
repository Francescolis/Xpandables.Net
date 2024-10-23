
using Xpandables.Net.Api.Shared.Integrations;
using Xpandables.Net.Events;
using Xpandables.Net.Events.Filters;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Api.Users.Domains.Events;

public sealed class UserCreateRequestedHandler(
    IEventStore eventStore) : IEventHandler<UserCreateRequested>
{
    public async Task<IOperationResult> HandleAsync(
        UserCreateRequested @event,
        CancellationToken cancellationToken = default)
    {
        // check for duplicate user

        IEventFilter filter = new EventEntityFilterDomain<Guid>
        {
            Predicate = x => x.AggregateId == @event.AggregateId
                && x.EventName == nameof(UserCreateRequested),
            Selector = x => x.AggregateId,
            PageIndex = 1,
            PageSize = 1
        };

        if (await eventStore.FetchAsync(filter, cancellationToken)
            .AnyAsync(cancellationToken))
        {
            return OperationResults
                .Conflict()
                .WithError(nameof(@event.AggregateId), "User already exists.")
                .Build();
        }

        UserCreated created = new()
        {
            KeyId = @event.AggregateId,
            UserName = @event.UserName,
            UserEmail = @event.Email,
            Password = @event.Password
        };

        return await eventStore
            .AppendAsync([created], cancellationToken)
            .ToOperationResultAsync();
    }
}
