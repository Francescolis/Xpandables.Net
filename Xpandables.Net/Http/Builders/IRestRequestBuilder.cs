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
using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Pipelines;

namespace Xpandables.Net.Http.Builders;

/// <summary>
/// Defines a contract for building REST requests.
/// </summary>
/// <typeparam name="TRestRequest">The type of the interface
/// implemented by the request source : <see cref="IRestBasicAuthentication"/>,
/// <see cref="IRestByteArray"/>, <see cref="IRestCookie"/>,
/// <see cref="IRestFormUrlEncoded"/>, <see cref="IRestHeader"/>,
/// <see cref="IRestMultipart"/>, <see cref="IRestPatch"/>,
/// <see cref="IRestPathString"/>, <see cref="IRestQueryString"/>,
/// <see cref="IRestStream"/> and <see cref="IRestString"/>.</typeparam>
public interface IRestRequestBuilder<TRestRequest> : IPipelineDecorator<RestRequestContext<TRestRequest>, ExecutionResult>
    where TRestRequest : class, IRestRequest
{
}
