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
using System.Text.Json;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;


#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering services with an <see cref="IServiceCollection"/>.
/// </summary>
public static class JsonSerializerOptionsExtensions
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
				JsonSerializerOptions options = provider.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;
                return options;
            });
        }
    }
}