
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
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;

using Xpandables.Net.Executions;

namespace Xpandables.Net.DataAnnotations;

/// <summary>
/// Validates endpoint arguments asynchronously using registered validators. 
/// Returns validation results or proceeds to the next delegate if valid.
/// </summary>
/// <param name="validatorProvider">The validator provider.</param>
public sealed class EndpointValidator(IValidatorProvider validatorProvider) : IEndpointValidator
{
    /// <inheritdoc/>
    public async ValueTask<object?> ValidateAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        ImmutableHashSet<ArgumentDescriptor> arguments = GetArgumentDescriptors(context);

        if (arguments.Count == 0)
        {
            return await next(context).ConfigureAwait(false);
        }

        ImmutableHashSet<ValidatorDescriptor> validators = GetAppropriateValidators(arguments, validatorProvider);

        ExecutionResult execution = await ApplyValidationAsync(validators).ConfigureAwait(false);

        if (execution.Errors.Any())
        {
            return execution.ToMinimalResult();
        }

        return await next(context).ConfigureAwait(false);
    }

    static async Task<ExecutionResult> ApplyValidationAsync(ImmutableHashSet<ValidatorDescriptor> validators)
    {
        IExecutionResultFailureBuilder failureBuilder = ExecutionResult.BadRequest();

        foreach (ValidatorDescriptor descriptor in validators)
        {
            try
            {
                ExecutionResult executionResult = await descriptor.Validator
                    .ValidateAsync(descriptor.Argument)
                    .ConfigureAwait(false);

                if (!executionResult.IsSuccessStatusCode)
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

        return failureBuilder.Build();
    }

    static ImmutableHashSet<ArgumentDescriptor> GetArgumentDescriptors(EndpointFilterInvocationContext context)
    {
        List<ArgumentDescriptor> arguments = [.. context
            .Arguments
            .OfType<IRequiresValidation>()
            .Select((parameter, index) => new ArgumentDescriptor
            {
                Index = index,
                Parameter = parameter,
                ParameterType = parameter.GetType()
            })];

        return [.. arguments];
    }

    static ImmutableHashSet<ValidatorDescriptor> GetAppropriateValidators(
        ImmutableHashSet<ArgumentDescriptor> arguments, IValidatorProvider provider)
    {
        List<ValidatorDescriptor> validators = [];
        foreach (ArgumentDescriptor argument in arguments)
        {
            IValidator? validator = provider.TryGetValidator(argument.ParameterType);
            if (validator is not null)
            {
                validators.Add(new ValidatorDescriptor
                {
                    ArgumentIndex = argument.Index,
                    ArgumentType = argument.ParameterType,
                    Argument = argument.Parameter,
                    Validator = validator
                });
            }
        }

        return [.. validators];
    }
}

internal readonly record struct ArgumentDescriptor
{
    public required int Index { get; init; }
    public required IRequiresValidation Parameter { get; init; }
    public required Type ParameterType { get; init; }
}

internal readonly record struct ValidatorDescriptor
{
    public required int ArgumentIndex { get; init; }
    public required Type ArgumentType { get; init; }
    public required IRequiresValidation Argument { get; init; }
    public required IValidator Validator { get; init; }
}
