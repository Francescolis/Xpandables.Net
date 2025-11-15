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
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xpandables.Net.Exports;


namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides extension methods for registering services with an <see cref="IServiceCollection"/> using
/// configuration and assembly scanning.
/// </summary>
/// <remarks>These extension methods enable modular service registration by discovering and adding services that
/// implement specific export interfaces. Some methods perform assembly scanning and may be affected by trimming when
/// publishing applications; ensure that all required types are preserved if trimming is enabled.</remarks>
public static class IServiceExportExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds the configured <see cref="System.Text.Json.JsonSerializerOptions"/> to the service collection as a
        /// singleton service.
        /// </summary>
        /// <remarks>This method retrieves the <see cref="System.Text.Json.JsonSerializerOptions"/> from
        /// the application's <see cref="Microsoft.Extensions.Options.IOptions{JsonOptions}"/> and makes it available
        /// for dependency injection. Use this method to ensure consistent JSON serialization settings throughout the
        /// application.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> with the <see cref="System.Text.Json.JsonSerializerOptions"/>
        /// registered as a singleton.</returns>
        public IServiceCollection AddXJsonSerializerOptions()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;
                return options;
            });
        }

        /// <summary>
        /// Registers support for resolving <see cref="Lazy{T}"/> dependencies using the <see cref="LazyResolved{T}"/> implementation in the service
        /// collection.
        /// </summary>
        /// <remarks>Use this method to allow constructor injection of <see cref="Lazy{T}"/> in services registered with
        /// the dependency injection container. Each <see cref="Lazy{T}"/> will resolve its value from the container when first
        /// accessed.</remarks>
        /// <returns>The IServiceCollection instance with <see cref="Lazy{T}"/> registration added. This enables <see cref="Lazy{T}"/> to be injected and
        /// resolved by the dependency injection container.</returns>
        public IServiceCollection AddXLazyResolved()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddTransient(typeof(Lazy<>), typeof(LazyResolved<>));
        }

        /// <summary>
        /// Adds services exports to the service collection using the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration settings used to configure the X service exports. Cannot be null.</param>
        /// <returns>The service collection with X service exports registered. This enables further chaining of service
        /// registration calls.</returns>
        [RequiresAssemblyFiles]
        public IServiceCollection AddXServiceExports(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            return services.AddXServiceExports(configuration, _ => { });
        }

        /// <summary>
        /// Adds services to the service collection using the specified configuration and export options.
        /// </summary>
        /// <param name="configuration">The configuration source used to initialize the exported services. Cannot be null.</param>
        /// <param name="configureOptions">A delegate that configures the export options. Cannot be null.</param>
        /// <returns>The current <see cref="IServiceCollection"/> instance with the X service exports registered.</returns>
        [RequiresAssemblyFiles]
        public IServiceCollection AddXServiceExports(IConfiguration configuration, Action<ExportOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(configureOptions);

            ExportOptions options = new();
            configureOptions(options);

            ApplyServiceExports<IAddServiceExport>(
                options, exports =>
                {
                    foreach (IAddServiceExport export in exports)
                    {
                        export.AddServices(services, configuration);
                    }
                });

            return services;
        }

        /// <summary>
        /// Adds all services implementing the IAddService interface from the specified assemblies to the service
        /// collection using the provided configuration.
        /// </summary>
        /// <remarks>This method scans the provided assemblies for non-abstract, non-generic types that
        /// implement IAddService and invokes their AddServices method to register services. The method may be trimmed
        /// when publishing with trimming enabled; ensure that all required types are preserved.</remarks>
        /// <param name="configuration">The configuration to be used when adding services. Cannot be null.</param>
        /// <param name="assemblies">The assemblies to scan for types implementing IAddService. If no assemblies are specified, the calling
        /// assembly is used by default.</param>
        /// <returns>The IServiceCollection instance with the discovered service exports added.</returns>
        [RequiresUnreferencedCode("This method may be trimmed.")]
        public IServiceCollection AddXServiceExports(IConfiguration configuration, params Assembly[] assemblies)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            assemblies = assemblies is { Length: > 0 } ? assemblies : [Assembly.GetCallingAssembly()];

            List<Type> types = [.. assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                type is
                {
                    IsAbstract: false,
                    IsInterface: false,
                    IsGenericType: false
                }
                && Array.Exists(type.GetInterfaces(),
                    t => !t.IsGenericType
                    && t == typeof(IAddService)))];

            foreach (Type type in types)
            {
                if (Activator.CreateInstance(type) is IAddService addService)
                {
                    addService.AddServices(services, configuration);
                }
            }

            return services;
        }
    }

    [RequiresAssemblyFiles]
    internal static void ApplyServiceExports<TServiceExport>(
        ExportOptions options,
        Action<IEnumerable<TServiceExport>> onServiceExport)
        where TServiceExport : class
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(onServiceExport);

        try
        {
            using ComposablePartCatalog directoryCatalog = options
                 .SearchSubDirectories
                 ? new RecursiveDirectoryCatalog(
                     options.Path, options.SearchPattern)
                 : new DirectoryCatalog(
                     options.Path, options.SearchPattern);

            ImportDefinition importDefinition =
                     ApplyImportDefinition<TServiceExport>();

            using AggregateCatalog aggregateCatalog = new();
            aggregateCatalog.Catalogs.Add(directoryCatalog);

            using CompositionContainer compositionContainer =
                new(aggregateCatalog);

            IEnumerable<TServiceExport> exportServices = compositionContainer
                .GetExports(importDefinition)
                .Select(def => def.Value)
                .OfType<TServiceExport>();

            onServiceExport(exportServices);
        }
        catch (Exception exception)
              when (exception is NotSupportedException
                            or DirectoryNotFoundException
                            or UnauthorizedAccessException
                            or ArgumentException
                            or PathTooLongException
                            or ReflectionTypeLoadException)
        {
            throw new InvalidOperationException(
                "Adding or using exports failed." +
                " See inner exception.", exception);
        }
    }

    internal static ImportDefinition ApplyImportDefinition<TServiceExport>()
     where TServiceExport : class
     => new(
             _ => true,
             typeof(TServiceExport).FullName,
             ImportCardinality.ZeroOrMore,
             false,
             false);
}