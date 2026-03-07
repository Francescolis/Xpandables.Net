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
using System.Diagnostics.CodeAnalysis;
using System.Events;
using System.Events.Data;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering ADO.NET event store services.
/// </summary>
public static class IEventDataExtensions
{
	/// <summary>
	/// </summary>
	/// <param name="services">The service collection to add the services to.</param>
	extension(IServiceCollection services)
	{
		/// <summary>
		/// Registers the default ADO.NET domain store using default entity types.
		/// </summary>
		/// <returns>The updated service collection.</returns>
		[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
		[UnconditionalSuppressMessage("Trimming", "IL2066", Justification = "ADO.NET stores are referenced explicitly.")]
		public IServiceCollection AddXDomainStore()
		{
			ArgumentNullException.ThrowIfNull(services);
			return services.AddXDomainStore<DataEventDomain, DataEventSnapshot>();
		}

		/// <summary>
		/// Registers a domain store for handling data events within the service collection.
		/// </summary>
		/// <remarks>This method is intended for use in dependency injection scenarios where data event handling is
		/// required. Ensure that the specified types meet the necessary interface requirements.</remarks>
		/// <typeparam name="TDataEventDomain">Specifies the type of the data event domain. The type must be a class that implements the IDataEventDomain
		/// interface.</typeparam>
		/// <typeparam name="TDataEventSnapshot">Specifies the type of the data event snapshot. The type must be a class that implements the IDataEventSnapshot
		/// interface.</typeparam>
		/// <returns>An IServiceCollection instance that can be used to configure additional services.</returns>
		public IServiceCollection AddXDomainStore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TDataEventDomain, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TDataEventSnapshot>()
			where TDataEventDomain : class, IDataEventDomain
			where TDataEventSnapshot : class, IDataEventSnapshot
		{
			ArgumentNullException.ThrowIfNull(services);
			return services.AddXDomainStore<DomainStore<TDataEventDomain, TDataEventSnapshot>>();
		}

		/// <summary>
		/// Registers the default ADO.NET outbox store using default entity types.
		/// </summary>
		/// <returns>The updated service collection.</returns>
		[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
		[UnconditionalSuppressMessage("Trimming", "IL2066", Justification = "ADO.NET stores are referenced explicitly.")]
		public IServiceCollection AddXOutboxStore()
		{
			ArgumentNullException.ThrowIfNull(services);
			return services.AddXOutboxStore<DataEventOutbox>();
		}

		/// <summary>
		/// Registers an outbox store for handling data events in the service collection using the specified outbox
		/// implementation.
		/// </summary>
		/// <remarks>The services collection must not be null when calling this method. This extension method is
		/// intended for use in dependency injection scenarios to support outbox patterns for data event processing.</remarks>
		/// <typeparam name="TDataEventOutbox">The type of the data event outbox to register. Must be a class that implements the IDataEventOutbox interface.</typeparam>
		/// <returns>The updated IServiceCollection instance with the outbox store registration, enabling method chaining.</returns>
		public IServiceCollection AddXOutboxStore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TDataEventOutbox>()
			where TDataEventOutbox : class, IDataEventOutbox
		{
			ArgumentNullException.ThrowIfNull(services);
			return services.AddXOutboxStore<OutboxStore<TDataEventOutbox>>();
		}

		/// <summary>
		/// Registers the default ADO.NET inbox store using default entity types.
		/// </summary>
		/// <returns>The updated service collection.</returns>
		[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
		[UnconditionalSuppressMessage("Trimming", "IL2066", Justification = "ADO.NET stores are referenced explicitly.")]
		public IServiceCollection AddXInboxStore()
		{
			ArgumentNullException.ThrowIfNull(services);
			return services.AddXInboxStore<DataEventInbox>();
		}

		/// <summary>
		/// Registers an inbox store for the specified data event inbox type in the dependency injection container.
		/// </summary>
		/// <remarks>Use this method to set up the inbox store for handling data events in an application. The type
		/// parameter TDataEventInbox must be a class and implement IDataEventInbox. This method is typically called during
		/// application startup to enable event inbox functionality.</remarks>
		/// <typeparam name="TDataEventInbox">The type of the data event inbox to register. Must be a class that implements the IDataEventInbox interface.</typeparam>
		/// <returns>An IServiceCollection instance that can be used to configure additional services.</returns>
		public IServiceCollection AddXInboxStore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TDataEventInbox>()
			where TDataEventInbox : class, IDataEventInbox
		{
			ArgumentNullException.ThrowIfNull(services);
			return services.AddXInboxStore<InboxStore<TDataEventInbox>>();
		}

		/// <summary>
		/// Registers default event stores (domain store, outbox store, inbox store).
		/// </summary>
		/// <returns>The updated service collection.</returns>
		[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
		[RequiresUnreferencedCode("ADO.NET event store registration uses generic instantiation.")]
		public IServiceCollection AddXEventStores()
		{
			ArgumentNullException.ThrowIfNull(services);
			return services
				.AddXDomainStore()
				.AddXOutboxStore()
				.AddXInboxStore();
		}

		/// <summary>
		/// Registers a singleton event converter for the specified data event and event types in the service collection.
		/// </summary>
		/// <remarks>This method adds the specified event converter as a singleton to the service collection, allowing
		/// it to be resolved via dependency injection. Ensure that the provided types satisfy the required interface
		/// constraints.</remarks>
		/// <typeparam name="TDataEvent">The type of the data event. Must implement the IDataEvent interface.</typeparam>
		/// <typeparam name="TEvent">The type of the event. Must implement the IEvent interface.</typeparam>
		/// <typeparam name="TEventConverter">The type of the event converter. Must implement the <see cref="IEventConverter{TDataEvent, TEvent}"/> interface and have a
		/// public constructor.</typeparam>
		/// <returns>The updated IServiceCollection instance, enabling method chaining.</returns>
		public IServiceCollection AddXEventConverter<TDataEvent, TEvent, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEventConverter>()
			where TDataEvent : class, IDataEvent
			where TEvent : class, IEvent
			where TEventConverter : class, IEventConverter<TDataEvent, TEvent>
		{
			ArgumentNullException.ThrowIfNull(services);
			services.AddSingleton<IEventConverter<TDataEvent, TEvent>, TEventConverter>();
			services.AddSingleton<IEventConverter>(sp => sp.GetRequiredService<IEventConverter<TDataEvent, TEvent>>());
			return services;
		}

		/// <summary>
		/// Registers event converter implementations found in the specified assemblies with the dependency injection
		/// container.
		/// </summary>
		/// <remarks>This method dynamically discovers and registers all non-generic, sealed classes that implement
		/// the IEventConverter interface from the provided assemblies. Dynamic code generation and reflection are required,
		/// which may involve unreferenced code. All discovered converters are registered as singletons.</remarks>
		/// <param name="assemblies">An array of assemblies to scan for classes implementing the IEventConverter interface. If no assemblies are
		/// provided, the calling assembly is used.</param>
		/// <returns>The updated IServiceCollection instance, allowing for method chaining.</returns>
		[RequiresDynamicCode("Dynamic code generation is required for this method.")]
		[RequiresUnreferencedCode("Calls MakeGenericMethod which may require unreferenced code.")]
		public IServiceCollection AddXEventConverters(params IEnumerable<Assembly> assemblies)
		{
			ArgumentNullException.ThrowIfNull(assemblies);
			Assembly[] assembliesArray = assemblies as Assembly[] ?? [.. assemblies];
			assembliesArray = assembliesArray is { Length: > 0 } ? assembliesArray : [typeof(EventConverterDomain).Assembly];

			IEnumerable<(Type InterfaceType, Type ImplementationType)> conversters = assembliesArray
				.SelectMany(a => a.GetTypes())
				.Where(t => t is
				{
					IsSealed: true,
					IsAbstract: false,
					IsClass: true,
					IsGenericType: false,
					IsGenericTypeDefinition: false
				})
				.SelectMany(t => t.GetInterfaces()
					.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventConverter<,>))
					.Select(i => (InterfaceType: i, ImplementationType: t)));

			foreach ((Type interfaceType, Type implementationType) in conversters)
			{
				services.AddSingleton(interfaceType, implementationType);
				services.AddSingleton(typeof(IEventConverter), sp => sp.GetRequiredService(interfaceType));
			}

			return services;
		}

		/// <summary>
		/// Registers the specified event converter factory type as a singleton implementation of <see
		/// cref="IEventConverterProvider"/> in the service collection.
		/// </summary>
		/// <remarks>If an <see cref="IEventConverterProvider"/> service is already registered, this method
		/// does not overwrite the existing registration. This method is typically used to enable custom event
		/// conversion logic in applications that consume event data.</remarks>
		/// <typeparam name="TEventConverterProvider">The type of the event converter provider to register. Must be a class that implements <see
		/// cref="IEventConverterProvider"/> and has a public constructor.</typeparam>
		/// <returns>The <see cref="IServiceCollection"/> instance with the event converter provider registration added.</returns>
		public IServiceCollection AddXEventConverterProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEventConverterProvider>()
			where TEventConverterProvider : class, IEventConverterProvider
		{
			ArgumentNullException.ThrowIfNull(services);
			services.TryAddSingleton<IEventConverterProvider, TEventConverterProvider>();
			return services;
		}

		/// <summary>
		/// Adds a singleton implementation of IEventConverterContext to the service collection using the specified
		/// context type.
		/// </summary>
		/// <remarks>If an IEventConverterContext service is already registered, this method does not
		/// overwrite the existing registration. This method is typically used during application startup to configure
		/// event conversion services for dependency injection.</remarks>
		/// <typeparam name="TEventConverterContext">The type that implements IEventConverterContext to be registered as a singleton. Must have a public
		/// constructor.</typeparam>
		/// <returns>The IServiceCollection instance with the IEventConverterContext service registered.</returns>
		public IServiceCollection AddXEventConverterContext<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEventConverterContext>()
			where TEventConverterContext : class, IEventConverterContext
		{
			ArgumentNullException.ThrowIfNull(services);
			services.TryAddSingleton<IEventConverterContext, TEventConverterContext>();
			return services;
		}

		/// <summary>
		/// Adds the default implementation of the event converter context to the service collection.
		/// </summary>
		/// <remarks>This method registers <see cref="IEventConverterContext"/> as a singleton service if
		/// it has not already been registered. Call this method during application startup to enable event conversion
		/// features.</remarks>
		/// <returns>The current <see cref="IServiceCollection"/> instance for method chaining.</returns>
		public IServiceCollection AddXEventConverterContext()
		{
			ArgumentNullException.ThrowIfNull(services);
			services.AddXEventConverterContext<DefaultEventConverterContext>();
			return services;
		}

		/// <summary>
		/// Adds the default Event converter factory to the service collection.
		/// </summary>
		/// <remarks>Use this method to enable Event conversion capabilities in the application's
		/// dependency injection container. This is typically required for components that process or convert Event
		/// data.</remarks>
		/// <returns>The updated service collection with the Event converter provider registered.</returns>
		public IServiceCollection AddXEventConverterProvider() =>
			services.AddXEventConverterProvider<EventConverterProvider>();
	}
}
