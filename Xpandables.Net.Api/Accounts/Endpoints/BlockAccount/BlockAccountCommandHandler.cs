using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.BlockAccount;

public sealed class BlockAccountCommandHandler : IDependencyRequestHandler<BlockAccountCommand, Account>
{
    public Task<ExecutionResult> HandleAsync(
        BlockAccountCommand command,
        Account dependency,
        CancellationToken cancellationToken = default)
    {
        dependency.Block();

        return Task.FromResult(ExecutionResults.Success());
    }
}
