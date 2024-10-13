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
namespace Xpandables.Net.Http;

/// <summary>
/// Provides with methods to handle <see cref="IHttpClientRequest"/> 
/// or <see cref="IHttpClientRequest{TResponse}"/> requests using a typed 
/// client HTTP Client.
/// The request should implement one of the following interfaces :
/// <see cref="IHttpRequestString"/>, <see cref="IHttpRequestStream"/>, 
/// <see cref="IHttpRequestByteArray"/>, 
/// <see cref="IHttpRequestFormUrlEncoded"/>,
/// <see cref="IHttpRequestMultipart"/>, <see cref="IHttpRequestQueryString"/>, 
/// <see cref="IHttpRequestCookie"/>,
/// <see cref="IHttpRequestHeader"/>, <see cref="IHttpRequestPatch"/>
/// or <see cref="IHttpRequestPathString"/>, and must be decorated 
/// with <see cref="HttpClientRequestOptionsAttribute"/> 
/// or implement <see cref="IHttpClientRequestOptionsBuilder"/>.
/// </summary>
public interface IHttpClientDispatcher
{
}
