/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering data unit of work and repository services with an IServiceCollection for
/// dependency injection.
/// </summary>
/// <remarks>The methods in this class enable flexible registration of IDataUnitOfWork and IDataRepository
/// implementations, supporting both keyed and non-keyed registrations, as well as configurable service lifetimes. These
/// extensions help ensure that data access patterns are consistently and correctly integrated into the application's
/// dependency injection container. Type constraints and runtime checks are used to enforce that only valid
/// implementations are registered. Some methods support automatic discovery and registration of repository types from
/// assemblies.</remarks>
public static class IDataExtensions
{
	extension(IServiceCollection services)
	{
		/// <summary>
		/// Registers the database connection factory and scope factory providers in the service collection using the
		/// specified configuration.
		/// <code language="csharp">
		///     You can use the registered providers in your services as shown below:
		///     public sealed class ReportingRepository(IDataDbConnectionScopeFactoryProvider provider)
		///     {
		///         private readonly IDataDbConnectionScope _scope = provider.CreateScope("Reporting");
		///         public Task OpenReporting()
		///         {
		///             var command = _scope.Connection.CreateCommand();
		///         }
		///     }
		/// </code>
		/// </summary>
		/// <remarks>This method is typically used in the startup configuration of an application to set
		/// up database connection management services.</remarks>
		/// <param name="configuration">The configuration settings used to initialize the database connection factory provider. Must not be null.</param>
		/// <returns>The updated IServiceCollection instance, allowing for method chaining.</returns>
		public IServiceCollection AddXDataDbConnectionFactoryProviders(IConfiguration configuration)
		{
			ArgumentNullException.ThrowIfNull(services);
			ArgumentNullException.ThrowIfNull(configuration);

			services.TryAddSingleton<IDataDbConnectionFactoryProvider>(
				_ => new DataDbConnectionFactoryProvider(configuration));
			services.TryAddSingleton<IDataDbConnectionScopeFactoryProvider, DataDbConnectionScopeFactoryProvider>();

			return services;
		}

		/// <summary>
		/// Registers a SQL Server database connection factory with the specified connection string in the service
		/// collection.
		/// </summary>
		/// <remarks>This method adds a singleton implementation of IDbConnectionFactory configured for
		/// SQL Server to the service collection. Ensure that the provided connection string is valid to avoid runtime
		/// errors when creating database connections.</remarks>
		/// <param name="connectionString">The connection string used to establish connections to the SQL Server database. This value must not be null,
		/// empty, or consist only of white-space characters.</param>
		/// <returns>The updated IServiceCollection instance, enabling method chaining.</returns>
		public IServiceCollection AddXDataDbConnectionMsSqlServer(string connectionString)
		{
			ArgumentNullException.ThrowIfNull(services);
			ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
			services.TryAddSingleton<IDataDbConnectionFactory>(
				_ => new DataDbConnectionFactory(DbProviders.MsSqlServer.InvariantName, connectionString));
			return services;
		}

		/// <summary>
		/// Registers a PostgreSQL database connection factory with the specified connection string for dependency
		/// injection.
		/// </summary>
		/// <remarks>Throws an ArgumentNullException if the services collection is null, or an
		/// ArgumentException if the connection string is null or consists only of white-space characters. Use this
		/// method during application startup to configure PostgreSQL database connectivity for services that depend on
		/// IDbConnectionFactory.</remarks>
		/// <param name="connectionString">The connection string used to establish connections to the PostgreSQL database. This value cannot be null or
		/// whitespace.</param>
		/// <returns>The updated IServiceCollection instance, enabling method chaining.</returns>
		public IServiceCollection AddXDataDbConnectionPostgreSql(string connectionString)
		{
			ArgumentNullException.ThrowIfNull(services);
			ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
			services.TryAddSingleton<IDataDbConnectionFactory>(
				_ => new DataDbConnectionFactory(DbProviders.PostgreSql.InvariantName, connectionString));
			return services;
		}

