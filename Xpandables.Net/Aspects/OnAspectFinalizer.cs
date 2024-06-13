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
/// the interface decorated with <see cref="AspectFinalizerAttribute"/>.
/// </summary>
/// <param name="aspectFinalizer">The aspect finalizer.</param>
public sealed class OnAspectFinalizer(IAspectFinalizer aspectFinalizer) :
    OnAspect<AspectFinalizerAttribute>
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
                DoFinalize(ex);
            }
            else if (invocation.Exception is null)
            {
                DoFinalize(invocation.ReturnValue);
            }
        }
        catch (Exception exception)
            when (AspectAttribute.CallFinalizerOnException is true)
        {
            DoFinalize(exception);
        }

        void DoFinalize(object? value)
        {
            object result = aspectFinalizer.Finalizer.Invoke(value);

            if (result is Exception reThrow)
            {
                invocation.SetException(reThrow);
            }
            else
                if (invocation.ReturnType != typeof(void))
            {
                invocation.SetReturnValue(result);
            }
        }
    }
}
