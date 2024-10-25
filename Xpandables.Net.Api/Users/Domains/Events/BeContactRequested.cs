using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Xpandables.Net.Events;

namespace Xpandables.Net.Api.Users.Domains.Events;

public sealed record BeContactRequested : EventDomain<User>
{
    [JsonConstructor]
    private BeContactRequested() { }

    [SetsRequiredMembers]
    public BeContactRequested(User user, ContactId contactId) :
        base(user) =>
        ContactId = contactId;

    public required ContactId ContactId { get; init; }
}
