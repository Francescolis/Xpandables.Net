using System.Runtime.CompilerServices;

using Xpandables.Net.Api.Accounts.Events;
using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetOperationsAccount;

public sealed class GetOperationsAccountQueryHandler(
    IEventStore eventStore) :
    IRequestHandler<GetOperationsAccountQuery>
{
    public async Task<ExecutionResult> HandleAsync(
        GetOperationsAccountQuery query,
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        Func<IQueryable<EntityDomainEvent>, IAsyncQueryable<IEvent>> filterFunc = q =>
            q.Where(w => w.AggregateId == query.KeyId
                             && (w.EventName == nameof(DepositMade) || w.EventName == nameof(WithdrawMade)))
                .OrderByDescending(o => o.CreatedOn)
                .SelectEvent()
                .OfType<IEvent>();

        IAsyncEnumerable<IEvent> events = eventStore.FetchAsync(filterFunc, cancellationToken);

        IAsyncEnumerable<OperationAccount> operations = GetOperations(events, cancellationToken);

        return ExecutionResults.Ok(operations)
            .WithHeader("Count", $"{await events.CountAsync(cancellationToken)}")
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
                            Id = deposit.EventId,
                            Date = deposit.OccurredOn.DateTime,
                            Amount = deposit.Amount,
                            Type = "Deposit"
                        };
                        break;
                    case WithdrawMade withdraw:
                        yield return new OperationAccount
                        {
                            Id = withdraw.EventId,
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