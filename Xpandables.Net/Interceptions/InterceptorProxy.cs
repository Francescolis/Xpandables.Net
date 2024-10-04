
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
using System.Diagnostics;
using System.Reflection;

namespace Xpandables.Net.Interceptions;

/// <summary>
/// The base class for interceptor that contains common types.
/// </summary>
public abstract class InterceptorProxy : DispatchProxy
{
    internal Type InterfaceType { get; set; } = default!;
    internal object Instance { get; set; } = default!;
    internal IInterceptor Interceptor { get; set; } = default!;

    /// <summary>
    /// Contains the GetType method.
    /// </summary>
    protected static readonly MethodBase MethodBaseType
        = typeof(object).GetMethod("GetType")!;

    /// <summary>
    /// Returns a new instance of <typeparamref name="TInterface"/> 
    /// wrapped by a proxy.
    /// </summary>
    /// <param name="instance">the instance to be wrapped.</param>
    /// <param name="interceptor">The instance of the interceptor.</param>
    /// <returns>An instance that has been wrapped by a proxy.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="instance"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="interceptor"/> is null.</exception>
    public static TInterface CreateProxy<TInterface>(
        TInterface instance,
        IInterceptor interceptor)
        where TInterface : class
    {
        object proxy = Create<TInterface, InterceptorProxy<TInterface>>();

        ((InterceptorProxy<TInterface>)proxy)
            .SetParameters(instance, interceptor);

        return (TInterface)proxy;
    }
}

/// <summary>
/// The base implementation for interceptor.
/// This implementation uses the <see cref="DispatchProxy" /> 
/// process to apply customer behaviors to a method.
/// </summary>
/// <typeparam name="TInterface">Type of interface.</typeparam>
public class InterceptorProxy<TInterface> : InterceptorProxy
    where TInterface : class
{
    /// <summary>
    /// Initializes a new instance of 
    /// <see cref="InterceptorProxy{TInstance}"/> with default values.
    /// </summary>
    public InterceptorProxy() { }

    /// <summary>
    /// Initializes the decorated instance and the interceptor 
    /// with the provided arguments.
    /// </summary>
    /// <param name="instance">The instance to be intercepted.</param>
    /// <param name="interceptor">The instance of interceptor.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="instance"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The
    /// <paramref name="interceptor"/> is null.</exception>
    internal void SetParameters(TInterface instance, IInterceptor interceptor)
    {
        Instance = instance
            ?? throw new ArgumentNullException(nameof(instance));
        Interceptor = interceptor
            ?? throw new ArgumentNullException(nameof(interceptor));
        InterfaceType = typeof(TInterface);
    }


    /// <summary>
    /// Executes the method specified in the <paramref name="targetMethod" />.
    /// Applies the interceptor behavior to the called method.
    /// </summary>
    /// <param name="targetMethod">The target method.</param>
    /// <param name="args">The expected arguments.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="targetMethod" /> is null.</exception>
    protected sealed override object? Invoke(
        MethodInfo? targetMethod,
        object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(targetMethod);

        try
        {
            return ReferenceEquals(targetMethod, MethodBaseType)
                ? Bypass(targetMethod, args)
                : DoInvoke(targetMethod, args);
        }
        catch (TargetInvocationException exception)
        {
            if (exception.InnerException is not null)
            {
                throw exception.InnerException;
            }

            throw;
        }
    }

    private object? DoInvoke(MethodInfo method, params object?[]? args)
    {
        Invocation invocation = new(method, Instance, InterfaceType, args);

        bool isAsyncOverridden = InterceptorProxy<TInterface>.IsMethodOverridden(
            Interceptor.GetType(), nameof(Interceptor.InterceptAsync));

        bool isSyncOverridden = InterceptorProxy<TInterface>.IsMethodOverridden(
            Interceptor.GetType(), nameof(Interceptor.Intercept));

        if (Interceptor.CanHandle(invocation))
        {
            if (isAsyncOverridden)
            {
                return DoInvokeAsync(invocation);
            }
            else if (isSyncOverridden)
            {
                Interceptor.Intercept(invocation);

                if (invocation.Exception is not null)
                {
                    if (invocation.ReThrowException)
                    {
                        throw invocation.Exception;
                    }
                }
            }
        }
        else
        {
            invocation.Proceed();
        }

        return invocation.ReturnValue;
    }

    private async Task<object?> DoInvokeAsync(IInvocation invocation)
    {
        await Interceptor.InterceptAsync(invocation);

        if (invocation.Exception is not null)
        {
            if (invocation.ReThrowException)
            {
                throw invocation.Exception;
            }
        }

        return invocation.ReturnValue;
    }

    private static bool IsMethodOverridden(Type derivedType, string methodName) =>
        derivedType.GetMethod(
            methodName,
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.DeclaredOnly) != null;

    /// <summary>
    /// Bypass the interceptor application because 
    /// the method is a system method (GetType).
    /// </summary>
    /// <param name="targetMethod">Contains all information 
    /// about the method being executed</param>
    /// <param name="args">Arguments to be used.</param>
    /// <returns><see cref="object"/> instance</returns>
    [DebuggerStepThrough]
    private object? Bypass(
        MethodInfo targetMethod,
        object?[]? args) => targetMethod.Invoke(Instance, args);
}
