
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
using Microsoft.Extensions.Options;

namespace Xpandables.Net.Http;
/// <summary>
/// Configures the <see cref="RestOptions"/> for the application.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RestOptionsConfiguration"/> class.
/// </remarks>
/// <param name="provider">The service provider.</param>
public sealed class RestOptionsConfiguration(IServiceProvider provider) : IConfigureOptions<RestOptions>
{
    private readonly IServiceProvider _provider = provider;

    /// <inheritdoc/>
    public void Configure(RestOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Resolver = _provider.GetService;
    }
}
