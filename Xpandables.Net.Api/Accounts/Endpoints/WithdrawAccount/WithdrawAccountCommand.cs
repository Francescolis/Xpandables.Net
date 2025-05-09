using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.WithdrawAccount;

public sealed record WithdrawAccountCommand : IRequest, IUnitOfWorkApplied
{
    public required Guid KeyId { get; init; }
    public required decimal Amount { get; init; }
}
