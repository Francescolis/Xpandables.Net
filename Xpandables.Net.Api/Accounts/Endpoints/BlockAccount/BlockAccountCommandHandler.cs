using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.BlockAccount;

public sealed class BlockAccountCommandHandler : IRequestHandler<BlockAccountCommand, Account>
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
