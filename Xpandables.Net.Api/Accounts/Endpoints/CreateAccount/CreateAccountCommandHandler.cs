using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Api.Accounts.Endpoints.CreateAccount;

public sealed class CreateAccountCommandHandler(
    IAggregateStore<Account> aggregateStore) :
    IRequestHandler<CreateAccountCommand>
{
    public async Task<ExecutionResult> HandleAsync(
        CreateAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        Account account = Account.Create(command.KeyId);

        await aggregateStore
            .AppendAsync(account, cancellationToken)
            .ConfigureAwait(false);

        return ExecutionResults.Created().Build();
    }
}
