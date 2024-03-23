
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
using System.Net.Http.Headers;

namespace Xpandables.Net.Http;

/// <summary>
/// Represents a method signature used to apply 
/// <see cref="AuthenticationHeaderValue"/> to the request,
/// or returns the value to be used for the 
/// <see cref="AuthenticationHeaderValue"/> if not null.
/// </summary>
/// <param name="request">The target request to act on.</param>
/// <returns>A string that represents the value for
/// the <see cref="AuthenticationHeaderValue"/> if not null.</returns>
public delegate string? HttpClientAuthenticationHeaderValueProvider(
    HttpRequestMessage request);