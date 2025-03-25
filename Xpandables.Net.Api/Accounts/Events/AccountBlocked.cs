using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Xpandables.Net.Executions.Domains;

namespace Xpandables.Net.Api.Accounts.Events;

public sealed record AccountBlocked : EventDomain<Account>
{
    [JsonConstructor]
    private AccountBlocked() { }

    [SetsRequiredMembers]
    public AccountBlocked(Account account) : base(account) { }
}
