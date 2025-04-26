using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Xpandables.Net.Executions.Domains;

namespace Xpandables.Net.Api.Accounts.Events;

public sealed record AccountUnBlocked : DomainEvent<Account>
{
    [JsonConstructor]
    private AccountUnBlocked() { }

    [SetsRequiredMembers]
    public AccountUnBlocked(Account account) : base(account) { }
}