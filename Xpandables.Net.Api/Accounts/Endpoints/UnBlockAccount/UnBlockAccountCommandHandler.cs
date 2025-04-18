using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.UnBlockAccount;

public sealed class UnBlockAccountCommandHandler : IDependencyRequestHandler<UnBlockAccountCommand, Account>
{
    public Task<ExecutionResult> HandleAsync(
        UnBlockAccountCommand command,
        Account dependency,
        CancellationToken cancellationToken = default)
    {
        dependency.UnBlock();

        return Task.FromResult(ExecutionResults.Success());
    }
}
