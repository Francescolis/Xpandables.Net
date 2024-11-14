using Xpandables.Net.Commands;
using Xpandables.Net.Events.Aggregates;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetBalanceAccount;

public sealed class GetBalanceAccountQueryHandler(
    IAggregateStore<Account> aggregateStore) :
    IQueryHandler<GetBalanceAccountQuery, decimal>
{
    public async Task<IOperationResult<decimal>> HandleAsync(
        GetBalanceAccountQuery query,
        CancellationToken cancellationToken)
    {
        Account account = await aggregateStore
            .PeekAsync(query.KeyId, cancellationToken)
            .ConfigureAwait(false);

        return OperationResults.Ok(account.CurrentState.Balance).Build();
    }
}
