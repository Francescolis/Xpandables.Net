﻿
/************************************************************************************************************
 * Copyright (C) 2020 Francis-Black EWANE
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
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Xpandables.Net.Http.RequestLocations
{
    /// <summary>
    /// Default implementation for <see cref="IHttpIPAddressLocationAccessor"/>.
    /// </summary>
    public class HttpIPAddressLocationAccessor : IHttpIPAddressLocationAccessor
    {
        private readonly IHttpRestClientHandler _httpRestClientHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpIPAddressLocationAccessor"/> class 
        /// that uses the http://api.ipstack.com to retrieve the user location.
        /// </summary>
        /// <param name="httpRestClientHandler">The target handler.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="httpRestClientHandler"/> is null.</exception>
        public HttpIPAddressLocationAccessor(IHttpRestClientHandler httpRestClientHandler)
        {
            _httpRestClientHandler = httpRestClientHandler ?? throw new ArgumentNullException(nameof(httpRestClientHandler));
        }

        ///<inheritdoc/>
        public virtual async Task<HttpRestClientResponse<IPAddressLocation>> ReadLocationAsync(
            IPAddressLocationRequest request,
            JsonSerializerOptions? serializerOptions = default,
            CancellationToken cancellationToken = default)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));
            return await _httpRestClientHandler.SendAsync(
                request,
                serializerOptions,
                cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
