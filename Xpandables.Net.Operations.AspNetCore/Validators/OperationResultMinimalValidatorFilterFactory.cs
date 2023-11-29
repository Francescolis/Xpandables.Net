
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
/// Applies the validator filter factory to the target route(s).
/// </summary>
/// <remarks>Inspired from https://benfoster.io/blog/minimal-api-validation-endpoint-filters/</remarks>
public static class OperationResultMinimalValidatorFilterFactory
{
    /// <summary>
    /// The delegate to be used to apply validation.
    /// </summary>
    /// <param name="context">The <see cref="EndpointFilterInvocationContext"/> associated with the current request/response.</param>
    /// <param name="next">The next filter in the pipeline.</param>
    /// <returns>An awaitable result of calling the handler and apply
    /// any modifications made by filters in the pipeline.</returns>
    public static EndpointFilterDelegate MinimalFilterFactory(EndpointFilterFactoryContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var arguments = context.MethodInfo.GetParameters().ToList();

        IEnumerable<MinimalValidationDescriptor> validationDescriptors
            = OperationResultMinimalValidatorExtensions.GetMinimalValidationDescriptors(
                arguments,
                context.ApplicationServices);

        if (validationDescriptors.Any())
        {
            return invocationContext => OperationResultMinimalValidatorExtensions.ValidateDescriptorsAsync(
                validationDescriptors,
                invocationContext,
                next);
        }

        // pass-thru
        return invocationContext => next(invocationContext);
    }
}
