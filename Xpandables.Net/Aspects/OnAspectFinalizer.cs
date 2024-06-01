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
/// This class adds finalizer functionality to the method of the implementation of
/// the interface decorated with <see cref="AspectFinalizerAttribute{TInterface}"/>.
/// </summary>
/// <typeparam name="TInterface">The type of the interface.</typeparam>
/// <param name="aspectFinalizer">The aspect finalizer.</param>
public sealed class OnAspectFinalizer<TInterface>(IAspectFinalizer aspectFinalizer) :
    OnAspect<AspectFinalizerAttribute<TInterface>, TInterface>
    where TInterface : class
{
    ///<inheritdoc/>
    protected override void InterceptCore(IInvocation invocation)
    {
        invocation.ReThrowException = AspectAttribute.CallFinalizerOnException;

        try
        {
            invocation.Proceed();
            if (invocation.Exception is { } ex
                && AspectAttribute.CallFinalizerOnException)
            {
                object result = aspectFinalizer.Finalizer.Invoke(ex);

                if (result is Exception reThrow)
                    invocation.SetException(reThrow);
                else
                    invocation.SetReturnValue(result);
            }
            else if (invocation.Exception is null)
            {
                object result = aspectFinalizer.Finalizer
                    .Invoke(invocation.ReturnValue);
                if (result is Exception reThrow)
                    invocation.SetException(reThrow);
                else
                    invocation.SetReturnValue(result);
            }
        }
        catch (Exception exception)
            when (AspectAttribute.CallFinalizerOnException is true)
        {
            object result = aspectFinalizer.Finalizer.Invoke(exception);

            if (result is Exception reThrow)
                invocation.SetException(reThrow);
            else
                invocation.SetReturnValue(result);
        }
    }
}
