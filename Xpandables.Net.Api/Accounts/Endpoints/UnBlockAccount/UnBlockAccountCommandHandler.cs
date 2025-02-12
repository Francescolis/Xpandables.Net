using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.UnBlockAccount;

public sealed class UnBlockAccountCommandHandler : ICommandHandler<UnBlockAccountCommand, Account>
{
    public async Task<IExecutionResult> HandleAsync(
        UnBlockAccountCommand command,
        Account dependency,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        dependency.UnBlock();

        return ExecutionResults.Success();
    }
}
