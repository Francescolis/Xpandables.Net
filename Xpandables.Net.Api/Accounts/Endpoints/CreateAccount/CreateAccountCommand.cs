using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.CreateAccount;

public sealed record CreateAccountCommand : IRequest, IRequiresEventStorage, IRequiresValidation
{
    public required Guid KeyId { get; init; }
}