		/// <summary>
		/// Registers a MySQL database connection factory with the specified connection string for dependency injection.
		/// </summary>
		/// <remarks>This method throws an <see cref="ArgumentNullException"/> if the service collection
		/// is null, and an <see cref="ArgumentException"/> if the connection string is null or consists only of
		/// white-space characters. Use this method to configure MySQL database connectivity in applications that
		/// utilize dependency injection.</remarks>
		/// <param name="connectionString">The connection string used to establish connections to the MySQL database. This value cannot be null or
		/// whitespace.</param>
		/// <returns>The updated <see cref="IServiceCollection"/> instance, enabling method chaining.</returns>
		public IServiceCollection AddXDataDbConnectionMySql(string connectionString)
		{
			ArgumentNullException.ThrowIfNull(services);
			ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
			services.TryAddSingleton<IDataDbConnectionFactory>(
				_ => new DataDbConnectionFactory(DbProviders.MySql.InvariantName, connectionString));
			return services;
		}

		/// <summary>
		/// Registers a default database connection factory using provider invariant name and connection string.
		/// </summary>
		/// <param name="providerInvariantName">The provider invariant name.</param>
		/// <param name="connectionString">The connection string.</param>
		/// <returns>The <see cref="IServiceCollection"/> instance for chaining.</returns>
		public IServiceCollection AddXDataDbConnectionFactory(string providerInvariantName, string connectionString)
		{
			ArgumentNullException.ThrowIfNull(services);
			ArgumentException.ThrowIfNullOrWhiteSpace(providerInvariantName);
			ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

			services.TryAddSingleton<IDataDbConnectionFactory>(
				_ => new DataDbConnectionFactory(providerInvariantName, connectionString));

			return services;
		}

		/// <summary>
		/// Registers a custom database connection factory implementation.
		/// </summary>
		/// <typeparam name="TFactory">The connection factory type.</typeparam>
		/// <returns>The <see cref="IServiceCollection"/> instance for chaining.</returns>
		public IServiceCollection AddXDataDbConnectionFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFactory>()
			where TFactory : class, IDataDbConnectionFactory
		{
			ArgumentNullException.ThrowIfNull(services);
			return services.AddSingleton<IDataDbConnectionFactory, TFactory>();
		}

		/// <summary>
		/// Registers the default connection scope factory.
		/// </summary>
		/// <returns>The <see cref="IServiceCollection"/> instance for chaining.</returns>
		public IServiceCollection AddXDataDbConnectionScopeFactory()
		{
			ArgumentNullException.ThrowIfNull(services);
			return services.AddSingleton<IDataDbConnectionScopeFactory, DataDbConnectionScopeFactory>();
		}

		/// <summary>
		/// Registers a scoped database connection scope using the configured factory.
		/// </summary>
		/// <returns>The <see cref="IServiceCollection"/> instance for chaining.</returns>
		public IServiceCollection AddXDataDbConnectionScope()
		{
			ArgumentNullException.ThrowIfNull(services);
			return services.AddScoped<IDataDbConnectionScope>(sp =>
				sp.GetRequiredService<IDataDbConnectionScopeFactory>().CreateScope());
		}

		/// <summary>
		/// Registers the specified SQL mapper type as a singleton service in the dependency injection container.
		/// </summary>
		/// <remarks>This method ensures that the provided SQL mapper type is registered as a singleton,
		/// meaning a single instance will be used throughout the application's lifetime. It is important that the type
		/// parameter TSqlMapper has a public constructor to be instantiated by the dependency injection
		/// framework.</remarks>
		/// <typeparam name="TSqlMapper">The type of the SQL mapper to register. Must implement the ISqlMapper interface and have a public
		/// constructor.</typeparam>
		/// <returns>The IServiceCollection instance with the SQL mapper registration added, enabling method chaining.</returns>
		public IServiceCollection AddXDataSqlMapper<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TSqlMapper>()
			where TSqlMapper : class, IDataSqlMapper
		{
			ArgumentNullException.ThrowIfNull(services);
			return services.AddSingleton<IDataSqlMapper, TSqlMapper>();
		}

