
using Xpandables.Net.Commands;
using Xpandables.Net.Operations;

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
