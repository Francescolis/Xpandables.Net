using Xpandables.Net.Events;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Api.Shared.Integrations;

public sealed class ContactedHandler : IEventHandler<Contacted>
{
    public Task<IOperationResult> HandleAsync(Contacted @event, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
