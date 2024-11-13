using Xpandables.Net.Responsibilities;
using Xpandables.Net.Responsibilities.Decorators;

namespace Xpandables.Net.Api.Accounts.Endpoints.CreateAccount;

public sealed record CreateAccountCommand : ICommand, IApplyUnitOfWork
{
    public required Guid KeyId { get; init; }
}
