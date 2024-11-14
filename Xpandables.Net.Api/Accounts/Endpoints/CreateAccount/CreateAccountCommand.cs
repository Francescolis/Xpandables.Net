using Xpandables.Net.Commands;
using Xpandables.Net.DataAnnotations;

namespace Xpandables.Net.Api.Accounts.Endpoints.CreateAccount;

public sealed record CreateAccountCommand : ICommand, IApplyUnitOfWork
{
    public required Guid KeyId { get; init; }
}
