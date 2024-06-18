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
using Xpandables.Net.Aspects;

namespace Xpandables.Net.Interceptions;

/// <summary>
/// A collection of extension methods for the <see cref="IInterceptor"/> 
/// interface.
/// </summary>
public static class InterceptorExtensions
{
    /// <summary>
    /// Validates and returns the aspect attribute applied on the method.
    /// </summary>
    /// <param name="invocation">The invocation to act on.</param>
    public static TAspectAttribute ValidateAttribute<TAspectAttribute>
        (this IInvocation invocation)
        where TAspectAttribute : AspectAttribute
    {
        TAspectAttribute attribute =
            GetAspectAttribute<TAspectAttribute>(invocation);

        if (attribute.IsRegisteredByDI is false)
        {
            Type target = GetRealInstance(invocation).GetType();
            if (!target.IsAssignableFromInterface(attribute.InterfaceType))
            {
                throw new InvalidOperationException(
                    $"{target.Name} must implement " +
                    $"{attribute.InterfaceType.Name}.");
            }

            if ((target.IsGenericTypeDefinition
                 && !attribute.InterfaceType.IsGenericTypeDefinition)
                 || (!target.IsGenericTypeDefinition
                       && attribute.InterfaceType.IsGenericTypeDefinition))
            {
                throw new InvalidOperationException(
                    $"{target.Name} and {attribute.InterfaceType.Name} " +
                    "must be both generic or non-generic.");
            }
        }

        return attribute;
    }

    /// <summary>
    /// Returns the real instance of the invocation target.
    /// </summary>
    /// <param name="invocation">The method argument to be called.</param>
    /// <returns>The real instance of the invocation target.</returns>
    public static object GetRealInstance(this IInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(invocation);

        object target = invocation.Target;

        while (target is InterceptorProxy proxy)
        {
            target = proxy.Instance;
        }

        return target;
    }

    /// <summary>
    /// Returns the aspect attribute applied on the method.
    /// </summary>
    /// <param name="invocation">The method argument to be called.</param>
    /// <returns>The aspect attribute applied on the method.</returns>
    public static TAspectAttribute GetAspectAttribute
        <TAspectAttribute>(this IInvocation invocation)
        where TAspectAttribute : AspectAttribute
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
}
