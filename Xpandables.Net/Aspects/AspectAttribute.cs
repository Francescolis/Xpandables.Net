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
public abstract class AspectAttribute : InterceptorAttribute
{
    /// <summary>
    /// Constructs a new instance of <see cref="AspectAttribute"/>.
    /// </summary>
    /// <param name="interfaceType">The interface type to intercept.</param>
    /// <exception cref="InvalidOperationException">If the interface type is 
    /// not an interface.</exception>
    protected AspectAttribute(Type interfaceType)
    {
        ArgumentNullException.ThrowIfNull(interfaceType);

        InterfaceType = interfaceType
            .IsInterface is false
            ? throw new InvalidOperationException(
                $"{interfaceType.Name} must be an interface.")
        : interfaceType;
    }

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
    public Type InterfaceType { get; }

    internal bool IsRegisteredByDI { get; set; }
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
    /// <exception cref="InvalidOperationException">If the interface type is
    /// not an interface.</exception>
    protected AspectAttribute() : base(typeof(TInterface)) { }
}
