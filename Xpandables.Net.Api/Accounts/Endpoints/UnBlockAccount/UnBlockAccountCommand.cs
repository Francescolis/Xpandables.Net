using Xpandables.Net.Responsibilities;
using Xpandables.Net.Responsibilities.Decorators;

namespace Xpandables.Net.Api.Accounts.Endpoints.UnBlockAccount;

public sealed record UnBlockAccountCommand : ICommand, IApplyUnitOfWork
{
    public required Guid KeyId { get; init; }
}