
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
using System.Net.Optionals;

namespace System.Net.Tasks;

/// <summary>
/// Represents a request for a dependency, providing information about the dependency type, key identifier, and instance
/// used by a component.
/// </summary>
/// <remarks>Implementations of this interface are typically used in dependency resolution scenarios to specify
/// which dependency is required, optionally by key, and to supply or retrieve the resolved instance. The interface
/// extends <see cref="IRequest"/>, allowing it to be used in broader request handling contexts.</remarks>
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
/// Defines a request for a dependency of a specified reference type, providing access to its type and an optional
/// instance.
/// </summary>
/// <typeparam name="TDependency">The type of the dependency to be requested. Must be a reference type.</typeparam>
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
        get => DependencyInstance.ToOptional<object>();
        set => DependencyInstance = value.ToOptional<TDependency>();
    }
}
