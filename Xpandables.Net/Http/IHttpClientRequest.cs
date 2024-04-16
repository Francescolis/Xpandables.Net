
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
namespace Xpandables.Net.Http;

/// <summary>
/// This interface is used as a marker for request without result.
/// </summary>
/// <remarks>May be used in combination with <see cref="IHttpRequestByteArray"/>, 
/// <see cref="IHttpRequestCookie"/>, <see cref="IHttpRequestFormUrlEncoded"/>, 
/// <see cref="IHttpRequestHeader"/>, <see cref="IHttpRequestMultipart"/>, 
/// <see cref="IHttpRequestPatch"/>, <see cref="IHttpRequestPathString"/>, 
/// <see cref="IHttpRequestQueryString"/>, <see cref="IHttpRequestStream"/> 
/// or <see cref="IHttpRequestString"/>.</remarks>
#pragma warning disable CA1040 // Avoid empty interfaces
public interface IHttpClientRequest { }
#pragma warning restore CA1040 // Avoid empty interfaces

/// <summary>
/// This interface is used as a marker for request that contains a specific-type result.
/// </summary>
/// <typeparam name="TResponse">Type of the result of the query.</typeparam>
/// <remarks>May be used in combination with <see cref="IHttpRequestByteArray"/>, 
/// <see cref="IHttpRequestCookie"/>, <see cref="IHttpRequestFormUrlEncoded"/>, 
/// <see cref="IHttpRequestHeader"/>, <see cref="IHttpRequestMultipart"/>, 
/// <see cref="IHttpRequestPatch"/>, <see cref="IHttpRequestPathString"/>, 
/// <see cref="IHttpRequestQueryString"/>, <see cref="IHttpRequestStream"/> 
/// or <see cref="IHttpRequestString"/>.</remarks>
#pragma warning disable CA1040 // Avoid empty interfaces
public interface IHttpClientRequest<out TResponse> { }
#pragma warning restore CA1040 // Avoid empty interfaces

/// <summary>
/// This interface is used as a marker for request when using the asynchronous query pattern
/// that contains a <see cref="IAsyncEnumerable{TResult}"/> of specific-type result.
/// </summary>
/// <typeparam name="TResponse">Type of the result of the request.</typeparam>
/// <remarks>May be used in combination with <see cref="IHttpRequestByteArray"/>, 
/// <see cref="IHttpRequestCookie"/>, <see cref="IHttpRequestFormUrlEncoded"/>, 
/// <see cref="IHttpRequestHeader"/>, <see cref="IHttpRequestMultipart"/>, 
/// <see cref="IHttpRequestPatch"/>, <see cref="IHttpRequestPathString"/>, 
/// <see cref="IHttpRequestQueryString"/>, <see cref="IHttpRequestStream"/> 
/// or <see cref="IHttpRequestString"/>.</remarks>
#pragma warning disable CA1040 // Avoid empty interfaces
public interface IHttpClientAsyncRequest<out TResponse> { }
#pragma warning restore CA1040 // Avoid empty interfaces