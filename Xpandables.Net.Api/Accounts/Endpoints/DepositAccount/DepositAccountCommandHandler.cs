using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.DepositAccount;

public sealed class DepositAccountCommandHandler : IDependencyRequestHandler<DepositAccountCommand, Account>
{
    public Task<ExecutionResult> HandleAsync(
        DepositAccountCommand command,
        Account dependency,
        CancellationToken cancellationToken = default)
    {
        dependency.Deposit(command.Amount);

        return Task.FromResult(ExecutionResults.Success());
    }
}
