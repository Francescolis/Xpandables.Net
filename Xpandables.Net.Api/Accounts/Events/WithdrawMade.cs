using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Xpandables.Net.Events;

namespace Xpandables.Net.Api.Accounts.Events;

public sealed record WithdrawMade : EventDomain<Account>
{
    [JsonConstructor]
    private WithdrawMade() { }

    [SetsRequiredMembers]
    public WithdrawMade(Account account, decimal amount) :
        base(account) => Amount = amount;
    public required decimal Amount { get; init; }
}
