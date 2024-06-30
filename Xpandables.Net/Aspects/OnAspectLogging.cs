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

///<summary>
/// This class adds logging functionality to the method of the implementation of
/// the interface decorated with <see cref="AspectLoggingAttribute"/>.  
/// </summary>
/// <param name="aspectLogging">The aspect logging.</param>
public sealed class OnAspectLogging(IAspectLogger aspectLogging) :
    OnAspect<AspectLoggingAttribute>
{
    ///<inheritdoc/>
    protected override void InterceptCore(IInvocation invocation)
    {
        LoggingStateEntry entry = new()
        {
            ClassName = invocation.Target.GetTypeName(),
            MethodName = invocation.Method.Name,
            Arguments = invocation.Arguments
        };

        aspectLogging.OnEntry(entry);

#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            invocation.Proceed();

            if (invocation.Exception is not null)
            {
                LoggingStateFailure failure = new()
                {
                    ClassName = invocation.Target.GetTypeName(),
                    MethodName = invocation.Method.Name,
                    Arguments = invocation.Arguments,
                    Exception = invocation.Exception
                };

                aspectLogging.OnFailure(failure);
            }
            else
            {
                LoggingStateSuccess success = new()
                {
                    ClassName = invocation.Target.GetTypeName(),
                    MethodName = invocation.Method.Name,
                    Arguments = invocation.Arguments,
                    ReturnValue = invocation.ReturnValue
                };

                aspectLogging.OnSuccess(success);
            }
        }
        catch (Exception exception)
        {
            LoggingStateFailure failure = new()
            {
                ClassName = invocation.Target.GetTypeName(),
                MethodName = invocation.Method.Name,
                Arguments = invocation.Arguments,
                Exception = exception.InnerException ?? exception
            };

            aspectLogging.OnFailure(failure);
            invocation.SetException(exception.InnerException ?? exception);
        }
        finally
        {
            LoggingStateExit exit = new()
            {
                ClassName = invocation.Target.GetTypeName(),
                MethodName = invocation.Method.Name,
                Arguments = invocation.Arguments,
                ReturnValue = invocation.ReturnValue,
                Exception = invocation.Exception,
                ElapsedTime = invocation.ElapsedTime
            };

            aspectLogging.OnExit(exit);
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }
}
