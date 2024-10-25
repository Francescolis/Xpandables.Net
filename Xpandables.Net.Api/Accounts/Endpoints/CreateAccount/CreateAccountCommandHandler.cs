
using Xpandables.Net.Events.Aggregates;
using Xpandables.Net.Operations;
using Xpandables.Net.Responsibilities;

namespace Xpandables.Net.Api.Accounts.Endpoints.CreateAccount;

public sealed class CreateAccountCommandHandler(
    IAggregateStore<Account> aggregateStore) :
    ICommandHandler<CreateAccountCommand>
{
    public async Task<IOperationResult> HandleAsync(
        CreateAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        Account account = Account.Create(command.KeyId);

        await aggregateStore
            .AppendAsync(account, cancellationToken)
            .ConfigureAwait(false);

        return OperationResults.Ok().Build();
    }
}
