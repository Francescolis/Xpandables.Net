using Xpandables.Net.Responsibilities;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetBalanceAccount;

public sealed record GetBalanceAccountQuery : IQuery<decimal>
{
    public required Guid KeyId { get; init; }
}
