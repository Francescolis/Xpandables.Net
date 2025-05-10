using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.DepositAccount;

public sealed record DepositAccountCommand : DependencyRequest<Account>, IUnitOfWorkApplied, IAggregateAppended, IAggregateResolved
{
    public required decimal Amount { get; init; }
}
