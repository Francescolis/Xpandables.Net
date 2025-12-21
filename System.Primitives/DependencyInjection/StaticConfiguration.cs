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
using Microsoft.Extensions.Configuration;

namespace System.DependencyInjection;

/// <summary>
/// Provides access to application-wide configuration settings.
/// </summary>
/// <remarks>The configuration is loaded on first access and cached for subsequent calls. Use this class to
/// retrieve settings such as connection strings and custom configuration values throughout the application.</remarks>
public static class StaticConfiguration
{
    private static IConfiguration? _configuration;

    /// <summary>
    /// Gets the application's configuration settings.
    /// </summary>
    public static IConfiguration Configuration => _configuration ??= ConfigurationBuilder();

    static IConfiguration ConfigurationBuilder()
    {
        string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
