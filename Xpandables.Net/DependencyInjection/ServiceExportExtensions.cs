/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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
using System.Reflection;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides extension methods for applying service exports.
/// </summary>
/// <remarks>For internal use.</remarks>
public static class ServiceExportExtensions
{
    /// <summary>
    /// This method is for internal use only. it may change or be removed in 
    /// future releases.
    /// <para>Applies service exports based on the specified options and executes 
    /// the provided action on the exported services.</para>
    /// </summary>
    /// <typeparam name="TServiceExport">The type of the service export.</typeparam>
    /// <param name="options">The options for exporting services.</param>
    /// <param name="onServiceExport">The action to execute on the exported services.</param>
    /// <exception cref="InvalidOperationException">Thrown when adding or 
    /// using exports fails.</exception>
    public static void ApplyServiceExports<TServiceExport>(
        ExportOptions options,
        Action<IEnumerable<TServiceExport>> onServiceExport)
        where TServiceExport : class
    {
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