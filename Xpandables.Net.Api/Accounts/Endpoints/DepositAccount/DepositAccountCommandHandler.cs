using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.DepositAccount;

public sealed class DepositAccountCommandHandler : IDeciderRequestHandler<DepositAccountCommand, Account>
{
    public Task<IExecutionResult> HandleAsync(
        DepositAccountCommand command,
        Account dependency,
        CancellationToken cancellationToken = default)
    {
        dependency.Deposit(command.Amount);

        return Task.FromResult(ExecutionResults.Success());
    }
}
