using Xpandables.Net.Responsibilities;
using Xpandables.Net.Responsibilities.Decorators;

namespace Xpandables.Net.Api.Accounts.Endpoints.DepositAccount;

public sealed record DepositAccountCommand : ICommand, IApplyUnitOfWork
{
    public required Guid KeyId { get; init; }
    public required decimal Amount { get; init; }
}
