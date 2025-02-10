
using Xpandables.Net.Commands;
using Xpandables.Net.Executions;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Api.Accounts.Endpoints.DepositAccount;

public sealed class DepositAccountCommandHandler : ICommandHandler<DepositAccountCommand, Account>
{
    public async Task<IExecutionResult> HandleAsync(
        DepositAccountCommand command,
        Account dependency,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        dependency.Deposit(command.Amount);

        return ExecutionResults.Success();
    }
}
