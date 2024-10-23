using Xpandables.Net.Events;

namespace Xpandables.Net.Api.Shared.Integrations;

public sealed record Contacted : EventIntegration
{
    public required Guid UserId { get; init; }
    public required Guid ContactId { get; init; }
}
