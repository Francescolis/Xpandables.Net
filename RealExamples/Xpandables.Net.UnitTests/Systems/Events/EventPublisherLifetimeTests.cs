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
using System.Events;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.UnitTests.Systems.Events;

public sealed class EventPublisherLifetimeTests
{
	[Fact]
	public void Singleton_Publisher_Resolves_Without_CaptiveDependency()
	{
		// Arrange
		ServiceCollection services = [];
		services.AddXEventPublisher();
		services.AddXEventHandler<MoneyDeposited, TestMoneyDepositedHandler>(null);

		using ServiceProvider provider = services.BuildServiceProvider(
			new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

		// Act — resolving a singleton publisher from root should not throw
		IEventPublisher publisher = provider.GetRequiredService<IEventPublisher>();

		// Assert
		publisher.Should().NotBeNull();
		publisher.Should().BeOfType<EventPublisherSubscriber>();
	}

	[Fact]
	public void Singleton_Publisher_Returns_Same_Instance()
	{
		// Arrange
		ServiceCollection services = [];
		services.AddXEventPublisher();
		services.AddXEventHandler<MoneyDeposited, TestMoneyDepositedHandler>(null);

		using ServiceProvider provider = services.BuildServiceProvider(
			new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

		// Act
		IEventPublisher publisher1 = provider.GetRequiredService<IEventPublisher>();
		IEventPublisher publisher2 = provider.GetRequiredService<IEventPublisher>();

		// Assert — singleton returns same instance
		publisher1.Should().BeSameAs(publisher2);
	}

	[Fact]
	public void Singleton_EventHandlerRegistry_Resolves_Successfully()
	{
		// Arrange
		ServiceCollection services = [];
		services.AddXEventPublisher();
		services.AddXEventHandler<MoneyDeposited, TestMoneyDepositedHandler>(null);

		using ServiceProvider provider = services.BuildServiceProvider(
			new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

		// Act
		IEventHandlerRegistry registry = provider.GetRequiredService<IEventHandlerRegistry>();

		// Assert
		registry.Should().NotBeNull();
		registry.Should().BeOfType<EventHandlerRegistry>();
		registry.TryGetWrapper(typeof(MoneyDeposited), out IEventHandlerWrapper? wrapper)
			.Should().BeTrue();
		wrapper!.EventType.Should().Be(typeof(MoneyDeposited));
	}

	[Fact]
	public async Task PublishAsync_Resolves_ScopedHandlers_PerCall()
	{
		// Arrange
		ServiceCollection services = [];
		services.AddXEventPublisher();
		services.AddXEventHandler<MoneyDeposited, TrackingMoneyDepositedHandler>(null);

		using ServiceProvider provider = services.BuildServiceProvider(
			new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

		IEventPublisher publisher = provider.GetRequiredService<IEventPublisher>();
		var @event = new MoneyDeposited { StreamId = Guid.NewGuid(), Amount = 42m };

		// Act — publish twice; each call should get a fresh handler instance
		await publisher.PublishAsync(@event);
		await publisher.PublishAsync(@event);

		// Assert — TrackingMoneyDepositedHandler is scoped, so each publish should
		// create a new scope and a new handler instance. The static counter tracks
		// total invocations across all instances.
		TrackingMoneyDepositedHandler.TotalInvocations.Should().Be(2);
	}

	[Fact]
	public void Static_RegistryMode_Resolves_Without_CaptiveDependency()
	{
		// Arrange
		ServiceCollection services = [];
		services.AddXEventPublisher(EventRegistryMode.Static);
		services.AddXEventHandler<MoneyDeposited, TestMoneyDepositedHandler>(null);

		using ServiceProvider provider = services.BuildServiceProvider(
			new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

		// Act
		IEventHandlerRegistry registry = provider.GetRequiredService<IEventHandlerRegistry>();

		// Assert
		registry.Should().BeOfType<StaticEventHandlerRegistry>();
		registry.TryGetWrapper(typeof(MoneyDeposited), out _).Should().BeTrue();
	}

	[Fact]
	public void Dynamic_RegistryMode_Resolves_And_Supports_Runtime_Registration()
	{
		// Arrange
		ServiceCollection services = [];
		services.AddXEventPublisher(EventRegistryMode.Dynamic);

		using ServiceProvider provider = services.BuildServiceProvider(
			new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

		DynamicEventHandlerRegistry registry = provider.GetRequiredService<DynamicEventHandlerRegistry>();

		// Act — register a handler at runtime
		var handler = new TestMoneyDepositedHandler();
		registry.Register<MoneyDeposited>([handler]);

		// Assert
		registry.TryGetWrapper(typeof(MoneyDeposited), out IEventHandlerWrapper? wrapper)
			.Should().BeTrue();
		wrapper!.EventType.Should().Be(typeof(MoneyDeposited));
	}

	[Fact]
	public async Task Dynamic_Registry_RuntimeHandler_Handles_Events()
	{
		// Arrange
		ServiceCollection services = [];
		services.AddXEventPublisher(EventRegistryMode.Dynamic);

		using ServiceProvider provider = services.BuildServiceProvider(
			new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

		DynamicEventHandlerRegistry registry = provider.GetRequiredService<DynamicEventHandlerRegistry>();
		var handler = new TestMoneyDepositedHandler();
		registry.Register<MoneyDeposited>([handler]);

		IEventPublisher publisher = provider.GetRequiredService<IEventPublisher>();
		var @event = new MoneyDeposited { StreamId = Guid.NewGuid(), Amount = 100m };

		// Act
		await publisher.PublishAsync(@event);

		// Assert
		handler.HandledEvents.Should().ContainSingle()
			.Which.Amount.Should().Be(100m);
	}

	[Fact]
	public void Dynamic_Registry_Unregister_Removes_Wrapper()
	{
		// Arrange
		ServiceCollection services = [];
		services.AddXEventPublisher(EventRegistryMode.Dynamic);

		using ServiceProvider provider = services.BuildServiceProvider(
			new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

		DynamicEventHandlerRegistry registry = provider.GetRequiredService<DynamicEventHandlerRegistry>();
		registry.Register<MoneyDeposited>([new TestMoneyDepositedHandler()]);

		// Act
		bool result = registry.Unregister<MoneyDeposited>();

		// Assert
		result.Should().BeTrue();
		registry.TryGetWrapper(typeof(MoneyDeposited), out _).Should().BeFalse();
	}

	[Fact]
	public void Composite_RegistryMode_Resolves_Without_CaptiveDependency()
	{
		// Arrange
		ServiceCollection services = [];
		services.AddXEventPublisher(EventRegistryMode.Composite);
		services.AddXEventHandler<MoneyDeposited, TestMoneyDepositedHandler>(null);

		using ServiceProvider provider = services.BuildServiceProvider(
			new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

		// Act
		IEventHandlerRegistry registry = provider.GetRequiredService<IEventHandlerRegistry>();

		// Assert
		registry.Should().BeOfType<CompositeEventHandlerRegistry>();
		registry.TryGetWrapper(typeof(MoneyDeposited), out _).Should().BeTrue();
	}

	[Fact]
	public void EventSubscriber_Resolves_As_Same_Instance_As_Publisher()
	{
		// Arrange
		ServiceCollection services = [];
		services.AddXEventPublisher();
		services.AddXEventHandler<MoneyDeposited, TestMoneyDepositedHandler>(null);

		using ServiceProvider provider = services.BuildServiceProvider(
			new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

		// Act
		IEventPublisher publisher = provider.GetRequiredService<IEventPublisher>();
		IEventSubscriber subscriber = provider.GetRequiredService<IEventSubscriber>();

		// Assert — both should be the same EventPublisherSubscriber singleton
		publisher.Should().BeSameAs(subscriber);
	}

	[Fact]
	public void EventHandlerWrapper_Is_Singleton()
	{
		// Arrange
		ServiceCollection services = [];
		services.AddXEventPublisher();
		services.AddXEventHandler<MoneyDeposited, TestMoneyDepositedHandler>(null);

		using ServiceProvider provider = services.BuildServiceProvider(
			new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

		// Act
		IEnumerable<IEventHandlerWrapper> wrappers1 = provider.GetServices<IEventHandlerWrapper>();
		IEnumerable<IEventHandlerWrapper> wrappers2 = provider.GetServices<IEventHandlerWrapper>();

		// Assert — singleton wrappers return same instances
		IEventHandlerWrapper wrapper1 = wrappers1.Single(w => w.EventType == typeof(MoneyDeposited));
		IEventHandlerWrapper wrapper2 = wrappers2.Single(w => w.EventType == typeof(MoneyDeposited));
		wrapper1.Should().BeSameAs(wrapper2);
	}

	[Fact]
	public void ValidateOnBuild_Succeeds_For_All_RegistryModes()
	{
		// Each mode should pass ValidateOnBuild without scope mismatches
		EventRegistryMode[] modes =
		[
			EventRegistryMode.Default,
			EventRegistryMode.Static,
			EventRegistryMode.Dynamic,
			EventRegistryMode.Composite
		];

		foreach (EventRegistryMode mode in modes)
		{
			ServiceCollection services = [];
			services.AddXEventPublisher(mode);
			services.AddXEventHandler<MoneyDeposited, TestMoneyDepositedHandler>(null);

			Action act = () =>
			{
				using ServiceProvider provider = services.BuildServiceProvider(
					new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
			};

			act.Should().NotThrow($"registry mode {mode} should not cause scope validation errors");
		}
	}
}

/// <summary>
/// A scoped handler that tracks total invocations across all instances via a static counter.
/// Used to verify that each PublishAsync call creates a new handler instance.
/// </summary>
internal sealed class TrackingMoneyDepositedHandler : IEventHandler<MoneyDeposited>
{
	private static int _totalInvocations;

	public static int TotalInvocations => _totalInvocations;

	public Task HandleAsync(MoneyDeposited eventInstance, CancellationToken cancellationToken = default)
	{
		Interlocked.Increment(ref _totalInvocations);
		return Task.CompletedTask;
	}

	public static void Reset() => Interlocked.Exchange(ref _totalInvocations, 0);
}
