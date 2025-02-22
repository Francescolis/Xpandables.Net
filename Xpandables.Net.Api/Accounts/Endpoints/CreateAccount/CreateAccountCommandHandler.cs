using Xpandables.Net.Events.Aggregates;
using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.CreateAccount;

public sealed class CreateAccountCommandHandler(
    IAggregateStore<Account> aggregateStore) :
    IRequestHandler<CreateAccountCommand>
{
    public async Task<IExecutionResult> HandleAsync(
        CreateAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        Account account = Account.Create(command.KeyId);

        await aggregateStore
            .AppendAsync(account, cancellationToken)
            .ConfigureAwait(false);

        return ExecutionResults.Success();
    }
}
