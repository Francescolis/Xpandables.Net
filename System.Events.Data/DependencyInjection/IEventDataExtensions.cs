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
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Events;
using System.Events.Data;
using System.Events.Domain;
using System.Events.Integration;
using System.Reflection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering ADO.NET event store services.
/// </summary>
public static class IEventDataExtensions
{
	/// <summary>
	/// Adds the Domain store and its dependencies to the specified service collection.
	/// </summary>
	/// <remarks>This method registers the required services for Domain store support using dependency injection.
	/// If a custom IDataUnitOfWork implementation is needed, provide a factory function; otherwise, the default
	/// implementation is used.</remarks>
	/// <param name="services">The service collection to which the Domain store services will be added. Cannot be null.</param>
	/// <param name="factory">An optional factory function used to create the IDataUnitOfWork instance. If null, a default implementation is
	/// registered.</param>
	/// <returns>The service collection with the Domain store services registered. This enables further configuration via method
	/// chaining.</returns>
	[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
	[UnconditionalSuppressMessage("Trimming", "IL2066", Justification = "ADO.NET stores are referenced explicitly.")]
	public static IServiceCollection AddXDomainStore(this IServiceCollection services, Func<IServiceProvider, IDataUnitOfWork>? factory = null)
	{
		ArgumentNullException.ThrowIfNull(services);
		return services.AddXDomainStore<DataEventDomain, DataEventSnapshot>(factory);
	}

	/// <summary>
	/// Adds a domain store for the specified event domain and snapshot types to the service collection.
	/// </summary>
	/// <remarks>Registers the domain store as a scoped service. If a factory is provided, it is used to create the
	/// IDataUnitOfWork dependency for the domain store; otherwise, a default registration is used.</remarks>
	/// <typeparam name="TDataEventDomain">The type representing the event domain. Must implement IDataEventDomain and have public properties.</typeparam>
	/// <typeparam name="TDataEventSnapshot">The type representing the event snapshot. Must implement IDataEventSnapshot and have public properties.</typeparam>
	/// <param name="services">The IServiceCollection to which the domain store will be added. Cannot be null.</param>
	/// <param name="factory">An optional factory function used to create an IDataUnitOfWork instance. If null, a default implementation is
	/// registered.</param>
	/// <returns>The IServiceCollection instance with the domain store service registered.</returns>
	public static IServiceCollection AddXDomainStore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TDataEventDomain,
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TDataEventSnapshot>(
		this IServiceCollection services,
		Func<IServiceProvider, IDataUnitOfWork>? factory = null)
		where TDataEventDomain : class, IDataEventDomain
		where TDataEventSnapshot : class, IDataEventSnapshot
	{
		ArgumentNullException.ThrowIfNull(services);

		if (factory is null)
		{
			return services.AddXDomainStore<DomainStore<TDataEventDomain, TDataEventSnapshot>>();
		}

		ObjectFactory<DomainStore<TDataEventDomain, TDataEventSnapshot>> objectFactory =
			ActivatorUtilities.CreateFactory<DomainStore<TDataEventDomain, TDataEventSnapshot>>([typeof(IDataUnitOfWork)]);

		services.AddScoped<IDomainStore>(provider =>
		{
			IDataUnitOfWork unitOfWork = factory(provider);
			return objectFactory(provider, [unitOfWork]);
		});

		return services;
	}

	/// <summary>
	/// Adds the default XOutbox store implementation to the dependency injection container.
	/// </summary>
	/// <remarks>This method registers the default DataEventOutbox implementation for use with XOutbox. Use the
	/// optional factory parameter to customize the data unit of work instantiation if needed.</remarks>
	/// <param name="services">The service collection to which the XOutbox store will be added. Cannot be null.</param>
	/// <param name="factory">An optional factory function used to create the underlying data unit of work. If null, the default implementation
	/// is used.</param>
	/// <returns>The same service collection instance, enabling method chaining.</returns>
	[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
	[UnconditionalSuppressMessage("Trimming", "IL2066", Justification = "ADO.NET stores are referenced explicitly.")]
	public static IServiceCollection AddXOutboxStore(this IServiceCollection services, Func<IServiceProvider, IDataUnitOfWork>? factory = null)
	{
		ArgumentNullException.ThrowIfNull(services);
		return services.AddXOutboxStore<DataEventOutbox>(factory);
	}

	/// <summary>
	/// Registers an outbox store implementation for the specified data event outbox type in the dependency injection
	/// container.
	/// </summary>
	/// <remarks>Use this method to configure a custom or default outbox store for event publishing scenarios. If a
	/// factory is provided, it will be used to resolve the IDataUnitOfWork dependency for the outbox store.</remarks>
	/// <typeparam name="TDataEventOutbox">The type of the data event outbox to use. Must implement the IDataEventOutbox interface.</typeparam>
	/// <param name="services">The IServiceCollection to which the outbox store will be added. Cannot be null.</param>
	/// <param name="factory">An optional factory function used to create the IDataUnitOfWork instance for the outbox store. If null, a default
	/// implementation is registered.</param>
	/// <returns>The IServiceCollection instance with the outbox store service registered.</returns>
	public static IServiceCollection AddXOutboxStore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TDataEventOutbox>(
		this IServiceCollection services,
		Func<IServiceProvider, IDataUnitOfWork>? factory = null)
		where TDataEventOutbox : class, IDataEventOutbox
	{
		ArgumentNullException.ThrowIfNull(services);

		if (factory is null)
		{
			return services.AddXOutboxStore<OutboxStore<TDataEventOutbox>>();
		}

		ObjectFactory<OutboxStore<TDataEventOutbox>> objectFactory =
			ActivatorUtilities.CreateFactory<OutboxStore<TDataEventOutbox>>([typeof(IDataUnitOfWork)]);

		services.AddScoped<IOutboxStore>(provider =>
		{
			IDataUnitOfWork unitOfWork = factory(provider);
			return objectFactory(provider, [unitOfWork]);
		});

		return services;
	}

	/// <summary>
	/// Adds the default Inbox store implementation to the dependency injection container.
	/// </summary>
	/// <remarks>This method registers the DataEventInbox implementation for use with Inbox. Use the optional
	/// factory parameter to customize the IDataUnitOfWork instantiation if needed.</remarks>
	/// <param name="services">The service collection to which the XInbox store will be added. Cannot be null.</param>
	/// <param name="factory">An optional factory function used to create the IDataUnitOfWork instance. If null, the default implementation is
	/// used.</param>
	/// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
	[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
	[UnconditionalSuppressMessage("Trimming", "IL2066", Justification = "ADO.NET stores are referenced explicitly.")]
	public static IServiceCollection AddXInboxStore(this IServiceCollection services, Func<IServiceProvider, IDataUnitOfWork>? factory = null)
	{
		ArgumentNullException.ThrowIfNull(services);
		return services.AddXInboxStore<DataEventInbox>(factory);
	}

	/// <summary>
	/// Registers an implementation of IInboxStore using the specified IDataEventInbox type and a custom IDataUnitOfWork
	/// factory in the dependency injection container.
	/// </summary>
	/// <remarks>Use this method to configure a scoped IInboxStore implementation that depends on a custom
	/// IDataUnitOfWork. This is useful when the unit of work requires runtime configuration or context-specific
	/// instantiation.</remarks>
	/// <typeparam name="TDataEventInbox">The type of the data event inbox to use for the inbox store. Must implement IDataEventInbox.</typeparam>
	/// <param name="services">The IServiceCollection to add the inbox store service to. Cannot be null.</param>
	/// <param name="factory">A factory function that provides an IDataUnitOfWork instance for each service resolution. Cannot be null.</param>
	/// <returns>The IServiceCollection instance with the inbox store service registered.</returns>
	public static IServiceCollection AddXInboxStore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TDataEventInbox>(
		this IServiceCollection services,
		Func<IServiceProvider, IDataUnitOfWork>? factory = null)
		where TDataEventInbox : class, IDataEventInbox
	{
		ArgumentNullException.ThrowIfNull(services);

		if (factory is null)
		{
			return services.AddXInboxStore<InboxStore<TDataEventInbox>>();
		}

		ObjectFactory<InboxStore<TDataEventInbox>> objectFactory =
			ActivatorUtilities.CreateFactory<InboxStore<TDataEventInbox>>([typeof(IDataUnitOfWork)]);

		services.AddScoped<IInboxStore>(provider =>
		{
			IDataUnitOfWork unitOfWork = factory(provider);
			return objectFactory(provider, [unitOfWork]);
		});

		return services;
	}

	/// <summary>
	/// Registers the EventStores infrastructure, including domain, outbox, and inbox event stores, with the dependency
	/// injection container.
	/// </summary>
	/// <remarks>This method adds all required event store services for EventStores in a single call. It is
	/// typically called during application startup to configure event sourcing infrastructure.</remarks>
	/// <param name="services">The service collection to which the event store services will be added. Cannot be null.</param>
	/// <param name="factory">An optional factory function used to create the IDataUnitOfWork implementation. If null, the default registration
	/// is used.</param>
	/// <returns>The IServiceCollection instance with the event store services registered. This enables method chaining.</returns>
	[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
	[RequiresUnreferencedCode("ADO.NET event store registration uses generic instantiation.")]
	public static IServiceCollection AddXEventStores(this IServiceCollection services, Func<IServiceProvider, IDataUnitOfWork>? factory = null)
	{
		ArgumentNullException.ThrowIfNull(services);
		return services
			.AddXDomainStore(factory)
			.AddXOutboxStore(factory)
			.AddXInboxStore(factory);
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
	public static IServiceCollection AddXEventConverter<TDataEvent, TEvent, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEventConverter>(this IServiceCollection services)
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
	/// <param name="services">The IServiceCollection instance to which the event converters will be added.</param>
	/// <param name="assemblies">An array of assemblies to scan for classes implementing the IEventConverter interface. If no assemblies are
	/// provided, the calling assembly is used.</param>
	/// <returns>The updated IServiceCollection instance, allowing for method chaining.</returns>
	[RequiresDynamicCode("Dynamic code generation is required for this method.")]
	[RequiresUnreferencedCode("Calls MakeGenericMethod which may require unreferenced code.")]
	public static IServiceCollection AddXEventConverters(this IServiceCollection services, params IEnumerable<Assembly> assemblies)
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
	public static IServiceCollection AddXEventConverterProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEventConverterProvider>(this IServiceCollection services)
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
	public static IServiceCollection AddXEventConverterContext<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEventConverterContext>(this IServiceCollection services)
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
	public static IServiceCollection AddXEventConverterContext(this IServiceCollection services)
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
	public static IServiceCollection AddXEventConverterProvider(this IServiceCollection services) =>
		services.AddXEventConverterProvider<EventConverterProvider>();
}
