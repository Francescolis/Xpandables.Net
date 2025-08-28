using Xpandables.Net.Executions;
using Xpandables.Net.Repositories;
using Xpandables.Net.Tasks;

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
            .LoadAsync(query.KeyId, cancellationToken)
            .ConfigureAwait(false);

        return ExecutionResult.Success(account.CurrentState.Balance);
    }
}
