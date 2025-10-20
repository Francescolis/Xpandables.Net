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

namespace Xpandables.Net.Cqrs;

/// <summary>
/// Defines a contract for providing dependencies based on specified types or requests.
/// </summary>
/// <remarks>Implementations of this interface are responsible for determining if they can provide a specific
/// dependency and for retrieving the dependency when requested. This interface supports both synchronous type-checking
/// and asynchronous dependency retrieval.</remarks>
public interface IDependencyProvider
{
    /// <summary>
    /// Determines whether the provider can provide the dependency of the specified type.
    /// </summary>
    bool CanProvideDependency(Type dependencyType);

    /// <summary>
    /// Gets the dependency for the request.
    /// </summary>
    /// <param name="request">The request that needs the dependency.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation 
    /// requests.</param>
    /// <returns>The dependency expected.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    Task<object> GetDependencyAsync(
        IDependencyRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the dependency of the specified type.
    /// </summary>
    /// <typeparam name="TDependency">The type of the dependency.</typeparam>
    /// <param name="decider">The decider of the dependency.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation 
    /// requests.</param>
    /// <returns>The dependency of the specified type.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<TDependency> GetDependencyAsync<TDependency>(
        IDependencyRequest<TDependency> decider,
        CancellationToken cancellationToken = default)
        where TDependency : class =>
        (TDependency)await GetDependencyAsync(
            (IDependencyRequest)decider, cancellationToken)
            .ConfigureAwait(false);
}
