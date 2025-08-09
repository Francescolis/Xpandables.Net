using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Xpandables.Net.Events;

namespace Xpandables.Net.Api.Accounts.Events;

public sealed record AccountBlocked : DomainEvent<Account>
{
    [JsonConstructor]
    private AccountBlocked() { }

    [SetsRequiredMembers]
    public AccountBlocked(Account account) : base(account) { }
}