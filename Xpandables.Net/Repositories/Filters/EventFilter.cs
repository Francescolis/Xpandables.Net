using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.Json;

using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories.Converters;
using Xpandables.Net.Text;

namespace Xpandables.Net.Repositories.Filters;

/// <summary>
/// Represents a filter for event entity.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TEvent">The type of the event.</typeparam>
public abstract record EventFilter<TEntity, TEvent> : EntityFilter<TEntity, TEvent>, IEventFilter<TEntity, TEvent>
    where TEntity : class, IEntityEvent
    where TEvent : class, IEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventFilter{TEntity, TEvent}"/> class.
    /// </summary>
    /// <remarks>This constructor is protected and is intended to be used by derived classes to ensure that
    /// all required members are set. It calls the base class constructor.</remarks>
    [SetsRequiredMembers]
    protected EventFilter() : base() => Selector = CreateSimpleEventSelector();

    /// <inheritdoc />
    public Type EventType => typeof(TEvent);

    /// <inheritdoc />
    public Expression<Func<JsonDocument, bool>>? EventDataWhere { get; init; }

    /// <summary>
    /// Creates an EF Core-compatible selector expression that enables client-side event deserialization.
    /// </summary>
    /// <returns>An expression that selects and transforms entity data to the target event type.</returns>
    protected static Expression<Func<TEntity, TEvent>> CreateEventSelector()
    {
        var entityParameter = Expression.Parameter(typeof(TEntity), "entity");
        var eventDataProperty = Expression.Property(entityParameter, nameof(IEntityEvent.EventData));
        var eventFullNameProperty = Expression.Property(entityParameter, nameof(IEntityEvent.EventFullName));

        var deserializeMethod = typeof(EventFilter<TEntity, TEvent>)
            .GetMethod(
                nameof(DeserializeEventData),
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;

        var deserializeCall = Expression.Call(
            deserializeMethod,
            eventDataProperty,
            eventFullNameProperty,
            Expression.Constant(DefaultSerializerOptions.Defaults, typeof(JsonSerializerOptions)));

        return Expression.Lambda<Func<TEntity, TEvent>>(deserializeCall, entityParameter);
    }

    /// <summary>
    /// Alternative method that creates a simpler selector for troubleshooting.
    /// This version uses explicit casting and may work better with some EF Core configurations.
    /// </summary>
    /// <returns>A simplified selector expression.</returns>
    protected static Expression<Func<TEntity, TEvent>> CreateSimpleEventSelector() =>
        entity => DeserializeEventData(
            entity.EventData,
            entity.EventFullName,
            DefaultSerializerOptions.Defaults);

    /// <summary>
    /// Static method for deserializing event data that will be called on the client side.
    /// This method is marked as internal to allow EF Core to see it but keep it encapsulated.
    /// </summary>
    /// <param name="eventData">The JSON document containing the event data.</param>
    /// <param name="eventFullName">The full name of the concrete event type.</param>
    /// <param name="options">The JSON serializer options to use.</param>
    /// <returns>The deserialized event.</returns>
    protected static TEvent DeserializeEventData(
        JsonDocument eventData,
         string eventFullName,
        JsonSerializerOptions? options)
    {
        Type concreteEventType = Type.GetType(eventFullName)
            ?? throw new InvalidOperationException(
                $"The event type '{eventFullName}' could not be found. Ensure it is referenced and available at runtime.");

        return (TEvent)EventConverter.DeserializeEvent(eventData, concreteEventType, options);
    }
}