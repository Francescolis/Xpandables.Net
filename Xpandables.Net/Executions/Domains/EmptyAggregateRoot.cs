namespace Xpandables.Net.Executions.Domains;

/// <summary>
/// Represents an empty aggregate root, typically used for domain events
/// that are not associated with a specific, complex aggregate entity.
/// </summary>
public sealed class EmptyAggregateRoot : Aggregate
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmptyAggregateRoot"/> class.
    /// Sets a default KeyId.
    /// </summary>
    public EmptyAggregateRoot()
    {
        KeyId = Guid.Empty; // Or some other well-known Guid if appropriate
    }
}
