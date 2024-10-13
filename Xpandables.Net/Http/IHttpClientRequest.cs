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
/// Represents a request to be sent by an HTTP client.
/// </summary>
public interface IHttpClientRequest { }

/// <summary>
/// Represents a request to be sent by an HTTP client with a response 
/// of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IHttpClientRequest<out TResponse> : IHttpClientRequest { }

/// <summary>
/// Represents a request to be sent by an HTTP client with a stream response 
/// of type <see cref="IAsyncEnumerable{T}"/> of <typeparamref name="TResponse"/> 
/// type.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IHttpClientAsyncRequest<out TResponse> : IHttpClientRequest { }
