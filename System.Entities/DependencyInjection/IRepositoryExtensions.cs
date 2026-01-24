/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
#pragma warning disable IDE0130 // Namespace does not match folder structure
using System.Entities;

namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for repository types to enable ambient context injection.
/// </summary>
public static class IRepositoryExtensions
{
    /// <summary>
    /// <see cref="IRepository"/> extensions.
    /// </summary>
    extension(IRepository repository)
    {
        /// <summary>
        /// Injects the specified ambient <typeparamref name="TContext"/> into the repository instance,
        /// enabling it to participate in the current unit of work.
        /// </summary>
        /// <remarks>
        /// The repository must implement <see cref="IAmbientContextReceiver{TContext}"/> 
        /// to receive the ambient context. This is an AOT-compliant approach that avoids reflection.
        /// </remarks>
        /// <param name="context">The <typeparamref name="TContext"/> to inject into the repository.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the repository does not implement <see cref="IAmbientContextReceiver{TContext}"/>.
        /// </exception>
        public void InjectAmbientContext<TContext>(TContext context)
            where TContext : class
        {
            ArgumentNullException.ThrowIfNull(context);

            if (repository is IAmbientContextReceiver<TContext> receiver)
            {
                receiver.SetAmbientContext(context);
            }
            else
            {
                throw new InvalidOperationException(
                    $"The repository type '{repository.GetType().Name}' does not implement " +
                    $"'{nameof(IAmbientContextReceiver<>)}'. " +
                    $"Ensure the repository implements this interface to support ambient context injection.");
            }
        }
    }
}