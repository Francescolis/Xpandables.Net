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
namespace Xpandables.Net.Commands;

/// <summary>
/// Provides a mechanism to get 
/// <see cref="ICommandHandler{TCommand, TDependency}"/> dependencies asynchronously.
/// </summary>
public interface ICommandDeciderDependencyProvider
{
    /// <summary>
    /// Gets the dependency of the specified type asynchronously.
    /// </summary>
    /// <param name="command">The command of the dependency.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation 
    /// requests.</param>
    /// <returns>The dependency of the specified type.</returns>
    Task<object> GetDependencyAsync(
        ICommandDecider command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the dependency of the specified type asynchronously.
    /// </summary>
    /// <typeparam name="TDependency">The type of the dependency.</typeparam>
    /// <param name="command">The command of the dependency.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation 
    /// requests.</param>
    /// <returns>The dependency of the specified type.</returns>
    public async Task<TDependency> GetDependencyAsync<TDependency>(
        ICommandDecider<TDependency> command,
        CancellationToken cancellationToken = default)
        where TDependency : class =>
        (TDependency)await GetDependencyAsync(
            (ICommandDecider)command, cancellationToken)
            .ConfigureAwait(false);
}
