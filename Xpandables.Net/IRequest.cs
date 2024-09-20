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
namespace Xpandables.Net;

/// <summary>
/// This interface is used as a marker for request.
/// Class implementation is used with the <see cref="IRequestHandler{TRequest}"/> 
/// where "TRequest" is a class that implements <see cref="IRequest"/>.
/// This can also be enhanced with some useful decorators.
/// </summary>
public interface IRequest
{
}

/// <summary>
/// This interface is used as a marker for requests when using the synchronous 
/// request pattern that contains a specific-type result.
/// Class implementation is used with the 
/// <see cref="IRequestHandler{TRequest, TResult}"/> where
/// "TRequest" is a class that implements the 
/// <see cref="IRequest{TResponse}"/> interface. 
/// This can also be enhanced with some useful decorators.
/// </summary>
/// <typeparam name="TResponse">Type of the response of the request.</typeparam>
public interface IRequest<out TResponse>
{
}

/// <summary>
/// This interface is used as a marker for requests when using the asynchronous 
/// request pattern that contains a <see cref="IAsyncEnumerable{TResponse}"/> 
/// of specific-type response.
/// Class implementation is used with the 
/// <see cref="IAsyncRequestHandler{TRequest, TResponse}"/> where
/// "TRequest" is a class that implements the 
/// <see cref="IAsyncRequest{TResponse}"/> interface. 
/// This can also be enhanced with some useful decorators.
/// </summary>
/// <typeparam name="TResponse">Type of the response of the request.</typeparam>
public interface IAsyncRequest<out TResponse>
{
}