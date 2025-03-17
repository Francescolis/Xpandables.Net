
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

using Xpandables.Net.DataAnnotations;

namespace Xpandables.Net.Executions.Minimals;

/// <summary>
/// Represents a filter that validates endpoints in a minimal API.
/// </summary>
/// <param name="validatorProvider">The validator provider to use.</param>
public class MinimalValidationFilter(IValidatorProvider validatorProvider) : IEndpointFilter
{
    /// <summary>
    /// Validates the endpoint invocation.
    /// </summary>
    /// <param name="context">The context for the endpoint filter invocation.</param>
    /// <param name="next">The delegate to invoke the next filter in the pipeline.</param>
    /// <returns>A task that represents the asynchronous execution, containing 
    /// the result of the filter invocation.</returns>
    public virtual async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        return await InvokeAsyncCore<IValidationEnabled>(context, next)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Applies the validation filter to the request of the target route(s).
    /// </summary>
    /// <param name="context">The context for the endpoint filter invocation.</param>
    /// <param name="next">The delegate to invoke the next filter in the pipeline.</param>
    /// <returns>A task that represents the asynchronous execution, containing
    /// an object that represents the result of the filter invocation.</returns>
    protected async ValueTask<object?> InvokeAsyncCore<TArgument>(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
        where TArgument : class, IValidationEnabled
    {
        var arguments = GetArgumentDescriptors<TArgument>(context);

        if (arguments.Count == 0)
        {
            return await next(context).ConfigureAwait(false);
        }

        var validors = GetValidatorDescriptors(arguments, validatorProvider);

        var failureBuilder = ExecutionResults.BadRequest();

        foreach (var descriptor in validors)
        {
            try
            {
                var executionResult = await descriptor.Validator
                    .ValidateAsync(descriptor.ArgumentInstance)
                    .ConfigureAwait(false);

                if (executionResult.IsFailureStatusCode())
                {
                    _ = failureBuilder.Merge(executionResult);
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
                    $"An error occurred while validating the argument :" +
                    $" {descriptor.ArgumentType.Name} with the validator " +
                    $"{descriptor.Validator.GetType().Name}.",
                    exception);
            }
        }

        var result = failureBuilder.Build();

        if (result.Errors.Any())
        {
            return result.ToMinimalResult();
        }

        return await next(context).ConfigureAwait(false);
    }

    private static List<ArgumentDescriptor> GetArgumentDescriptors<TArgument>(
        EndpointFilterInvocationContext context)
        where TArgument : class, IValidationEnabled
        =>
        [.. context
            .Arguments
            .OfType<TArgument>()
            .Select((parameter, index) => new ArgumentDescriptor
            {
                Index = index,
                Parameter = parameter,
                ParameterType = parameter.GetType()
            })];

    private static IEnumerable<ValidatorDescriptor> GetValidatorDescriptors(
        List<ArgumentDescriptor> arguments,
        IValidatorProvider validatorProvider)
    {
        foreach (var argument in arguments)
        {
            if (validatorProvider.GetValidator(argument.ParameterType) is IValidator validator)
            {
                yield return new ValidatorDescriptor
                {
                    ArgumentIndex = argument.Index,
                    ArgumentType = argument.ParameterType,
                    ArgumentInstance = argument.Parameter,
                    Validator = validator
                };
            }
        }
    }
}

/// <summary>
/// Represents a filter that validates endpoints in a minimal API.
/// </summary>
/// <typeparam name="TArgument">The type of the argument to validate.</typeparam>
/// <param name="validatorProvider">The validator provider to use.</param>
public class MinimalValidationFilter<TArgument>(IValidatorProvider validatorProvider) :
    MinimalValidationFilter(validatorProvider)
    where TArgument : class, IValidationEnabled
{
    /// <summary>
    /// Validates the endpoint invocation.
    /// </summary>
    /// <param name="context">The context for the endpoint filter invocation.</param>
    /// <param name="next">The delegate to invoke the next filter in the pipeline.</param>
    /// <returns>A task that represents the asynchronous execution, containing 
    /// the result of the filter invocation.</returns>
    public override async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        return await InvokeAsyncCore<TArgument>(context, next)
            .ConfigureAwait(false);
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
    public required IValidationEnabled ArgumentInstance { get; init; }
    public required IValidator Validator { get; init; }
}