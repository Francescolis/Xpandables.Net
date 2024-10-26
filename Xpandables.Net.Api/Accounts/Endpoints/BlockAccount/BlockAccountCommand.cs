using Xpandables.Net.Responsibilities;
using Xpandables.Net.Responsibilities.Decorators;

namespace Xpandables.Net.Api.Accounts.Endpoints.BlockAccount;

public sealed record BlockAccountCommand : ICommand, IUseUnitOfWork
{
    public required Guid KeyId { get; init; }
}
