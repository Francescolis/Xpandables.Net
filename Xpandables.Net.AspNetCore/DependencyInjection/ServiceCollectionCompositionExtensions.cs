﻿/*******************************************************************************
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
using System.Reflection;

using Microsoft.AspNetCore.Builder;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides extension methods for using service exports to 
/// a <see cref="WebApplication"/>.
/// </summary>
public static class ServiceCollectionCompositionExtensions
{
    /// <summary>
    /// Uses the service exports with default options.
    /// </summary>
    /// <param name="application">The web application.</param>
    /// <returns>The web application with applied service exports.</returns>
    public static WebApplication UseXServiceExports(
        this WebApplication application) =>
        application.UseXServiceExports(_ => { });

    /// <summary>
    /// Uses the service exports with specified options.
    /// </summary>
    /// <param name="application">The web application.</param>
    /// <param name="configureOptions">The action to configure export options.</param>
    /// <returns>The web application with applied service exports.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static WebApplication UseXServiceExports(
        this WebApplication application,
        Action<ExportOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(application);
        ArgumentNullException.ThrowIfNull(configureOptions);

        ExportOptions options = new();
        configureOptions(options);

        ServiceExportExtensions.ApplyServiceExports<IUseServiceExport>(
            options, exports =>
        {
            foreach (IUseServiceExport export in exports)
            {
                export.UseServices(application);
            }
        });

        return application;
    }

    /// <summary>
    /// Uses the specified assemblies to apply services to the web application.
    /// </summary>
    /// <param name="application">The web application.</param>
    /// <param name="assemblies">The assemblies to scan for services.</param>
    /// <returns>The web application with applied services.</returns>
    public static WebApplication UseXServices(
        this WebApplication application,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(application);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

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
                    && t == typeof(IUseService)))];

        foreach (Type type in types)
        {
            if (Activator.CreateInstance(type) is IUseService useService)
            {
                useService.UseServices(application);
            }
        }

        return application;
    }

}
