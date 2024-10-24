using Xpandables.Net.Events;

namespace Xpandables.Net.Api.Users.Domains.Events;

public sealed record UserAdded : EventDomain
{
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
}
