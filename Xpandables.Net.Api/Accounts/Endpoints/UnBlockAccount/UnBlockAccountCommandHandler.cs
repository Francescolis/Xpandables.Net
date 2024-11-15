
using Xpandables.Net.Commands;
using Xpandables.Net.Events.Aggregates;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Api.Accounts.Endpoints.UnBlockAccount;

public sealed class UnBlockAccountCommandHandler(
    IAggregateStore<Account> aggregateStore) : ICommandHandler<UnBlockAccountCommand>
{
    public async Task<IExecutionResult> HandleAsync(
        UnBlockAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        Account account = await aggregateStore
            .PeekAsync(command.KeyId, cancellationToken)
            .ConfigureAwait(false);

        account.UnBlock();

        await aggregateStore
            .AppendAsync(account, cancellationToken)
            .ConfigureAwait(false);

        return ExecutionResults.Ok().Build();
    }
}
