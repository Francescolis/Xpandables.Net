
using Xpandables.Net.Commands;
using Xpandables.Net.Events.Aggregates;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Api.Accounts.Endpoints.BlockAccount;

public sealed class BlockAccountCommandHandler(
    IAggregateStore<Account> aggregateStore) : ICommandHandler<BlockAccountCommand>
{
    public async Task<IOperationResult> HandleAsync(
        BlockAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        Account account = await aggregateStore
            .PeekAsync(command.KeyId, cancellationToken)
            .ConfigureAwait(false);

        account.Block();

        await aggregateStore
            .AppendAsync(account, cancellationToken)
            .ConfigureAwait(false);

        return OperationResults.Ok().Build();
    }
}
