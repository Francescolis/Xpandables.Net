using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetBalanceAccount;

public sealed class GetBalanceAccountQueryHandler(
    IAggregateStore<Account> aggregateStore) :
    IRequestHandler<GetBalanceAccountQuery, decimal>
{
    public async Task<ExecutionResult<decimal>> HandleAsync(
        GetBalanceAccountQuery query,
        CancellationToken cancellationToken)
    {
        Account account = await aggregateStore
            .PeekAsync(query.KeyId, cancellationToken)
            .ConfigureAwait(false);

        return ExecutionResults.Success(account.CurrentState.Balance);
    }
}
