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
using Xpandables.Net.Interceptions;

namespace Xpandables.Net.Aspects;

/// <summary>
/// Base class for all aspect attributes.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = true)]
public abstract class AspectAttribute(Type interfaceType) : InterceptorAttribute
{
    /// <summary>
    /// Gets the zero-base order in which the aspect will be applied.
    /// The default value is zero.
    /// </summary>
    public virtual int Order { get; set; }

    /// <summary>
    /// Determines whether the aspect is disabled.
    /// </summary>
    /// <remarks>The default value is <see langword="false"/>.</remarks>
    public bool IsDisabled { get; set; }

    /// <summary>
    /// Gets the interface type implemented by the decorated class.
    /// </summary>
    public Type InterfaceType => interfaceType
        .IsInterface is false
            ? throw new InvalidOperationException(
                $"{interfaceType.Name} is not an interface.")
        : interfaceType;
}


/// <summary>
/// Base class for all aspect attributes.
/// </summary>
/// <typeparam name="TInterface">The interface type to intercept.</typeparam>
public abstract class AspectAttribute<TInterface> : AspectAttribute
    where TInterface : class
{
    /// <summary>
    /// Constructs a new instance of <see cref="AspectAttribute{TInterface}"/>.
    /// </summary>
    protected AspectAttribute() : base(typeof(TInterface))
    {
        if (!typeof(TInterface).IsInterface)
        {
            throw new InvalidOperationException(
                $"{typeof(TInterface).Name} is not an interface.");
        }
    }
}
