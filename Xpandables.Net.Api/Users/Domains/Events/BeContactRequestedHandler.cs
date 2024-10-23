
using Xpandables.Net.Api.Shared.Integrations;
using Xpandables.Net.Events;
using Xpandables.Net.Events.Filters;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Api.Users.Domains.Events;

public sealed class BeContactRequestedHandler(
    IEventStore eventStore) : IEventHandler<BeContactRequested>
{
    public async Task<IOperationResult> HandleAsync(
        BeContactRequested @event,
        CancellationToken cancellationToken = default)
    {
        // check contact existence

        IEventFilter filter = new EventEntityFilterDomain<Guid>
        {
            Predicate = x => x.AggregateId == @event.ContactId.Value
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
                .WithError(nameof(@event.ContactId), "Contact does not exist.")
                .Build();
        }

        Contacted contacted = new()
        {
            UserId = @event.AggregateId,
            ContactId = @event.ContactId
        };

        return await eventStore
            .AppendAsync([contacted], cancellationToken)
            .ToOperationResultAsync();
    }
}
