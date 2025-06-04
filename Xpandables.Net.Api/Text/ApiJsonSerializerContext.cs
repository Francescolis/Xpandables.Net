using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.JsonPatch.Operations; // From Xpandables.Net
using Xpandables.Net.Collections; // From Xpandables.Net
using Xpandables.Net.Executions; // From Xpandables.Net
using Xpandables.Net.Executions.Domains; // From Xpandables.Net
using Xpandables.Net.Api.Accounts; // Local to Xpandables.Net.Api
using Xpandables.Net.Api.Accounts.Events; // Local to Xpandables.Net.Api
using Xpandables.Net.Optionals; // From Xpandables.Net
using Xpandables.Net.Executions.Rests; // From Xpandables.Net
using System.Text.Json.Serialization.Metadata; // Standard library
using Xpandables.Net.Executions.Tasks; // For base Event type
using Xpandables.Net.Text; // For JsonConverters from Xpandables.Net
// For ElementCollectionJsonConverter and ElementEntryJsonConverter - already imported via Xpandables.Net.Collections
using Xpandables.Net.Api.Accounts.Endpoints.GetOperationsAccount; // For OperationAccount
using Xpandables.Net.Test.Models; // For Monkey - if tests are to be AOT compatible via this context

// Namespace for the Api project's context
namespace Xpandables.Net.Api.Text;

// Base types from Xpandables.Net and standard/ASP.NET types
[JsonSerializable(typeof(ExecutionResult))]
[JsonSerializable(typeof(Xpandables.Net.Collections.ElementEntry))]
[JsonSerializable(typeof(Xpandables.Net.Executions.ErrorMessagePoco))]
[JsonSerializable(typeof(Optional<string>))]
[JsonSerializable(typeof(Optional<object>))]
[JsonSerializable(typeof(TokenValue))] // Assuming TokenValue is in Xpandables.Net.Executions.Rests or similar
[JsonSerializable(typeof(RefreshTokenValue))] // Assuming RefreshTokenValue is in Xpandables.Net.Executions.Rests or similar
[JsonSerializable(typeof(RestResponse))]
[JsonSerializable(typeof(ExecutionResult<string>))]
[JsonSerializable(typeof(ElementCollection))]
[JsonSerializable(typeof(Xpandables.Net.Collections.ElementEntry[]))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Operation))]
[JsonSerializable(typeof(List<Operation>))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(decimal))] // For GetBalance

// OperationAccount and related types
[JsonSerializable(typeof(OperationAccount))]
[JsonSerializable(typeof(IAsyncEnumerable<OperationAccount>))]

// Monkey and related types (from Test project)
[JsonSerializable(typeof(Monkey))]
[JsonSerializable(typeof(IAsyncEnumerable<Monkey>))]

// RestResponse<T> types based on identified TResults
[JsonSerializable(typeof(RestResponse<string>))]
[JsonSerializable(typeof(RestResponse<decimal>))]
[JsonSerializable(typeof(RestResponse<OperationAccount>))] // If a single OperationAccount can be a response
[JsonSerializable(typeof(RestResponse<IAsyncEnumerable<OperationAccount>>))]
[JsonSerializable(typeof(RestResponse<Monkey>))] // If a single Monkey can be a response
[JsonSerializable(typeof(RestResponse<IAsyncEnumerable<Monkey>>))]
// [JsonSerializable(typeof(RestResponse<object>))] // Already covered by Optional<object> effectively? Let's be explicit if needed by composer.

// Domain Events & Related (local to Xpandables.Net.Api or from Xpandables.Net)
[JsonSerializable(typeof(AccountBlocked))]
[JsonSerializable(typeof(AccountClosed))]
[JsonSerializable(typeof(AccountCreated))]
[JsonSerializable(typeof(AccountUnBlocked))]
[JsonSerializable(typeof(DepositMade))]
[JsonSerializable(typeof(WithdrawMade))]
[JsonSerializable(typeof(Account))]
[JsonSerializable(typeof(EmptyAggregateRoot))] // From Xpandables.Net.Executions.Domains

