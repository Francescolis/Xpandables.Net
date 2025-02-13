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
namespace Xpandables.Net.Http.Interfaces;

/// <summary>
/// Represents a request to be sent by <see cref="IHttpClientSender"/>.
/// </summary>
#pragma warning disable CA1040 // Avoid empty interfaces
public interface IHttpClientRequest { }
#pragma warning restore CA1040 // Avoid empty interfaces

/// <summary>
/// Represents a request to be sent by <see cref="IHttpClientSender"/> with a response 
/// of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
#pragma warning disable CA1040 // Avoid empty interfaces
public interface IHttpClientRequest<out TResponse> : IHttpClientRequest { }
#pragma warning restore CA1040 // Avoid empty interfaces

/// <summary>
/// Represents a request to be sent by <see cref="IHttpClientSender"/> with a stream response 
/// of type <see cref="IAsyncEnumerable{T}"/> of <typeparamref name="TResponse"/> 
/// type.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
#pragma warning disable CA1040 // Avoid empty interfaces
public interface IHttpClientAsyncRequest<out TResponse> : IHttpClientRequest { }
#pragma warning restore CA1040 // Avoid empty interfaces
