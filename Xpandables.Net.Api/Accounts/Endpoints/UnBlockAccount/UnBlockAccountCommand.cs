using Xpandables.Net.Commands;
using Xpandables.Net.DataAnnotations;

namespace Xpandables.Net.Api.Accounts.Endpoints.UnBlockAccount;

public sealed record UnBlockAccountCommand : ICommand, IApplyUnitOfWork
{
    public required Guid KeyId { get; init; }
}