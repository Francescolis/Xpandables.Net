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
/// Base class for aspect implementation interceptor.
/// </summary>
/// <typeparam name="TAspectAttribute">The type of the aspect attribute.</typeparam>
public abstract class OnAspect<TAspectAttribute> : Interceptor
    where TAspectAttribute : AspectAttribute
{
    /// <summary>
    /// Gets the aspect attribute applied on the method.
    /// </summary>  
    protected TAspectAttribute AspectAttribute { get; private set; } = default!;

    ///<inheritdoc/>
    public sealed override bool CanHandle(IInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(invocation);

        AspectAttribute = GetAspectAttribute(invocation);

        if (AspectAttribute.IsInterfaceImplemented is false)
        {
            Type target = GetRealInstance(invocation).GetType();
            if (!target.IsAssignableFromInterface(AspectAttribute.InterfaceType))
            {
                throw new InvalidOperationException(
                    $"{target.Name} must implement " +
                    $"{AspectAttribute.InterfaceType.Name}.");
            }
        }

        return CanHandleInvocation(invocation);
    }

    ///<inheritdoc/>
    public sealed override void Intercept(IInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(invocation);

        if (AspectAttribute.IsDisabled)
        {
            invocation.Proceed();
            return;
        }

        InterceptCore(invocation);
    }

    /// <summary>
    /// When implemented in a derived class, intercepts the method invocation.
    /// </summary>
    /// <param name="invocation">The method argument to be called.</param>
    protected abstract void InterceptCore(IInvocation invocation);

    /// <summary>
    /// Returns a flag indicating if this behavior will actually 
    /// do anything when invoked.
    /// This is used to optimize interception. If the behaviors 
    /// won't actually do anything then the interception
    /// mechanism can be skipped completely.
    /// Returns <see langword="true"/> if so, otherwise <see langword="false"/>.
    /// The default behavior returns <see langword="true"/>.
    /// </summary>
    /// <param name="invocation">The method argument to be called.</param>
    /// <returns><see langword="true"/> if it can handle the argument, 
    /// otherwise <see langword="false"/></returns>
    protected virtual bool CanHandleInvocation(IInvocation invocation)
        => invocation is not null;

    /// <summary>
    /// Returns the aspect attribute applied on the method.
    /// </summary>
    /// <param name="invocation">The method argument to be called.</param>
    /// <returns>The aspect attribute applied on the method.</returns>
    protected static TAspectAttribute GetAspectAttribute(IInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(invocation);

        Type target = GetRealInstance(invocation).GetType();

        return target
            .GetMethod(invocation.Method.Name)?
            .GetCustomAttributes(true)
            .OfType<TAspectAttribute>()
            .FirstOrDefault()
            ?? target
            .GetCustomAttributes(true)
            .OfType<TAspectAttribute>()
            .First();
    }

    /// <summary>
    /// Returns the real instance of the invocation target.
    /// </summary>
    /// <param name="invocation">The method argument to be called.</param>
    /// <returns>The real instance of the invocation target.</returns>
    protected static object GetRealInstance(IInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(invocation);

        object target = invocation.Target;

        while (target is InterceptorProxy proxy)
        {
            target = proxy.Instance;
        }

        return target;
    }
}

/// <summary>
/// Base class for aspect implementation.
/// </summary>
/// <typeparam name="TAspectAttribute">The type of the aspect attribute.</typeparam>
/// <typeparam name="TInterface">The type of the interface.</typeparam>
public abstract class OnAspect<TAspectAttribute, TInterface> :
    OnAspect<TAspectAttribute>
    where TAspectAttribute : AspectAttribute
    where TInterface : class
{
}
