using System.Linq.Expressions;
using System.Text.Json;

using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Repositories.Filters;

/// <summary>
/// Represents a filter for event entity.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TEvent">The type of the event.</typeparam>
public abstract record EntityEventFilter<TEntity, TEvent> :
    EntityFilter<TEntity>,
    IEventFilter<TEntity>
    where TEntity : class, IEntityEvent
    where TEvent : class, IEvent
{
    /// <inheritdoc />
    public Type EventType => typeof(TEvent);

    /// <inheritdoc />
    public Expression<Func<JsonDocument, bool>>? EventDataPredicate { get; init; }
}