		/// <summary>
		/// Adds the SqlMapper services to the current IServiceCollection for dependency injection.
		/// </summary>
		/// <remarks>This method requires that the IServiceCollection instance is not null. It configures
		/// the SqlMapper for use within the application, allowing database mapping functionality to be injected where
		/// needed.</remarks>
		/// <returns>The IServiceCollection instance with the SqlMapper services registered. This enables method chaining for
		/// further service configuration.</returns>
		public IServiceCollection AddXDataSqlMapper()
		{
			ArgumentNullException.ThrowIfNull(services);
			return services.AddXDataSqlMapper<DataSqlMapper>();
		}

		/// <summary>
		/// Registers the SQL Server implementation of the ISqlBuilder interface in the service collection.
		/// </summary>
		/// <remarks>This method adds a singleton instance of SqlServerSqlBuilder, which provides SQL
		/// query building functionality specific to Microsoft SQL Server. The services collection must not be null when
		/// calling this method.</remarks>
		/// <returns>The IServiceCollection instance that can be used to configure additional services.</returns>
		public IServiceCollection AddXDataMsSqlBuilder()
		{
			ArgumentNullException.ThrowIfNull(services);
			return services.AddSingleton<IDataSqlBuilder, MsDataSqlBuilder>();
		}

		/// <summary>
		/// Registers the PostgreSQL implementation of the ISqlBuilder interface in the service collection.
		/// </summary>
		/// <remarks>This method adds a singleton instance of PostgreSqlBuilder to the service collection.
		/// Ensure that the service collection is not null before calling this method.</remarks>
		/// <returns>The IServiceCollection instance that can be used to configure additional services.</returns>
		public IServiceCollection AddXDataPostgreSqlBuilder()
		{
			ArgumentNullException.ThrowIfNull(services);
			return services.AddSingleton<IDataSqlBuilder, PostgreDataSqlBuilder>();
		}

		/// <summary>
		/// Registers the MySQL implementation of the ISqlBuilder interface in the service collection.
		/// </summary>
		/// <remarks>This method adds a singleton service of type ISqlBuilder with a MySqlBuilder
		/// implementation to the service collection. The method throws an ArgumentNullException if the service
		/// collection is null.</remarks>
		/// <returns>The IServiceCollection instance that can be used to configure additional services.</returns>
		public IServiceCollection AddXDataMySqlBuilder()
		{
			ArgumentNullException.ThrowIfNull(services);
			return services.AddSingleton<IDataSqlBuilder, MyDataSqlBuilder>();
		}

		/// <summary>
		/// Registers a SQL builder implementation.
		/// </summary>
		/// <typeparam name="TBuilder">The SQL builder type.</typeparam>
		/// <returns>The <see cref="IServiceCollection"/> instance for chaining.</returns>
		public IServiceCollection AddXDataSqlBuilder<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TBuilder>()
			where TBuilder : class, IDataSqlBuilder
		{
			ArgumentNullException.ThrowIfNull(services);
			return services.AddSingleton<IDataSqlBuilder, TBuilder>();
		}
		/// <summary>
		/// Adds the Data unit of work implementation to the service collection for dependency injection.
		/// </summary>
		/// <remarks>This method registers the <c>DataUnitOfWork</c> as the implementation for the unit of
		/// work pattern. The services parameter must not be null.</remarks>
		/// <returns>The <see cref="IServiceCollection"/> instance with the XData unit of work service registered.</returns>
		public IServiceCollection AddXDataUnitOfWork()
		{
			ArgumentNullException.ThrowIfNull(services);
			return services.AddXDataUnitOfWork<DataUnitOfWork>();
		}

		/// <summary>
		/// Registers the specified unit of work implementation as a scoped service in the dependency injection
		/// container.
		/// </summary>
		/// <remarks>Use this method to configure the unit of work pattern for data access in
		/// applications. The registered unit of work will have a scoped lifetime, ensuring a single instance is used
		/// within each request scope.</remarks>
		/// <typeparam name="TUnitOfWork">The type of the unit of work to register. Must be a class that implements the IDataUnitOfWork interface and
		/// has a public constructor.</typeparam>
		/// <returns>The IServiceCollection instance to allow for method chaining.</returns>
		public IServiceCollection AddXDataUnitOfWork<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TUnitOfWork>()
			where TUnitOfWork : class, IDataUnitOfWork =>
			services.AddScoped<IDataUnitOfWork, TUnitOfWork>();

