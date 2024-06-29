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
using System.Reflection;

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
    /// Gets the real return value of the invocation.
    /// </summary>
    /// <param name="invocation">The method argument to be called.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="invocation"/>
    /// is null.</exception>
    /// <returns>The real return value of the invocation.</returns>
    public static object? GetRealReturnValue(this IInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(invocation);

        dynamic? awaitable = invocation.ReturnValue;
        if (awaitable is null)
        {
            return null;
        }

        Type returnType = invocation.Method.ReturnType;
        bool isAsync = returnType.IsTaskType();

        if (isAsync)
        {
            Task task = awaitable;
            task.Wait();
            return task.GetType().GetProperty("Result")?.GetValue(task);
        }

        return awaitable;
    }

    /// <summary>
    /// Determines whether the type is a task type.
    /// </summary>
    /// <param name="type">The type to act on.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> 
    /// is null.</exception>
    /// <returns><see langword="true"/> if the type is a task type; 
    /// otherwise, <see langword="false"/>.</returns>
    public static bool IsTaskType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return type == typeof(Task) || type == typeof(ValueTask)
                || (type.IsGenericType
                    && (type.GetGenericTypeDefinition() == typeof(Task<>)
                   || type.GetGenericTypeDefinition() == typeof(ValueTask<>)));
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

        object target = GetRealInstance(invocation);

        Type interfaceType = invocation.InterfaceType;

        object? currentTarget = target;
        while (currentTarget is not null)
        {
            TAspectAttribute? aspectAttribute = currentTarget
                .GetType()
                .GetMethod(invocation.Method.Name)?
                .GetCustomAttributes(true)
                .OfType<TAspectAttribute>()
                .FirstOrDefault()
                ?? currentTarget
                .GetType()
                .GetCustomAttributes(true)
                .OfType<TAspectAttribute>()
                .FirstOrDefault();

            if (aspectAttribute is not null)
            {
                return aspectAttribute;
            }

            // the currentTarger is a decorator or a wrapper.
            // Attempt to find a field or property in the current instance
            // that holds an interface type instance.

            IEnumerable<MemberInfo> fieldsAndProperties = target
                .GetType()
                .GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.MemberType is MemberTypes.Field or MemberTypes.Property)
                .Where(m => interfaceType.IsAssignableFrom(m.GetMemberType()));

            object? nextInstance = null;
            foreach (MemberInfo member in fieldsAndProperties)
            {
                if (member is FieldInfo field
                    && interfaceType.IsAssignableFrom(field.FieldType))
                {
                    nextInstance = field.GetValue(currentTarget);
                }
                else if (member is PropertyInfo property
                    && interfaceType.IsAssignableFrom(property.PropertyType)
                    && property.CanRead)
                {
                    nextInstance = property.GetValue(currentTarget);
                }

                if (nextInstance != null)
                {
                    break; // Found the next instance to inspect, break the loop
                }
            }

            // Move to the next instance for the next iteration
            currentTarget = nextInstance;
        }

        // we should never reach this point.
        throw new InvalidOperationException(
            $"Unable to find an aspect attribute on the implementation class " +
            $"of {interfaceType.Name}.");
    }

    private static Type? GetMemberType(this MemberInfo member)
#pragma warning disable IDE0072 // Add missing cases
        => member.MemberType switch
        {
            MemberTypes.Field => ((FieldInfo)member).FieldType,
            MemberTypes.Property => ((PropertyInfo)member).PropertyType,
            _ => null
        };
#pragma warning restore IDE0072 // Add missing cases
}
