using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Xpandables.Net.Executions.Domains;

namespace Xpandables.Net.Api.Accounts.Events;

public sealed record AccountBlocked : DomainEvent<Account>
{
    [JsonConstructor]
    public AccountBlocked() { }

    [SetsRequiredMembers]
    public AccountBlocked(Account account) : base(account) { }
}