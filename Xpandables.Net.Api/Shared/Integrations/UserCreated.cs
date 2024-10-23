using Xpandables.Net.Events;

namespace Xpandables.Net.Api.Shared.Integrations;

public sealed record UserCreated : EventIntegration
{
    public required Guid KeyId { get; init; }
    public required string UserName { get; init; }
    public required string UserEmail { get; init; }
    public required string Password { get; init; }
}
