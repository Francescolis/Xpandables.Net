using Xpandables.Net.Commands;
using Xpandables.Net.DataAnnotations;

namespace Xpandables.Net.Api.Accounts.Endpoints.BlockAccount;

public sealed record BlockAccountCommand : ICommand, IApplyUnitOfWork
{
    public required Guid KeyId { get; init; }
}
