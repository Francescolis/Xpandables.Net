using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.BlockAccount;

public sealed class BlockAccountCommandHandler : IDeciderRequestHandler<BlockAccountCommand, Account>
{
    public Task<IExecutionResult> HandleAsync(
        BlockAccountCommand command,
        Account dependency,
        CancellationToken cancellationToken = default)
    {
        dependency.Block();

        return Task.FromResult(ExecutionResults.Success());
    }
}
