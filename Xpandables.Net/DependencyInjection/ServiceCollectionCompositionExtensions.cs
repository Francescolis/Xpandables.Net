
/************************************************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
************************************************************************************************************/
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Compositions;

[assembly: InternalsVisibleTo("Xpandables.Net.Composition.AspNetCore, PublicKey=0024000004800000940000000602000000240000525341310004000001000100410b9f6b317bb83c59c2727a39ad3e0c3aff55cbfc6f1328e2a925ab2e85d44b1815b23cea3f22924ea4226a6b3318eb90d1f28234e0116be8b70c29a41849a93e1baa680deae7f56e8d75d352d6f3b8457746223adf8cc2085a2d1d8c3f7be439bc53f1a032cc696f75afa378e0e054f3eb325fb9a7898a31c612c21e9c3cb8")]

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides with methods to register composition services.
/// </summary>
public static class ServiceCollectionCompositionExtensions
{
    /// <summary>
    /// Registers and configures registration of services using the
    /// <see cref="IAddServiceExport"/> implementations found in the current application path.
    /// This method uses MEF : Managed Extensibility Framework.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
    public static IServiceCollection AddXServiceExport(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddXServiceExport(configuration, _ => { });
        return services;
    }

    /// <summary>
    /// Registers and configures registration of services using the
    /// <see cref="IAddServiceExport"/> implementations found in the path.
    /// This method uses MEF : Managed Extensibility Framework.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="configureOptions">A delegate to configure the <see cref="ExportServiceOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="configureOptions"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
    public static IServiceCollection AddXServiceExport(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<ExportServiceOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(configureOptions);

        var definedOptions = new ExportServiceOptions();
        configureOptions.Invoke(definedOptions);
        ServiceExportExtensions.ApplyServiceExports<IAddServiceExport>(
            definedOptions, exportServices =>
        {
            foreach (var export in exportServices)
                export.AddServices(services, configuration);
        });

        return services;
    }
}

internal static class ServiceExportExtensions
{
    internal static void ApplyServiceExports<TServiceExport>(
        ExportServiceOptions options,
        Action<IEnumerable<TServiceExport>> onServiceExport)
        where TServiceExport : class
    {
        try
        {
            using var directoryCatalog = options.SearchSubDirectories
                ? new RecursiveDirectoryCatalog(options.Path, options.SearchPattern)
                : (ComposablePartCatalog)new DirectoryCatalog(options.Path, options.SearchPattern);

            var importDefinition = ApplyImportDefinition<TServiceExport>();

            using var aggregateCatalog = new AggregateCatalog();
            aggregateCatalog.Catalogs.Add(directoryCatalog);

            using var compositionContainer = new CompositionContainer(aggregateCatalog);
            var exportServices = compositionContainer
                .GetExports(importDefinition)
                .Select(def => def.Value)
                .OfType<TServiceExport>();

            onServiceExport(exportServices);
        }
        catch (Exception exception) when (exception is NotSupportedException
                                        || exception is DirectoryNotFoundException
                                        || exception is UnauthorizedAccessException
                                        || exception is ArgumentException
                                        || exception is PathTooLongException
                                        || exception is ReflectionTypeLoadException)
        {
            throw new InvalidOperationException("Adding or using exports failed. See inner exception.", exception);
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
