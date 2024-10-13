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
using System.Net;

namespace Xpandables.Net.Http;
public interface IHttpClientResponseBuilder
{
    /// <summary>
    /// Gets the response content type result being built by the current 
    /// builder instance.
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// When overridden in a derived class, determines whether the builder
    /// instance can build the response for the specified status code.
    /// </summary>
    /// <param name="targetType">The type of the response.</param>
    /// <returns><see langword="true"/> if the instance can build the
    /// specified request; otherwise, <see langword="false"/>.</returns>
    /// <param name="targetStatusCode">The status code of the response.</param>
    bool CanBuild(Type targetType, HttpStatusCode targetStatusCode);
}
