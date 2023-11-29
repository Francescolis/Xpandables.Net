
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
using Microsoft.AspNetCore.Http;

namespace Xpandables.Net.Validators;

/// <summary>
/// Provides with validation for minimal route and controllers using <see cref="IEndpointFilter"/>.
/// </summary>
public sealed class OperationResultMinimalValidatorFilter : IEndpointFilter
{
    /// <summary>
    /// Specifies the predicate that a request type must match in order to be validated.
    /// </summary>
    internal static Predicate<Type> ValidatorPredicate { get; set; } = type => type.IsAssignableTo(typeof(IValidateDecorator));

    ///<inheritdoc/>
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        IEnumerable<MinimalValidationDescriptor> validationDescriptors =
            OperationResultMinimalValidatorExtensions.GetMinimalValidationDescriptors(
                context.Arguments,
                context.HttpContext.RequestServices);

        if (validationDescriptors.Any())
        {
            return await OperationResultMinimalValidatorExtensions.ValidateDescriptorsAsync(
                validationDescriptors,
                context,
                next)
                .ConfigureAwait(false);
        }

        return await next(context).ConfigureAwait(false);
    }
}
