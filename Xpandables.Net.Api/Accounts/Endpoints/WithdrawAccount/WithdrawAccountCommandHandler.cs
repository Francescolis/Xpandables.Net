using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Api.Accounts.Endpoints.WithdrawAccount;

public sealed class WithdrawAccountCommandHandler(
    IAggregateStore<Account> aggregateStore) : IRequestHandler<WithdrawAccountCommand>
{
    public async Task<ExecutionResult> HandleAsync(
        WithdrawAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        Account account = await aggregateStore
            .ResolveAsync(command.KeyId, cancellationToken)
            .ConfigureAwait(false);

        account.Withdraw(command.Amount);

        await aggregateStore
            .AppendAsync(account, cancellationToken)
            .ConfigureAwait(false);

        return ExecutionResults.Success();
    }
}
