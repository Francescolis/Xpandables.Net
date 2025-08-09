using System.Runtime.CompilerServices;

using Xpandables.Net.Api.Accounts.Events;
using Xpandables.Net.Collections;
using Xpandables.Net.Events;
using Xpandables.Net.Executions;
using Xpandables.Net.Repositories;
using Xpandables.Net.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetOperationsAccount;

public sealed class GetOperationsAccountQueryHandler(
    IEventStore eventStore) :
    IStreamRequestHandler<GetOperationsAccountQuery, OperationAccount>
{
    public async Task<ExecutionResult<IAsyncPagedEnumerable<OperationAccount>>> HandleAsync(
        GetOperationsAccountQuery request,
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        Func<IQueryable<EntityDomainEvent>, IQueryable<EntityDomainEvent>> filterFunc = q =>
            q.Where(w => w.AggregateId == request.KeyId
                             && (w.Name == nameof(DepositMade) || w.Name == nameof(WithdrawMade)))
                .OrderByDescending(o => o.CreatedOn)
                .Skip(0)
                .Take(2);

        IAsyncPagedEnumerable<IEvent> events = eventStore
            .FetchAsync(filterFunc, cancellationToken)
            .AsEventsPagedAsync(cancellationToken);

        return ExecutionResult
            .Ok(events.WithPagination(GetOperations, cancellationToken))
            .Build();

        static async IAsyncEnumerable<OperationAccount> GetOperations(IAsyncEnumerable<IEvent> events,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (IEvent @event in events.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                switch (@event)
                {
                    case DepositMade deposit:
                        yield return new OperationAccount
                        {
                            Id = deposit.Id,
                            Date = deposit.OccurredOn.DateTime,
                            Amount = deposit.Amount,
                            Type = "Deposit"
                        };
                        break;
                    case WithdrawMade withdraw:
                        yield return new OperationAccount
                        {
                            Id = withdraw.Id,
                            Date = withdraw.OccurredOn.DateTime,
                            Amount = withdraw.Amount,
                            Type = "Withdraw"
                        };
                        break;
                }
            }
        }
    }
}