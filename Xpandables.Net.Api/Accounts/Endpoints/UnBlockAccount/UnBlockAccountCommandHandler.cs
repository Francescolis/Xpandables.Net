
using Xpandables.Net.Events.Aggregates;
using Xpandables.Net.Operations;
using Xpandables.Net.Responsibilities;

namespace Xpandables.Net.Api.Accounts.Endpoints.UnBlockAccount;

public sealed class UnBlockAccountCommandHandler(
    IAggregateStore<Account> aggregateStore) : ICommandHandler<UnBlockAccountCommand>
{
    public async Task<IOperationResult> HandleAsync(
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

        return OperationResults.Ok().Build();
    }
}
