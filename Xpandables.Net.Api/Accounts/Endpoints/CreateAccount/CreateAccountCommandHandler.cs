using Xpandables.Net.Api.Accounts.Events;
using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories;
using Xpandables.Net.Repositories.Filters;

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

public sealed class CreateAccountPreCommandHandler(IEventStore eventStore) :
    IRequestPreHandler<CreateAccountCommand>
{
    public async Task<ExecutionResult> HandleAsync(
        RequestContext<CreateAccountCommand> context,
        CancellationToken cancellationToken = default)
    {
        EventFilterDomain filter = new()
        {
            Where = e => e.AggregateId == context.Request.KeyId
                && e.EventName == nameof(AccountCreated),
            PageSize = 1
        };

        if (await eventStore.FetchAsync(filter, cancellationToken)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false) > 0)
        {
            return ExecutionResults
                .Conflict()
                .WithError("Account", "An account already exists.")
                .Build();
        }

        return ExecutionResults.Ok().Build();
    }
}