		/// <summary>
		/// Registers a keyed, scoped unit of work service of the specified type for dependency injection.
		/// </summary>
		/// <remarks>Use this method when multiple IDataUnitOfWork implementations are required and need
		/// to be resolved by a unique key. This enables scenarios where different units of work are selected at runtime
		/// based on the provided key.</remarks>
		/// <typeparam name="TUnitOfWork">The type of the unit of work to register. Must implement the IDataUnitOfWork interface and have a public
		/// constructor.</typeparam>
		/// <param name="key">The unique key that identifies the registered unit of work service. Cannot be null.</param>
		/// <returns>The IServiceCollection instance that can be used to further configure the service collection.</returns>
		public IServiceCollection AddXDataUnitOfWorkKeyed<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TUnitOfWork>(string key)
			where TUnitOfWork : class, IDataUnitOfWork
		{
			ArgumentNullException.ThrowIfNull(key);
			return services.AddKeyedScoped<IDataUnitOfWork, TUnitOfWork>(key);
		}

		/// <summary>
		/// Registers a scoped service for the specified unit of work interface and its implementation in the dependency
		/// injection container.
		/// </summary>
		/// <remarks>Use this method to configure dependency injection for unit of work patterns, ensuring
		/// that requests for the specified interface resolve to the provided implementation within the scope of a
		/// request.</remarks>
		/// <typeparam name="TInterface">The interface type that represents a unit of work and must implement IDataUnitOfWork.</typeparam>
		/// <typeparam name="TImplementation">The concrete type that implements the specified interface and must have a public constructor.</typeparam>
		/// <returns>The IServiceCollection instance that this method was called on, to support method chaining.</returns>
		public IServiceCollection AddXDataUnitOfWork<TInterface, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>()
			where TInterface : class, IDataUnitOfWork
			where TImplementation : class, TInterface =>
			services.AddScoped<TInterface, TImplementation>();

		/// <summary>
		/// Registers a keyed, scoped service for a specified data unit of work interface and its implementation in the
		/// service collection.
		/// </summary>
		/// <remarks>Use this method to register multiple implementations of the same data unit of work
		/// interface, each associated with a unique key. This enables resolving specific implementations by key at
		/// runtime, which is useful in scenarios where different data contexts or strategies are required.</remarks>
		/// <typeparam name="TInterface">The interface type that extends IDataUnitOfWork to be registered as a service.</typeparam>
		/// <typeparam name="TImplementation">The concrete implementation type that must derive from TInterface and provide a public constructor.</typeparam>
		/// <param name="key">The unique non-null key used to identify the service registration for keyed resolution.</param>
		/// <returns>The IServiceCollection instance that this method was called on, to support method chaining.</returns>
		public IServiceCollection AddXDataUnitOfWorkKeyed<TInterface, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(string key)
			where TInterface : class, IDataUnitOfWork
			where TImplementation : class, TInterface
		{
			ArgumentNullException.ThrowIfNull(key);
			return services.AddKeyedScoped<TInterface, TImplementation>(key);
		}

		/// <summary>
		/// Registers a data repository implementation of the specified interface with the given service lifetime in the
		/// dependency injection container.
		/// </summary>
		/// <remarks>Use this method to enable dependency injection of a custom data repository throughout
		/// the application. If the repository type is already registered, this method will not overwrite the existing
		/// registration.</remarks>
		/// <typeparam name="TRepository">The type of the data repository interface to register. Must implement IDataRepository.</typeparam>
		/// <typeparam name="TImplementation">The concrete implementation type that fulfills the TRepository interface. Must have a public constructor.</typeparam>
		/// <param name="lifetime">The lifetime with which to register the service. The default is Scoped.</param>
		/// <returns>The IServiceCollection instance with the repository registration added.</returns>
		public IServiceCollection AddXDataRepository<TRepository, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(
			ServiceLifetime lifetime = ServiceLifetime.Scoped)
			where TRepository : class, IDataRepository
			where TImplementation : class, TRepository
		{
			ArgumentNullException.ThrowIfNull(services);

			services.TryAdd(new ServiceDescriptor(typeof(TRepository), typeof(TImplementation), lifetime));

			return services;
		}

