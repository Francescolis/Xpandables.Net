using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.UnBlockAccount;

public sealed class UnBlockAccountCommandHandler : IDeciderRequestHandler<UnBlockAccountCommand, Account>
{
    public Task<IExecutionResult> HandleAsync(
        UnBlockAccountCommand command,
        Account dependency,
        CancellationToken cancellationToken = default)
    {
        dependency.UnBlock();

        return Task.FromResult(ExecutionResults.Success());
    }
}
