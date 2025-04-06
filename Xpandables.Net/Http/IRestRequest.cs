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
/// Defines a contract for RESTful requests. 
/// It serves as a blueprint for implementing REST request functionalities.
/// </summary>
public interface IRestRequest
{
    /// <summary>
    /// Returns the name of the type of the current instance as a string.
    /// This is typically the class name.
    /// </summary>
    public string Name => GetType().Name;
}

/// <summary>
/// Defines a contract for a REST request that produces a specific response type.
/// </summary>
/// <typeparam name="TResponse">Represents the type of response expected from the REST request.</typeparam>
public interface IRestRequest<out TResponse> : IRestRequest
{
}

/// <summary>
/// Defines a request interface for REST streams that specifies a response type. 
/// It provides the name of the response type as a string.
/// </summary>
/// <typeparam name="TResponse">Represents the type of data expected in the response from the REST request.</typeparam>
public interface IRestStreamRequest<out TResponse> : IRestRequest
{
    /// <summary>
    /// Returns the type of the response as a string.
    /// This is typically the class name of the response type.
    /// </summary>
    public string ResponseType => typeof(IAsyncEnumerable<TResponse>).Name;
}