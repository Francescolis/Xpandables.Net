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
using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Interface for adding services to the service collection.
/// </summary>
public interface IAddService
{
    /// <summary>
    /// Adds services to the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    public void AddServices(IServiceCollection services) { }

    /// <summary>
    /// Adds services to the specified service collection using the provided configuration.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration to use for adding services.</param>
    public void AddServices(IServiceCollection services, IConfiguration configuration) =>
        AddServices(services);
}
