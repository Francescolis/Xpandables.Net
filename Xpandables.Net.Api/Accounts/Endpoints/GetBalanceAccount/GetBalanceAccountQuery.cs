using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetBalanceAccount;

public sealed record GetBalanceAccountQuery : IQuery<decimal>
{
    public required Guid KeyId { get; init; }
}
