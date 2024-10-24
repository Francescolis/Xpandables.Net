
using System.ComponentModel.DataAnnotations;

using Xpandables.Net.Api.Shared.Integrations;
using Xpandables.Net.Events;
using Xpandables.Net.Events.Filters;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Api.Users.Domains.Events;

public sealed class BeContactRequestedHandler(
    IEventStore eventStore) : IEventHandler<BeContactRequested>
{
    public async Task HandleAsync(
        BeContactRequested @event,
        CancellationToken cancellationToken = default)
    {
        // check contact existence

        IEventFilter filter = new EventEntityFilterDomain<Guid>
        {
            Predicate = x => x.AggregateId == @event.ContactId.Value
                && x.EventName == nameof(UserAdded),
            Selector = x => x.AggregateId,
            PageIndex = 1,
            PageSize = 1
        };

        if (await eventStore.FetchAsync(filter, cancellationToken)
            .AnyAsync(cancellationToken))
        {
            throw new ValidationException(new ValidationResult(
                "Contact does not exist.",
                [nameof(@event.ContactId)]), null, @event.ContactId.Value);
        }

        Contacted contacted = new()
        {
            UserId = @event.AggregateId,
            ContactId = @event.ContactId
        };

        await eventStore
            .AppendAsync([contacted], cancellationToken)
            .ToOperationResultAsync();
    }
}
