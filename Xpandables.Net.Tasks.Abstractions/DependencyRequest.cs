
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

using System.Net.Optionals;

namespace Xpandables.Net.Tasks;

/// <summary>
/// Represents a request for a dependency of a specified type, including its key identifier and an optional instance.
/// </summary>
/// <typeparam name="TDependency">The type of the dependency being requested. Must be a reference type.</typeparam>
public abstract record DependencyRequest<TDependency> : IDependencyRequest<TDependency>
    where TDependency : class
{
    /// <inheritdoc />
    public required object DependencyKeyId { get; init; }

    /// <inheritdoc />
    public Optional<TDependency> DependencyInstance { get; set; } = Optional.Empty<TDependency>();
}