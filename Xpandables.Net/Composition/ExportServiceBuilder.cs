
/************************************************************************************************************
 * Copyright (C) 2022 Francis-Black EWANE
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
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Reflection;

using Xpandables.Net.Extensions;
using Xpandables.Net.I18n;

namespace Xpandables.Net.Composition;

/// <summary>
/// Provides with methods for building application object 
/// using the Managed Extensibility Framework (MEF).
/// </summary>
public sealed class ExportServiceBuilder
{
    /// <summary>
    /// The composer used to build object.
    /// </summary>
    public CompositionContainer Composer { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="ExportServiceBuilder"/> using 
    /// the default export options to compose parts of the target object.
    /// </summary>
    /// <param name="target">The object to act on.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Building the object failed. 
    /// See inner exception.</exception>
    public ExportServiceBuilder(object target) : this(target, new ExportServiceOptions()) { }

    /// <summary>
    /// Initializes a new instance of <see cref="ExportServiceBuilder"/> using 
    /// the specified export options to compose parts of the target object.
    /// </summary>
    /// <param name="target">The object to act on.</param>
    /// <param name="options">The export options to be used.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="options"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Building the object failed. See inner exception.</exception>
    public ExportServiceBuilder(object target, ExportServiceOptions options)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(options);

        try
        {
            var directoryCatalog = options.SearchSubDirectories
                ? new RecursiveDirectoryCatalog(options.Path, options.SearchPattern)
                : (ComposablePartCatalog)new DirectoryCatalog(options.Path, options.SearchPattern);

            using var aggregateCatalog = new AggregateCatalog(directoryCatalog);
            Composer = new CompositionContainer(aggregateCatalog, true);
            Composer.ComposeParts(target);
        }
        catch (Exception exception) when (exception is NotSupportedException
                                        || exception is DirectoryNotFoundException
                                        || exception is UnauthorizedAccessException
                                        || exception is ArgumentException
                                        || exception is PathTooLongException
                                        || exception is ReflectionTypeLoadException)
        {
            throw new InvalidOperationException(
                I18nXpandables.ActionSpecifiedFailedSeeException.StringFormat(nameof(ExportServiceBuilder)),
                exception);
        }
    }
}
