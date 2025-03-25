using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Xpandables.Net.Executions.Domains;

namespace Xpandables.Net.Api.Accounts.Events;

public sealed record DepositMade : EventDomain<Account>
{
    [JsonConstructor]
    private DepositMade() { }

    [SetsRequiredMembers]
    public DepositMade(Account account, decimal amount) :
        base(account) => Amount = amount;

    public required decimal Amount { get; init; }
}
