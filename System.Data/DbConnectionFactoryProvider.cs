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
using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace System.Data;

/// <summary>
/// Provides a factory for creating database connection instances based on configuration settings.
/// </summary>
/// <remarks>This class manages a collection of database connection factories, allowing retrieval based on
/// connection string names and provider invariant names. It automatically clears the factory cache when the
/// configuration changes, ensuring that the latest settings are used.</remarks>
public sealed class DbConnectionFactoryProvider : IDbConnectionFactoryProvider, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ConcurrentDictionary<string, IDbConnectionFactory> _factories;
    private readonly IDisposable _reloadSubscription;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbConnectionFactoryProvider"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    public DbConnectionFactoryProvider(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _factories = new ConcurrentDictionary<string, IDbConnectionFactory>(StringComparer.OrdinalIgnoreCase);

        _reloadSubscription = ChangeToken.OnChange(
            () => _configuration.GetReloadToken(),
            () => _factories.Clear());
    }

    /// <inheritdoc />
    public IDbConnectionFactory GetFactory(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (TryGetFactory(name, out var factory))
        {
            return factory;
        }

        throw new InvalidOperationException($"No connection string named '{name}' was found.");
    }

    /// <inheritdoc />
    public IDbConnectionFactory GetFactory(string name, string providerInvariantName)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(providerInvariantName);

        if (!TryGetFactory(name, providerInvariantName, out var factory))
        {
            throw new InvalidOperationException($"No connection string named '{name}' was found.");
        }

        return factory;
    }

    /// <inheritdoc />
    public bool TryGetFactory(string name, [NotNullWhen(true)] out IDbConnectionFactory? factory)
    {
        ArgumentNullException.ThrowIfNull(name);

        var connectionString = _configuration.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            factory = null;
            return false;
        }

        var resolvedProvider = ResolveProviderInvariantName(connectionString, null);
        var cacheKey = $"{name}|{resolvedProvider}";

        factory = _factories.GetOrAdd(cacheKey,
            _ => new DbConnectionFactory(resolvedProvider, connectionString));

        return true;
    }

    /// <inheritdoc />
    public bool TryGetFactory(string name, string providerInvariantName, [NotNullWhen(true)] out IDbConnectionFactory? factory)
    {
        ArgumentNullException.ThrowIfNull(name);

        var connectionString = _configuration.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            factory = null;
            return false;
        }

        var resolvedProvider = ResolveProviderInvariantName(connectionString, providerInvariantName);
        var cacheKey = $"{name}|{resolvedProvider}";

        factory = _factories.GetOrAdd(cacheKey,
            _ => new DbConnectionFactory(resolvedProvider, connectionString));

        return true;
    }

    /// <inheritdoc />
    public void Dispose() => _reloadSubscription.Dispose();

    private static string ResolveProviderInvariantName(string connectionString, string? providerInvariantName)
    {
        if (!string.IsNullOrWhiteSpace(providerInvariantName))
        {
            return providerInvariantName;
        }

        try
        {
            var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };
            if (builder.TryGetValue("Provider", out var provider))
            {
                var value = Convert.ToString(provider, CultureInfo.InvariantCulture);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }
        catch (ArgumentException)
        {
            // Keep default when parsing fails.
        }

        return DbProviders.MsSqlServer.InvariantName;
    }
}