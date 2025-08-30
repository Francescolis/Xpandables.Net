using Xpandables.Net.Api.Accounts.Events;
using Xpandables.Net.Collections;
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

        IAsyncPagedEnumerable<OperationAccount> events = eventStore
            .ReadStreamAsync(request.KeyId, cancellationToken: cancellationToken)
            .Where(e => e.Event is DepositMade or WithdrawMade)
            .Select(e => e.Event switch
            {
                DepositMade deposit => new OperationAccount
                {
                    Id = deposit.Id,
                    Date = deposit.OccurredOn.DateTime,
                    Amount = deposit.Amount,
                    Type = "Deposit"
                },
                WithdrawMade withdraw => new OperationAccount
                {
                    Id = withdraw.Id,
                    Date = withdraw.OccurredOn.DateTime,
                    Amount = withdraw.Amount,
                    Type = "Withdraw"
                },
                _ => throw new InvalidOperationException("Unknown event type.")
            })
            .AsAsyncPagedEnumerable();

        return ExecutionResult
            .Ok(events)
            .Build();
    }
}