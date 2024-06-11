
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
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Validators;

namespace Xpandables.Net.Operations;

internal readonly record struct ArgumentDescriptor(
    int Index,
    object? Parameter,
    Type? ParameterType);

internal readonly record struct ValidationDescriptor(
    int ArgumentIndex,
    Type ArgumentType,
    object? Argument,
    IValidator Validator);

#pragma warning disable CA1812 // Avoid uninstantiated internal classes
internal sealed class OperationResultRequestValidator :
    IOperationResultRequestValidator
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
{
    public async ValueTask<object?> ValidateAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        List<ArgumentDescriptor> arguments = context
            .Arguments
            .Select((p, i) => new ArgumentDescriptor(i, p, p?.GetType()))
            .ToList();

        IOperationResult.IFailureBuilder operationResultBuilder =
            OperationResults.BadRequest();

        foreach (ValidationDescriptor validationDescriptor
            in GetValidationDescriptors())
        {
            if (validationDescriptor.Argument is null)
            {
                continue;
            }

            try
            {
                IOperationResult operation = await validationDescriptor.Validator
                    .ValidateAsync(validationDescriptor.Argument)
                    .ConfigureAwait(false);

                if (operation.IsFailure)
                {
                    _ = operationResultBuilder
                        .WithErrors(operation.Errors)
                        .WithHeaders(operation.Headers);
                }
            }
            catch (ValidationException exception)
            {
                IOperationResult operationException = exception
                    .ToOperationResult();
                _ = operationResultBuilder
                    .WithErrors(operationException.Errors)
                    .WithHeaders(operationException.Headers);
            }
        }

        IOperationResult operationResult = operationResultBuilder.Build();

        if (operationResult.Errors.Any())
        {
            return operationResult.ToMinimalResult();
        }

        return await next(context).ConfigureAwait(false);

        IEnumerable<ValidationDescriptor> GetValidationDescriptors()
        {
            foreach (ArgumentDescriptor item in arguments
                .Where(ValidatorPredicate))
            {
                if (!typeof(IValidator<>)
                    .TryMakeGenericType(
                    out Type? validatorType,
                    out _,
                    item.ParameterType!))
                {
                    continue;
                }

                IEnumerable<IValidator> validators = context
                    .HttpContext
                    .RequestServices
                    .GetServices(validatorType).OfType<IValidator>();

                foreach (IValidator validator in validators)
                {
                    yield return new(
                        item.Index,
                        item.ParameterType!,
                        item.Parameter, validator);
                }
            }
        }

    }

    private static bool ValidatorPredicate(ArgumentDescriptor argument)
        => argument.ParameterType != null
            && (argument.ParameterType.IsClass
                || (argument.ParameterType.IsValueType
                && !argument.ParameterType.IsEnum))
            && OperationResultValidatorFilter
                .ValidatorPredicate(argument.ParameterType);
}
