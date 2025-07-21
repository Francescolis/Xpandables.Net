using System.Runtime.CompilerServices;

using Xpandables.Net.Api.Accounts.Events;
using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories;
using Xpandables.Net.Repositories.Filters;

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

        EventFilterDomain filter = new()
        {
            Where = e => (e.AggregateId == query.KeyId
                              && e.EventName == nameof(DepositMade)) || e.EventName == nameof(WithdrawMade),
            //EventDataWhere = e => e.RootElement
            //    .GetProperty("EventId")
            //    .GetGuid()
            //    .Equals(new Guid("019814e4-817e-74df-94d8-4d8c42121c7f")), // For Postgresql
            OrderBy = e => e.OrderByDescending(o => o.CreatedOn)
        };

        IAsyncEnumerable<IEvent> events = eventStore.FetchAsync(filter, cancellationToken);

        IAsyncEnumerable<OperationAccount> operations = GetOperations(events, cancellationToken);

        return ExecutionResults.Ok(operations)
            .WithHeader("Count", $"{filter.TotalCount}")
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