using Xpandables.Net.Responsibilities;
using Xpandables.Net.Responsibilities.Decorators;

namespace Xpandables.Net.Api.Accounts.Endpoints.WithdrawAccount;

public sealed record WithdrawAccountCommand : ICommand, IApplyUnitOfWork
{
    public required Guid KeyId { get; init; }
    public required decimal Amount { get; init; }
}
