
/************************************************************************************************************
 * Copyright (C) 2022 Francis-Black EWANE
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
************************************************************************************************************/
using System.ComponentModel.DataAnnotations;
using System.Reflection;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Extensions;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Validators;
readonly record struct MinimalValidationDescriptor(int ArgumentIndex, Type ArgumentType, IValidator Validator);
readonly record struct ArgumentObject(int Index, object? Parameter, Type? ParameterType);
readonly record struct ArgumentInfo(int Index, ParameterInfo Parameter);

internal static class OperationResultMinimalValidatorExtensions
{
    internal static IEnumerable<MinimalValidationDescriptor> GetMinimalValidationDescriptors(
        IList<object?> arguments,
        IServiceProvider service)
    {
        var argumentList = arguments.Select((p, i) => new ArgumentObject(i, p, p?.GetType()));
#pragma warning disable IDE0039 // Use local function
        Func<ArgumentObject, bool> predicate = arg
            => arg.ParameterType != null
                && (arg.ParameterType.IsClass || (arg.ParameterType.IsValueType && !arg.ParameterType.IsEnum))
                && OperationResultMinimalValidatorFilter.ValidatorPredicate(arg.ParameterType);
#pragma warning restore IDE0039 // Use local function

        foreach (var item in argumentList.Where(predicate))
        {
            if (!typeof(IValidator<>).TryMakeGenericType(out var validatorType, out _, item.ParameterType!))
                continue;

            var validators = service.GetServices(validatorType).OfType<IValidator>();
            foreach (var validator in validators)
                yield return new MinimalValidationDescriptor(item.Index, item.ParameterType!, validator);
        }
    }

    internal static IEnumerable<MinimalValidationDescriptor> GetMinimalValidationDescriptors(
        IList<ParameterInfo> arguments,
        IServiceProvider service)
    {
        var argumentList = arguments.Select((p, i) => new ArgumentInfo(i, p));
#pragma warning disable IDE0039 // Use local function
        Func<ArgumentInfo, bool> predicate = arg
            => (arg.Parameter.ParameterType.IsClass || (arg.Parameter.ParameterType.IsValueType
                && !arg.Parameter.ParameterType.IsEnum))
                && OperationResultMinimalValidatorFilter.ValidatorPredicate(arg.Parameter.ParameterType);
#pragma warning restore IDE0039 // Use local function

        foreach (var item in argumentList.Where(predicate))
        {
            if (!typeof(IValidator<>).TryMakeGenericType(out var validatorType, out _, item.Parameter.ParameterType))
                continue;

            var validators = service.GetServices(validatorType).OfType<IValidator>();
            foreach (var validator in validators)
                yield return new MinimalValidationDescriptor(item.Index, item.Parameter.ParameterType, validator);

        }
    }

    internal static async ValueTask<object?> ValidateDescriptorsAsync(
         IEnumerable<MinimalValidationDescriptor> validatorDescriptions,
         EndpointFilterInvocationContext context,
         EndpointFilterDelegate next)
    {
        IOperationResult.IFailureBuilder operationResultBuilder = OperationResults.BadRequest();

        foreach (var description in validatorDescriptions)
        {
            if (context.Arguments[description.ArgumentIndex] is not { } argument)
                continue;

            try
            {
                OperationResult operation = await description.Validator
                    .ValidateAsync(argument)
                    .ConfigureAwait(false);

                if (operation.IsFailure)
                    operationResultBuilder
                        .WithErrors(operation.Errors)
                        .WithHeaders(operation.Headers);
            }
            catch (ValidationException exception)
            {
                var operationException = exception.ToOperationResult();
                operationResultBuilder
                    .WithErrors(operationException.Errors)
                    .WithHeaders(operationException.Headers);
            }
        }

        var result = operationResultBuilder.Build();
        if (result.Errors.Any())
            return result.ToMinimalResult();

        return await next(context).ConfigureAwait(false);
    }
}