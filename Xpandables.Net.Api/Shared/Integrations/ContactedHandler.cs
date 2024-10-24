using Xpandables.Net.Events;

namespace Xpandables.Net.Api.Shared.Integrations;

public sealed class ContactedHandler : IEventHandler<Contacted>
{
    public Task HandleAsync(Contacted @event, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
