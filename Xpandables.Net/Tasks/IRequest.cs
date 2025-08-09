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

using Xpandables.Net.Optionals;

namespace Xpandables.Net.Tasks;

/// <summary>
/// Provides a way to define a request.
/// Class implementation is used with the <see cref="IRequestHandler{TRequest}" />
/// where "TRequest" is a record that implements <see cref="IRequest" />.
/// </summary>
public interface IRequest
{
    /// <summary>
    /// Gets the date and time when the request was created.
    /// </summary>
    public DateTime CreatedAt => DateTime.Now;
}

/// <summary>
/// Provides a way to define a request that returns a result.
/// Class implementation is used with the <see cref="IRequestHandler{TRequest}" />
/// where "TRequest" is a class that implements <see cref="IRequest{TResult}" />.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IRequest<out TResult> : IRequest;

/// <summary>
/// Provides a way to define a stream request that returns a result asynchronously.
/// <see cref="IAsyncEnumerable{TResult}" /> of specific-type response.
/// Class implementation is used with the
/// <see cref="IRequestHandler{TRequest}" /> where
/// "TRequest" is a class that implements the
/// <see cref="IStreamRequest{TResult}" /> interface.
/// </summary>
/// <typeparam name="TResult">Type of the result of the request.</typeparam>
public interface IStreamRequest<out TResult> : IRequest;

/// <summary>
/// Provides a way to define a request with a dependency type that gets resolved by a provider.
/// </summary>
public interface IDependencyRequest : IRequest
{
    /// <summary>
    /// The dependency type.
    /// </summary>
    Type DependencyType { get; }

    /// <summary>
    /// The key identifier used to identify an instance of the dependency type.
    /// </summary>
    object DependencyKeyId { get; }

    /// <summary>
    /// Gets or sets the instance of the dependency used by the component.
    /// </summary>
    Optional<object> DependencyInstance { get; set; }
}

/// <summary>
/// Provides a way to define a request with a dependency type that gets resolved by a provider.
/// </summary>
/// <typeparam name="TDependency">The type of the dependency.</typeparam>
public interface IDependencyRequest<TDependency> : IDependencyRequest
    where TDependency : class
{
    /// <summary>
    /// The type of the dependency.
    /// </summary>
    public new Type DependencyType => typeof(TDependency);

    [EditorBrowsable(EditorBrowsableState.Never)]
    Type IDependencyRequest.DependencyType => DependencyType;

    /// <summary>
    /// Gets or sets the instance of the dependency.
    /// </summary>
    new Optional<TDependency> DependencyInstance { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    Optional<object> IDependencyRequest.DependencyInstance
    {
        get => DependencyInstance.AsOptional<object>();
        set => DependencyInstance = value.AsOptional<TDependency>();
    }
}

/// <summary>
/// Represents a request that contains a dependency type that gets resolved by a provider.
/// </summary>
/// <typeparam name="TDependency">The type of the dependency.</typeparam>
public abstract record DependencyRequest<TDependency> : IDependencyRequest<TDependency>
    where TDependency : class
{
    /// <inheritdoc />
    public required object DependencyKeyId { get; init; }

    /// <inheritdoc />
    public Optional<TDependency> DependencyInstance { get; set; } = Optional.Empty<TDependency>();
}