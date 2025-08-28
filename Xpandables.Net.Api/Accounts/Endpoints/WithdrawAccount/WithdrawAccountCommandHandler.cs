using Xpandables.Net.Executions;
using Xpandables.Net.Repositories;
using Xpandables.Net.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.WithdrawAccount;

public sealed class WithdrawAccountCommandHandler(
    IAggregateStore<Account> aggregateStore) : IRequestHandler<WithdrawAccountCommand>
{
    public async Task<ExecutionResult> HandleAsync(
        WithdrawAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        Account account = await aggregateStore
            .LoadAsync(command.KeyId, cancellationToken)
            .ConfigureAwait(false);

        account.Withdraw(command.Amount);

        await aggregateStore
            .SaveAsync(account, cancellationToken)
            .ConfigureAwait(false);

        return ExecutionResult.Success();
    }
}
