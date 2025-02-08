using Xpandables.Net.Commands;
using Xpandables.Net.DataAnnotations;

namespace Xpandables.Net.Api.Accounts.Endpoints.DepositAccount;

public sealed record DepositAccountCommand : Command<Account>, IApplyUnitOfWork, IApplyAggregate
{
    public required decimal Amount { get; init; }
}
