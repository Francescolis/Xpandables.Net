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
using System.ComponentModel;

namespace Xpandables.Net.Executions.Tasks;

/// <summary>
/// Provides a way to define a request.
/// Class implementation is used with the <see cref="IRequestHandler{TRequest}"/> 
/// where "TRequest" is a record that implements <see cref="IRequest"/>.
/// </summary>
#pragma warning disable CA1040 // Avoid empty interfaces
public interface IRequest
#pragma warning restore CA1040 // Avoid empty interfaces
{
}

/// <summary>
/// Provides a way to define a request that returns a result.
/// Class implementation is used with the <see cref="IRequestHandler{TRequest}"/> 
/// where "TRequest" is a class that implements <see cref="IRequest{TResult}"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
#pragma warning disable CA1040 // Avoid empty interfaces
public interface IRequest<out TResult> : IRequest
#pragma warning restore CA1040 // Avoid empty interfaces
{
}

/// <summary>
/// Provides a way to define a stream request that returns a result asynchronously.
/// <see cref="IAsyncEnumerable{TResult}"/> of specific-type response.
/// Class implementation is used with the 
/// <see cref="IRequestHandler{TRequest}"/> where
/// "TRequest" is a class that implements the 
/// <see cref="IStreamRequest{TResult}"/> interface. 
/// </summary>
/// <typeparam name="TResult">Type of the result of the request.</typeparam>
#pragma warning disable CA1040 // Avoid empty interfaces
public interface IStreamRequest<out TResult> : IRequest
#pragma warning restore CA1040 // Avoid empty interfaces
{
}

/// <summary>
/// Provides a way to define a request with a dependency type that gets resolved by the decider.
/// </summary>
public interface IDeciderRequest : IRequest
{
    /// <summary>
    /// The dependency type.
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// The key identifier used to identify an instance of the dependency type.
    /// </summary>
    object KeyId { get; }

    /// <summary>
    /// The value of the dependency.
    /// </summary>
    /// <remarks>For internal use only.</remarks>
    internal object Dependency { get; set; }
}

/// <summary>
/// Provides a way to define a request with a dependency type that gets resolved by the decider.
/// </summary>
/// <typeparam name="TDependency">The type of the dependency.</typeparam>
public interface IDeciderRequest<TDependency> : IDeciderRequest
    where TDependency : class
{
    /// <summary>
    /// The type of the dependency.
    /// </summary>
    public new Type Type => typeof(TDependency);
}

/// <summary>
/// Represents a request that contains a dependency type that gets resolved by the decider.
/// </summary>
/// <typeparam name="TDependency">The type of the dependency.</typeparam>
public abstract record DeciderRequest<TDependency> : IDeciderRequest<TDependency>
    where TDependency : class
{
    /// <inheritdoc/>
    public Type Type => typeof(TDependency);

    /// <inheritdoc/>
    public required object KeyId { get; init; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    object IDeciderRequest.Dependency { get; set; } = default!;
}