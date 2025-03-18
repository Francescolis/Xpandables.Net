
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
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Executions;

namespace Xpandables.Net.DataAnnotations;

/// <summary>
/// Validates the execution result by using the provided validators.
/// </summary>
public sealed class EndpointValidator : IEndpointValidator
{
    /// <inheritdoc/>
    public async ValueTask<object?> ValidateAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        List<ArgumentDescriptor> arguments = [.. context
            .Arguments
            .OfType<IValidationEnabled>()
            .Select((parameter, index) => new ArgumentDescriptor
            {
                Index = index,
                Parameter = parameter,
                ParameterType = parameter.GetType()
            })];

        if (arguments.Count == 0)
        {
            return await next(context).ConfigureAwait(false);
        }

        IExecutionResultFailureBuilder failureBuilder = ExecutionResults.BadRequest();

        foreach (ValidatorDescriptor descriptor in GetValidatorDescriptors())
        {
            try
            {
                IExecutionResult executionResult = await descriptor.Validator
                    .ValidateAsync(descriptor.Argument)
                    .ConfigureAwait(false);

                if (executionResult.IsFailureStatusCode())
                {
                    _ = failureBuilder
                        .Merge(executionResult);
                }
            }
            catch (ValidationException validationException)
            {
                _ = failureBuilder.Merge(validationException.ToExecutionResult());
            }
            catch (ExecutionResultException executionException)
            {
                _ = failureBuilder.Merge(executionException.ExecutionResult);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"An error occurred while validating the argument " +
                    $"'{descriptor.ArgumentType.Name}' at index '{descriptor.ArgumentIndex}'.",
                    exception);
            }
        }

        IExecutionResult result = failureBuilder.Build();

        if (result.Errors.Any())
        {
            return result.ToMinimalResult();
        }

        return await next(context).ConfigureAwait(false);

        IEnumerable<ValidatorDescriptor> GetValidatorDescriptors()
        {
            IValidatorProvider provider = context
                .HttpContext
                .RequestServices
                .GetRequiredService<IValidatorProvider>();

            foreach (ArgumentDescriptor argument in arguments)
            {
                IValidator? validator = provider.GetValidator(argument.ParameterType);
                if (validator is not null)
                {
                    yield return new ValidatorDescriptor
                    {
                        ArgumentIndex = argument.Index,
                        ArgumentType = argument.ParameterType,
                        Argument = argument.Parameter,
                        Validator = validator
                    };
                }
            }
        }
    }
}

internal readonly record struct ArgumentDescriptor
{
    public required int Index { get; init; }
    public required IValidationEnabled Parameter { get; init; }
    public required Type ParameterType { get; init; }
}

internal readonly record struct ValidatorDescriptor
{
    public required int ArgumentIndex { get; init; }
    public required Type ArgumentType { get; init; }
    public required IValidationEnabled Argument { get; init; }
    public required IValidator Validator { get; init; }
}
