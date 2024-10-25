
using Xpandables.Net.Events.Aggregates;
using Xpandables.Net.Operations;
using Xpandables.Net.Responsibilities;

namespace Xpandables.Net.Api.Accounts.Endpoints.WithdrawAccount;

public sealed class WithdrawAccountCommandHandler(
    IAggregateStore<Account> aggregateStore) : ICommandHandler<WithdrawAccountCommand>
{
    public async Task<IOperationResult> HandleAsync(
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

        return OperationResults.Ok().Build();
    }
}
