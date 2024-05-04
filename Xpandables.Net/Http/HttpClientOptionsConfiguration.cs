
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
using Microsoft.Extensions.Options;

namespace Xpandables.Net.Http;

/// <summary>
/// <see cref="HttpClientOptions"/>> configuration to sets the 
/// <see cref="IServiceProvider"/> to be used.
/// </summary>
/// <remarks>
/// Initializes a new instance of <see cref="HttpClientOptionsConfiguration"/>.
/// </remarks>
public sealed class HttpClientOptionsConfiguration(
    IServiceProvider serviceProvider) :
    IConfigureOptions<HttpClientOptions>
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    ///<inheritdoc/>
    public void Configure(HttpClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _ = _serviceProvider;
        //options.SetServiceProvider(_serviceProvider);
    }
}
