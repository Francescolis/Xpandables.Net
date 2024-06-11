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
/// the interface decorated with <see cref="AspectValidatorAttribute{TInterface}"/>.
/// </summary> 
/// <typeparam name="TInterface">The type of the interface.</typeparam>
/// <param name="serviceProvider">The service provider.</param>
public sealed class OnAspectValidator<TInterface>(IServiceProvider serviceProvider) :
    OnAspect<AspectValidatorAttribute<TInterface>, TInterface>
    where TInterface : class
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

            if (validator?.Validate(argument.Value)
                is IOperationResult { IsFailure: true } failure)
            {
                errors.Merge(failure.Errors);
            }
        }

        if (errors.Any())
        {
            invocation.ReThrowException = true;

            IOperationResult result = OperationResults
                .BadRequest()
                .WithErrors(errors)
                .Build();

            if (invocation
                .ReturnType
                .GetUnderlyingReturnTypeFromTaskOrValueTask() is Type returnType
                && returnType.IsGenericType)
            {
                result = result
                    .ToOperationResult(returnType.GetGenericArguments()[0]);
            }

            if (AspectAttribute.ThrowException)
            {
                invocation.SetException(
                    new OperationResultException(result));
            }
            else
            {
                invocation.SetReturnValue(result);
            }
        }
        else
        {
            invocation.Proceed();
        }
    }
}
