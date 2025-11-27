/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.ExecutionResults;

using AspNetCore.Net;

using Microsoft.AspNetCore.Http;

namespace AspNetCore.Net;

/// <summary>
/// Provides endpoint argument validation for minimal API endpoints using the specified validator provider.
/// </summary>
/// <remarks>This class is intended for use with minimal API endpoints that require automatic validation of
/// arguments implementing the validation interface. Validation failures are returned as execution results, allowing
/// endpoints to handle errors consistently. The validator provider determines which validators are applied to each
/// argument type.</remarks>
/// <param name="validatorProvider">The provider used to retrieve validators for endpoint arguments requiring validation.</param>
public sealed class MinimalResultEndpointValidator(IRuleValidatorProvider validatorProvider) : IMinimalResultEndpointValidator
{
    /// <inheritdoc/>
    public async ValueTask<object?> ValidateAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate nextDelegate)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(nextDelegate);

        ImmutableHashSet<ArgumentDescriptor> arguments = GetArgumentDescriptors(context);

        if (arguments.Count == 0)
        {
            return await nextDelegate(context).ConfigureAwait(false);
        }

        ImmutableHashSet<ValidatorDescriptor> validators = GetAppropriateValidators(arguments, validatorProvider);

        OperationResult execution = await ApplyValidationAsync(validators).ConfigureAwait(false);

        if (!execution.Errors.IsEmpty)
        {
            return execution;
        }

        return await nextDelegate(context).ConfigureAwait(false);
    }


    static async Task<OperationResult> ApplyValidationAsync(ImmutableHashSet<ValidatorDescriptor> validators)
    {
        IOperationResultFailureBuilder failureBuilder = OperationResult.BadRequest();

        foreach (ValidatorDescriptor descriptor in validators)
        {
            try
            {
                var validationResults = await descriptor.Validator
                    .ValidateAsync(descriptor.Argument)
                    .ConfigureAwait(false);

                if (validationResults.Count == 0)
                {
                    continue;
                }

                _ = failureBuilder.Merge(validationResults.ToExecutionResult());

            }
            catch (ValidationException validationException)
            {
                _ = failureBuilder.Merge(validationException.ToOperationResult());
            }
            catch (OperationResultException executionException)
            {
                _ = failureBuilder.Merge(executionException.OperationResult);
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
        ImmutableHashSet<ArgumentDescriptor> arguments, IRuleValidatorProvider provider)
    {
        List<ValidatorDescriptor> validators = [];
        foreach (ArgumentDescriptor argument in arguments)
        {
            IRuleValidator? validator = provider.TryGetValidator(argument.ParameterType);
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
    public readonly required int Index { get; init; }
    public readonly required IRequiresValidation Parameter { get; init; }
    public readonly required Type ParameterType { get; init; }
}

internal readonly record struct ValidatorDescriptor
{
    public readonly required int ArgumentIndex { get; init; }
    public readonly required Type ArgumentType { get; init; }
    public readonly required IRequiresValidation Argument { get; init; }
    public readonly required IRuleValidator Validator { get; init; }
}
