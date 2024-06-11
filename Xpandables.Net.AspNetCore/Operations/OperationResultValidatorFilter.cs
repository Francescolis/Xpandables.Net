
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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Primitives;
using Xpandables.Net.Validators;

namespace Xpandables.Net.Operations;

/// <summary>
/// Provides with validation for minimal route and controllers using 
/// <see cref="IEndpointFilter"/>.
/// </summary>
public sealed class OperationResultValidatorFilter : IEndpointFilter
{
    /// <summary>
    /// Specifies the predicate that a request type must match in order to be validated.
    /// </summary>
    internal static Predicate<Type> ValidatorPredicate { get; set; }
        = type => type.IsAssignableTo(typeof(IValidateDecorator));

    ///<inheritdoc/>
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        IOperationResultRequestValidator requestValidator = context
            .HttpContext
            .RequestServices
            .GetRequiredService<IOperationResultRequestValidator>();

        return await requestValidator
            .ValidateAsync(context, next)
            .ConfigureAwait(false);
    }
}


/// <summary>
/// A Helper class used with minimal Api to validate request on endpoint 
/// using <see cref="IValidator{TArgument}"/>.
/// </summary>
/// <typeparam name="TBindingRequest">The type of the request parameter
/// .</typeparam>
public sealed class OperationResultValidatorFilter<TBindingRequest> :
    IEndpointFilter
    where TBindingRequest : notnull
{
    ///<inheritdoc/>
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        TBindingRequest? request = context.GetArgument<TBindingRequest>(0);

        if (context.HttpContext.RequestServices
            .GetService<IValidator<TBindingRequest>>() is { } validator)
        {
            IOperationResult operation = await validator
                .ValidateAsync(request)
                .ConfigureAwait(false);

            if (operation.IsFailure)
            {
                return operation.ToMinimalResult();
            }
        }

        return await next
            .Invoke(context)
            .ConfigureAwait(false);
    }
}