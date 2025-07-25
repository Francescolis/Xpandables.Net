using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.BlockAccount;

public sealed class BlockAccountCommandHandler : IDependencyRequestHandler<BlockAccountCommand, Account>
{
    public Task<ExecutionResult> HandleAsync(
        BlockAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        command.DependencyInstance.Map(a => a.Block());

        return Task.FromResult(ExecutionResult.Success());
    }
}