// Account States (local to Xpandables.Net.Api)
[JsonSerializable(typeof(AccountState))]
[JsonSerializable(typeof(AccountStateActive))]
[JsonSerializable(typeof(AccountStateBlocked))]
[JsonSerializable(typeof(AccountStateClosed))]

// Base Event Types (from Xpandables.Net.Executions.Domains)
[JsonSerializable(typeof(DomainEvent))]
[JsonSerializable(typeof(DomainEvent<Account>))]
[JsonSerializable(typeof(DomainEvent<EmptyAggregateRoot>))]
[JsonSerializable(typeof(Event))]

// Integration Events & Related (from Xpandables.Net.Executions.Domains)
[JsonSerializable(typeof(IntegrationEvent))]
// Removed: [JsonSerializable(typeof(IntegrationEvent<Account>))] // Account is not an IDomainEvent
// Removed: [JsonSerializable(typeof(IntegrationEvent<EmptyAggregateRoot>))] // EmptyAggregateRoot is not an IDomainEvent

// Snapshot Events & Related (from Xpandables.Net.Executions.Domains)
[JsonSerializable(typeof(SnapshotEvent))]

// Concrete TResult types for RestResponse<TResult> are added above.

[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Converters =
    [
        // These converters are defined in Xpandables.Net project.
        // Ensure they are public and accessible.
        typeof(ExecutionResultJsonConverter),
        typeof(ExecutionResultJsonConverterFactory),
        typeof(OptionalJsonConverterFactory),
        typeof(PrimitiveJsonConverterFactory),
        typeof(ElementCollectionJsonConverter), // Now covered by 'using Xpandables.Net.Collections;'
        typeof(ElementEntryJsonConverter)    // Now covered by 'using Xpandables.Net.Collections;'
    ]
)]
public partial class ApiJsonSerializerContext : JsonSerializerContext
{
    // Helper for EventConverter to map type names to JsonTypeInfo
    public static Dictionary<string, JsonTypeInfo> EventTypeInfoMap { get; } = new();

    static ApiJsonSerializerContext()
    {
        // Populate the map with generated JsonTypeInfo properties
        // Domain Events (from Xpandables.Net.Api.Accounts.Events)
        TryAddEventTypeInfo(EventTypeInfoMap, Default.AccountBlocked);
        TryAddEventTypeInfo(EventTypeInfoMap, Default.AccountCreated);
        TryAddEventTypeInfo(EventTypeInfoMap, Default.AccountClosed);
        TryAddEventTypeInfo(EventTypeInfoMap, Default.AccountUnBlocked);
        TryAddEventTypeInfo(EventTypeInfoMap, Default.DepositMade);
        TryAddEventTypeInfo(EventTypeInfoMap, Default.WithdrawMade);

        // IntegrationEvent (non-generic from Xpandables.Net.Executions.Domains)
        TryAddEventTypeInfo(EventTypeInfoMap, Default.IntegrationEvent);
        // Note: Generic IntegrationEvent<T> would need specific JsonSerializable attributes
        // for each TDomainEvent used, e.g., [JsonSerializable(typeof(IntegrationEvent<SomeDomainEvent>))]
        // and then added here like: TryAddEventTypeInfo(EventTypeInfoMap, Default.IntegrationEventSomeDomainEvent);

        // SnapshotEvent (from Xpandables.Net.Executions.Domains)
        TryAddEventTypeInfo(EventTypeInfoMap, Default.SnapshotEvent);

        // Base DomainEvent (parameterless, from Xpandables.Net.Executions.Domains)
        // This might be used if an event type is "DomainEvent" literally.
        TryAddEventTypeInfo(EventTypeInfoMap, Default.DomainEvent);
    }

    private static void TryAddEventTypeInfo<T>(Dictionary<string, JsonTypeInfo> map, JsonTypeInfo<T> typeInfo)
    {
        // Use Type.FullName for robust mapping, and Type.Name as a fallback or for simpler names if preferred.
        var fullName = typeof(T).FullName;
        if (fullName != null && !map.ContainsKey(fullName))
        {
            map[fullName] = typeInfo;
        }

        var shortName = typeof(T).Name;
        if (shortName != null && !map.ContainsKey(shortName) && (fullName == null || !fullName.EndsWith(shortName)))
        {
             map[shortName] = typeInfo;
        }
    }
}
