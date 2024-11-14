using Xpandables.Net.Commands;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetBalanceAccount;

public sealed record GetBalanceAccountQuery : IQuery<decimal>
{
    public required Guid KeyId { get; init; }
}
