
/*******************************************************************************
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
********************************************************************************/
using Microsoft.AspNetCore.Builder;

using Xpandables.Net.Compositions;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides method to register services.
/// </summary>
public static class ServiceCollectionExportExtensions
{
    /// <summary>
    /// Adds and configures application services using the
    /// <see cref="IUseServiceExport"/> 
    /// implementations found in the current application path.
    /// This method is used with MEF : Managed Extensibility Framework.
    /// </summary>
    /// <param name="application">The collection of services.</param>
    /// <returns>The <see cref="WebApplication"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="application"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    public static WebApplication UseXServiceExport(this WebApplication application)
    {
        ArgumentNullException.ThrowIfNull(application);

        return application.UseXServiceExport(_ => { });
    }

    /// <summary>
    /// Adds and configures application services using the
    /// <see cref="IUseServiceExport"/> implementations found in the path.
    /// This method is used with MEF : Managed Extensibility Framework.
    /// </summary>
    /// <param name="application">The application builder instance.</param>
    /// <param name="configureOptions">A delegate to 
    /// configure the <see cref="ExportServiceOptions"/>.</param>
    /// <returns>The <see cref="WebApplication"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="application"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="configureOptions"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    public static WebApplication UseXServiceExport(
        this WebApplication application,
        Action<ExportServiceOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(application);
        ArgumentNullException.ThrowIfNull(configureOptions);

        ExportServiceOptions definedOptions = new();
        configureOptions.Invoke(definedOptions);

        ServiceExportExtensions
            .ApplyServiceExports<IUseServiceExport>(definedOptions,
            exportServices =>
            {
                foreach (IUseServiceExport export in exportServices)
                    export.UseServices(application);
            });

        return application;
    }
}
