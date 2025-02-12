using Xpandables.Net.Events.Aggregates;
using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.WithdrawAccount;

public sealed class WithdrawAccountCommandHandler(
    IAggregateStore<Account> aggregateStore) : ICommandHandler<WithdrawAccountCommand>
{
    public async Task<IExecutionResult> HandleAsync(
        WithdrawAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        Account account = await aggregateStore
            .PeekAsync(command.KeyId, cancellationToken)
            .ConfigureAwait(false);

        account.Withdraw(command.Amount);

        await aggregateStore
            .AppendAsync(account, cancellationToken)
            .ConfigureAwait(false);

        return ExecutionResults.Success();
    }
}
