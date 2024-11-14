using Xpandables.Net.Commands;
using Xpandables.Net.DataAnnotations;

namespace Xpandables.Net.Api.Accounts.Endpoints.WithdrawAccount;

public sealed record WithdrawAccountCommand : ICommand, IApplyUnitOfWork
{
    public required Guid KeyId { get; init; }
    public required decimal Amount { get; init; }
}
