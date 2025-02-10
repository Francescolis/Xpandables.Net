using Xpandables.Net.Commands;
using Xpandables.Net.Events.Aggregates;
using Xpandables.Net.Executions;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetBalanceAccount;

public sealed class GetBalanceAccountQueryHandler(
    IAggregateStore<Account> aggregateStore) :
    IQueryHandler<GetBalanceAccountQuery, decimal>
{
    public async Task<IExecutionResult<decimal>> HandleAsync(
        GetBalanceAccountQuery query,
        CancellationToken cancellationToken)
    {
        Account account = await aggregateStore
            .PeekAsync(query.KeyId, cancellationToken)
            .ConfigureAwait(false);

        return ExecutionResults.Success(account.CurrentState.Balance);
    }
}
