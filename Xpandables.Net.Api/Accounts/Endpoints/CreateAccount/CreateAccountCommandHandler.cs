using Xpandables.Net.Api.Accounts.Events;
using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Domains;
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

        return ExecutionResult.Created().Build();
    }
}

public sealed class CreateAccountPreCommandHandler(IEventStore eventStore) :
    IRequestPreHandler<CreateAccountCommand>
{
    public async Task<ExecutionResult> HandleAsync(
        RequestContext<CreateAccountCommand> context,
        CancellationToken cancellationToken = default)
    {
        Func<IQueryable<EntityDomainEvent>, IAsyncQueryable<IDomainEvent>> domainFilterFunc = query =>
            query.Where(w => w.AggregateId == context.Request.KeyId
                             && w.EventName == nameof(AccountCreated))
                .SelectEvent()
                .OfType<IDomainEvent>();

        if (await eventStore.FetchAsync(domainFilterFunc, cancellationToken)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false) > 0)
        {
            return ExecutionResult
                .Conflict()
                .WithError("Account", "An account already exists.")
                .Build();
        }

        return ExecutionResult.Ok().Build();
    }
}
