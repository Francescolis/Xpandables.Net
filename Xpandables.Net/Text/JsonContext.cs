using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Xpandables.Net.Collections;
using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Domains; // For Core event types like DomainEvent, IntegrationEvent, SnapshotEvent, EmptyAggregateRoot
using Xpandables.Net.Executions.Tasks; // For the base Event type
using Xpandables.Net.Optionals;
using Xpandables.Net.Executions.Rests; // For TokenValue, RefreshTokenValue, RestResponse, and Rest Enums
// Removed: using Xpandables.Net.Api.Accounts;
// Removed: using Xpandables.Net.Api.Accounts.Events;
// Removed: using System.Text.Json.Serialization.Metadata; // Not needed if EventTypeInfoMap is removed

namespace Xpandables.Net.Text;

// Core types from Xpandables.Net
[JsonSerializable(typeof(ExecutionResult))]
[JsonSerializable(typeof(Xpandables.Net.Collections.ElementEntry))]
[JsonSerializable(typeof(Xpandables.Net.Executions.ErrorMessagePoco))]
[JsonSerializable(typeof(Optional<string>))]
[JsonSerializable(typeof(Optional<object>))]
[JsonSerializable(typeof(TokenValue))]
[JsonSerializable(typeof(RefreshTokenValue))]
[JsonSerializable(typeof(RestResponse))]
[JsonSerializable(typeof(ExecutionResult<string>))] // Keep generic for basic cases
[JsonSerializable(typeof(ElementCollection))]
[JsonSerializable(typeof(Xpandables.Net.Collections.ElementEntry[]))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Operation))] // From Microsoft.AspNetCore.JsonPatch
[JsonSerializable(typeof(List<Operation>))] // From Microsoft.AspNetCore.JsonPatch
[JsonSerializable(typeof(Dictionary<string, object>))] // For ITokenDecoder and other dynamic scenarios
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(decimal))]
[JsonSerializable(typeof(Guid))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(DateTimeOffset))]

// Optional<T> types
[JsonSerializable(typeof(Optional<int>))]
[JsonSerializable(typeof(Optional<bool>))]
[JsonSerializable(typeof(Optional<decimal>))]
[JsonSerializable(typeof(Optional<Guid>))]
[JsonSerializable(typeof(Optional<DateTime>))]
[JsonSerializable(typeof(Optional<DateTimeOffset>))]
// Test specific types for Optional<T> (Removed due to project reference issues)
// [JsonSerializable(typeof(Xpandables.Net.Test.UnitTests.OptionalUnitTest.StructType))]
// [JsonSerializable(typeof(Optional<Xpandables.Net.Test.UnitTests.OptionalUnitTest.StructType>))]

// IPrimitive<TSelf, TValue> implementing types
[JsonSerializable(typeof(Xpandables.Net.Repositories.EntityStatus))]
// [JsonSerializable(typeof(Xpandables.Net.Test.UnitTests.PrimitiveUnitTest.TestPrimitive))] // Removed due to project reference issues.

// Enums from Xpandables.Net.Executions.Rests.Rest
[JsonSerializable(typeof(Rest.Method))]
[JsonSerializable(typeof(Rest.Location))]
[JsonSerializable(typeof(Rest.BodyFormat))]
[JsonSerializable(typeof(Rest.DataFormat))]

// Core Base Event Types from Xpandables.Net.Executions.Domains
[JsonSerializable(typeof(Event))]
[JsonSerializable(typeof(DomainEvent))] // Parameterless version
[JsonSerializable(typeof(IntegrationEvent))] // Parameterless version
[JsonSerializable(typeof(SnapshotEvent))]
[JsonSerializable(typeof(EmptyAggregateRoot))]


// Consider if generic base events like DomainEvent<T> are needed here
// If T is always an API-specific type, then it belongs in ApiJsonSerializerContext.
// If T could be a core type (e.g. DomainEvent<EmptyAggregateRoot>), it can stay.
[JsonSerializable(typeof(DomainEvent<EmptyAggregateRoot>))]
// Removed: [JsonSerializable(typeof(IntegrationEvent<EmptyAggregateRoot>))] as EmptyAggregateRoot is not an IDomainEvent
// Removed: [JsonSerializable(typeof(IntegrationEvent<Account>))] from previous attempts for the same reason.

// If specific IntegrationEvent<SpecificDomainEventType> are needed for core domain events, they could be added here.
// For example, if there was a core SomeCoreDomainEvent : IDomainEvent
// [JsonSerializable(typeof(IntegrationEvent<SomeCoreDomainEvent>))]


[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Converters =
    [
        typeof(ExecutionResultJsonConverter),
        typeof(ExecutionResultJsonConverterFactory),
        typeof(OptionalJsonConverterFactory),
        typeof(PrimitiveJsonConverterFactory),
        typeof(ElementCollectionJsonConverter),
        typeof(ElementEntryJsonConverter)
    ]
)]
public partial class CoreJsonSerializerContext : JsonSerializerContext
{
    // EventTypeInfoMap and its population logic are removed from here.
    // That logic, if needed, will be primarily in ApiJsonSerializerContext or passed to converters.
}
