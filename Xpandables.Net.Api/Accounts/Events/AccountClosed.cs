using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Xpandables.Net.Events;

namespace Xpandables.Net.Api.Accounts.Events;

public sealed record AccountClosed : EventDomain<Account>
{
    [JsonConstructor]
    private AccountClosed() { }
    [SetsRequiredMembers]
    public AccountClosed(Account context) : base(context) { }
}
