using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetBalanceAccount;

public sealed class GetBalanceAccountQueryHandler(
    IAggregateStore<Account> aggregateStore) :
    IRequestHandler<GetBalanceAccountQuery>
{
    public async Task<ExecutionResult> HandleAsync(
        GetBalanceAccountQuery query,
        CancellationToken cancellationToken)
    {
        Account account = await aggregateStore
            .ResolveAsync(query.KeyId, cancellationToken)
            .ConfigureAwait(false);

        return ExecutionResults.Success(account.CurrentState.Balance);
    }
}
