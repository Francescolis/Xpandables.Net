using Xpandables.Net.Commands;
using Xpandables.Net.DataAnnotations;

namespace Xpandables.Net.Api.Accounts.Endpoints.DepositAccount;

public sealed record DepositAccountCommand : ICommand, IApplyUnitOfWork
{
    public required Guid KeyId { get; init; }
    public required decimal Amount { get; init; }
}
