using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetBalanceAccount;

public sealed record GetBalanceAccountQuery : IRequest<decimal>
{
    public required Guid KeyId { get; init; }
}
