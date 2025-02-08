
using Xpandables.Net.Commands;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Api.Accounts.Endpoints.BlockAccount;

public sealed class BlockAccountCommandHandler : ICommandHandler<BlockAccountCommand, Account>
{
    public async Task<IExecutionResult> HandleAsync(
        BlockAccountCommand command,
        Account dependency,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        dependency.Block();

        return ExecutionResults.Success();
    }
}
