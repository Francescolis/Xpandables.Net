using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Xpandables.Net.Executions.Domains;

namespace Xpandables.Net.Api.Accounts.Events;

public sealed record AccountClosed : DomainEvent<Account>
{
    [JsonConstructor]
    private AccountClosed() { }

    [SetsRequiredMembers]
    public AccountClosed(Account context) : base(context) { }
}