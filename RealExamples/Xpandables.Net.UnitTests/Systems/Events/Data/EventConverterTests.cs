/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
********************************************************************************/
using System.Cache;
using System.Events.Data;
using System.Events.Domain;
using System.Events.Integration;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using FluentAssertions;

using Xpandables.Net.UnitTests.Systems.Events;

namespace Xpandables.Net.UnitTests.Systems.Events.Data;

public sealed class EventConverterDomainTests
{
	private static readonly JsonSerializerOptions s_options = new(JsonSerializerDefaults.Web)
	{
		TypeInfoResolver = JsonTypeInfoResolver.Combine(
			EventConverterTestJsonContext.Default)
	};

	private readonly TestCacheTypeResolver _typeResolver = new();
	private readonly TestEventConverterContext _converterContext = new(s_options);

	[Fact]
	public void ConvertEventToData_ShouldSerializeDomainEvent()
	{
		// Arrange
		var sut = new EventConverterDomain(_typeResolver, _converterContext);
		var accountId = Guid.NewGuid();
		var @event = new AccountOpened
		{
			StreamId = accountId,
			OwnerName = "Ada Lovelace",
			Amount = 500m,
			StreamVersion = 0
		};

		// Act
		DataEventDomain data = sut.ConvertEventToData(@event);

		// Assert
		data.KeyId.Should().Be(@event.EventId);
		data.StreamId.Should().Be(accountId);
		data.StreamVersion.Should().Be(0);
		data.EventName.Should().NotBeNullOrWhiteSpace();
		data.EventData.Should().Contain("Ada Lovelace");
		data.EventData.Should().Contain("500");
	}

	[Fact]
	public void ConvertDataToEvent_ShouldDeserializeDomainEvent()
	{
		// Arrange
		var sut = new EventConverterDomain(_typeResolver, _converterContext);
		var accountId = Guid.NewGuid();
		var original = new AccountOpened
		{
			StreamId = accountId,
			OwnerName = "Charles Babbage",
			Amount = 1000m,
			StreamVersion = 3
		};

		DataEventDomain data = sut.ConvertEventToData(original);

		// Act
		IDomainEvent restored = sut.ConvertDataToEvent(data);

		// Assert
		restored.Should().BeOfType<AccountOpened>();
		var typed = (AccountOpened)restored;
		typed.StreamId.Should().Be(accountId);
		typed.OwnerName.Should().Be("Charles Babbage");
		typed.Amount.Should().Be(1000m);
		typed.StreamVersion.Should().Be(3);
	}

	[Fact]
	public void RoundTrip_ShouldPreserveCorrelationAndCausationIds()
	{
		// Arrange
		var sut = new EventConverterDomain(_typeResolver, _converterContext);
		var @event = new MoneyDeposited
		{
			StreamId = Guid.NewGuid(),
			Amount = 42m,
			StreamVersion = 1,
			CorrelationId = "corr-123",
			CausationId = "cause-456"
		};

		// Act
		DataEventDomain data = sut.ConvertEventToData(@event);

		// Assert
		data.CorrelationId.Should().Be("corr-123");
		data.CausationId.Should().Be("cause-456");
	}
}

public sealed class EventConverterOutboxTests
{
	private static readonly JsonSerializerOptions s_options = new(JsonSerializerDefaults.Web)
	{
		TypeInfoResolver = JsonTypeInfoResolver.Combine(
			EventConverterTestJsonContext.Default)
	};

	private readonly TestCacheTypeResolver _typeResolver = new();
	private readonly TestEventConverterContext _converterContext = new(s_options);

	[Fact]
	public void ConvertEventToData_ShouldSerializeIntegrationEvent()
	{
		// Arrange
		var sut = new EventConverterOutbox(_typeResolver, _converterContext);
		var @event = new TestOrderPlacedEvent
		{
			OrderId = Guid.NewGuid(),
			Total = 99.99m
		};

		// Act
		DataEventOutbox data = sut.ConvertEventToData(@event);

		// Assert
		data.KeyId.Should().Be(@event.EventId);
		data.EventName.Should().NotBeNullOrWhiteSpace();
		data.EventData.Should().Contain("99.99");
	}

	[Fact]
	public void RoundTrip_ShouldDeserializeIntegrationEvent()
	{
		// Arrange
		var sut = new EventConverterOutbox(_typeResolver, _converterContext);
		var original = new TestOrderPlacedEvent
		{
			OrderId = Guid.NewGuid(),
			Total = 250m,
			CorrelationId = "corr-outbox-1"
		};

		DataEventOutbox data = sut.ConvertEventToData(original);

		// Act
		IIntegrationEvent restored = sut.ConvertDataToEvent(data);

		// Assert
		restored.Should().BeOfType<TestOrderPlacedEvent>();
		var typed = (TestOrderPlacedEvent)restored;
		typed.OrderId.Should().Be(original.OrderId);
		typed.Total.Should().Be(250m);
	}
}

// ── Test fixtures ──────────────────────────────────────────────────────

public sealed record TestOrderPlacedEvent : IntegrationEvent
{
	public Guid OrderId { get; init; }
	public decimal Total { get; init; }
}

/// <summary>
/// Minimal <see cref="ICacheTypeResolver"/> that maps known event names to types.
/// </summary>
internal sealed class TestCacheTypeResolver : ICacheTypeResolver
{
	public Type Resolve(string eventName)
	{
		return TryResolve(eventName)
			?? throw new InvalidOperationException($"Unknown event name: {eventName}");
	}

	public Type? TryResolve(string eventName)
	{
		return eventName switch
		{
			_ when eventName.Contains(nameof(AccountOpened), StringComparison.OrdinalIgnoreCase) => typeof(AccountOpened),
			_ when eventName.Contains(nameof(MoneyDeposited), StringComparison.OrdinalIgnoreCase) => typeof(MoneyDeposited),
			_ when eventName.Contains(nameof(TestOrderPlacedEvent), StringComparison.OrdinalIgnoreCase) => typeof(TestOrderPlacedEvent),
			_ => null
		};
	}
}

/// <summary>
/// Minimal <see cref="IEventConverterContext"/> backed by a <see cref="JsonSerializerOptions"/>.
/// </summary>
internal sealed class TestEventConverterContext(JsonSerializerOptions options) : IEventConverterContext
{
	public JsonSerializerOptions SerializerOptions => options;

	public JsonTypeInfo ResolveJsonTypeInfo(Type eventType) =>
		options.GetTypeInfo(eventType);
}

[JsonSourceGenerationOptions(
	PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(AccountOpened))]
[JsonSerializable(typeof(MoneyDeposited))]
[JsonSerializable(typeof(TestOrderPlacedEvent))]
internal sealed partial class EventConverterTestJsonContext : JsonSerializerContext;