		/// <summary>
		/// Registers multiple data repository interfaces and their corresponding implementations with the specified
		/// service lifetime in the dependency injection container.
		/// </summary>
		/// <remarks>This method allows for registering multiple data repositories in a single call,
		/// ensuring that each interface and implementation pair is valid. It is useful for configuring repository
		/// dependencies in a modular and maintainable way.</remarks>
		/// <param name="lifetime">The lifetime with which to register each data repository service. Determines how long the service instance
		/// is retained by the container.</param>
		/// <param name="repositoryRegistrations">An array of tuples, each containing a data repository interface type and its corresponding implementation
		/// type to be registered. Each interface type must implement IDataRepository, and each implementation type must
		/// implement its associated interface.</param>
		/// <returns>The IServiceCollection instance with the specified data repository registrations added. This enables method
		/// chaining.</returns>
		/// <exception cref="ArgumentException">Thrown if any interface type does not implement IDataRepository, or if any implementation type does not
		/// implement its corresponding interface type.</exception>
		public IServiceCollection AddXDataRepositories(
			ServiceLifetime lifetime,
			params (Type InterfaceType, Type ImplementationType)[] repositoryRegistrations)
		{
			ArgumentNullException.ThrowIfNull(services);
			ArgumentNullException.ThrowIfNull(repositoryRegistrations);
			ArgumentOutOfRangeException.ThrowIfZero(repositoryRegistrations.Length, nameof(repositoryRegistrations));

			foreach (var (interfaceType, implementationType) in repositoryRegistrations)
			{
				if (!typeof(IDataRepository).IsAssignableFrom(interfaceType))
				{
					throw new ArgumentException($"Interface type {interfaceType.Name} must implement IDataRepository.", nameof(repositoryRegistrations));
				}

				if (!interfaceType.IsAssignableFrom(implementationType))
				{
					throw new ArgumentException($"Implementation type {implementationType.Name} must implement {interfaceType.Name}.", nameof(repositoryRegistrations));
				}

				services.TryAdd(new ServiceDescriptor(interfaceType, implementationType, lifetime));
			}

			return services;
		}

		/// <summary>
		/// Registers all data repository implementations found in the specified assemblies with the provided service
		/// lifetime.
		/// </summary>
		/// <remarks>This method scans each provided assembly for non-abstract, sealed classes that
		/// implement IDataRepository and registers them, along with their interfaces, in the service collection. Ensure
		/// that the assemblies contain the desired repository implementations. If no assemblies are provided, the
		/// calling assembly is scanned.</remarks>
		/// <param name="lifetime">The lifetime with which to register the discovered repository services. Determines how instances are managed
		/// by the dependency injection container.</param>
		/// <param name="assemblies">An array of assemblies to scan for classes implementing the IDataRepository interface. If no assemblies are
		/// specified, the calling assembly is used by default.</param>
		/// <returns>The IServiceCollection instance with the repository services registered, enabling method chaining.</returns>
		[RequiresUnreferencedCode("Requires unreferenced code.")]
		public IServiceCollection AddXDataRepositories(
			ServiceLifetime lifetime,
			params IEnumerable<Assembly> assemblies)
		{
			ArgumentNullException.ThrowIfNull(services);
			ArgumentNullException.ThrowIfNull(assemblies);

			Assembly[] assembliesArray = assemblies as Assembly[] ?? [.. assemblies];
			assembliesArray = assembliesArray is { Length: > 0 } ? assembliesArray : [Assembly.GetCallingAssembly()];

			foreach (var assembly in assembliesArray)
			{
				var types = assembly.GetTypes();
				var repositoryTypes = types
					.Where(t => t.IsClass && t.IsSealed && !t.IsAbstract && typeof(IDataRepository).IsAssignableFrom(t));

				foreach (var implementationType in repositoryTypes)
				{
					var interfaceTypes = implementationType.GetInterfaces()
						.Where(i => i != typeof(IDataRepository) && typeof(IDataRepository).IsAssignableFrom(i));
					foreach (var interfaceType in interfaceTypes)
					{
						services.TryAdd(new ServiceDescriptor(interfaceType, implementationType, lifetime));
					}
				}
			}

			return services;
		}
	}
}
