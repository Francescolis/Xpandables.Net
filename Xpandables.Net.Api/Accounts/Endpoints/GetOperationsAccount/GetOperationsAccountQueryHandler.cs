using System.Runtime.CompilerServices;

using Xpandables.Net.Api.Accounts.Events;
using Xpandables.Net.Events;
using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories.Filters;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetOperationsAccount;

public sealed class GetOperationsAccountQueryHandler(
    IEventStore eventStore) :
    IStreamRequestHandler<GetOperationsAccountQuery, OperationAccount>
{
    public async IAsyncEnumerable<OperationAccount> HandleAsync(
        GetOperationsAccountQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IEventFilter filter = new EventEntityFilterDomain
        {
            Predicate = e => e.AggregateId == query.KeyId,
            //EventDataPredicate = e => e.RootElement
            //    .GetProperty(nameof(EventEntityDomain.EventName))
            //    .GetString()!
            //    .EndsWith("Made"), // For Postgresql
            OrderBy = e => e.OrderByDescending(o => o.CreatedOn)
        };

        var events = eventStore.FetchAsync(filter, cancellationToken);

        await foreach (var @event in events)
        {
            if ((@event is DepositMade deposit))
            {
                yield return new OperationAccount
                {
                    Id = deposit.EventId,
                    Date = deposit.OccurredOn.DateTime,
                    Amount = deposit.Amount,
                    Type = "Deposit"
                };
            }
            else if ((@event is WithdrawMade withdraw))
            {
                yield return new OperationAccount
                {
                    Id = withdraw.EventId,
                    Date = withdraw.OccurredOn.DateTime,
                    Amount = withdraw.Amount,
                    Type = "Withdraw"
                };
            }
        }
    }
}
