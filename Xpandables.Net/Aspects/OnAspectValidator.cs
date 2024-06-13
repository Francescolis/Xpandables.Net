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
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Aspects;

/// <summary>
/// This class adds validation to the method arguments of implementation of
/// the interface decorated with <see cref="AspectValidatorAttribute"/>
/// or <see cref="AspectValidatorAttribute{TInterface}"/>.
/// </summary> 
/// <param name="serviceProvider">The service provider.</param>
public sealed class OnAspectValidator<TAttribute>(
    IServiceProvider serviceProvider) : OnAspect<TAttribute>
    where TAttribute : _AspectValidatorAttribute<TAttribute>
{
    ///<inheritdoc/>
    protected override void InterceptCore(IInvocation invocation)
    {
        ElementCollection errors = [];

        foreach (Parameter argument in invocation.Arguments
            .Where(p => p.PassingBy == Parameter.PassingState.In
                && p.Type != typeof(CancellationToken)))
        {
            Type type = typeof(IAspectValidator<>)
                .MakeGenericType(argument.Type);

            IAspectValidator? validator = serviceProvider
                .GetService(type) as IAspectValidator;

            try
            {
                if (validator?.Validate(argument.Value)
                    is IOperationResult { IsFailure: true } failure)
                {
                    errors.Merge(failure.Errors);
                }
            }
            catch (OperationResultException exception)
            {
                errors.Merge(exception.Operation.Errors);
            }
        }

        if (errors.Any())
        {
            invocation.ReThrowException = true;

            IOperationResult result = OperationResults
                .BadRequest()
                .WithErrors(errors)
                .Build();

            Type returnType = invocation
                .ReturnType
                .GetUnderlyingReturnTypeFromTaskOrValueTask()
                ?? invocation.ReturnType;

            if (returnType.IsAssignableFromInterface(typeof(IOperationResult))
                && returnType.IsGenericType)
            {
                result = result
                    .ToOperationResult(returnType.GetGenericArguments()[0]);

                if (!AspectAttribute.ThrowException)
                {
                    invocation.SetReturnValue(result);
                    return;
                }
            }

            invocation.SetException(
                new OperationResultException(result));
        }
        else
        {
            invocation.Proceed();
        }
    }
}
