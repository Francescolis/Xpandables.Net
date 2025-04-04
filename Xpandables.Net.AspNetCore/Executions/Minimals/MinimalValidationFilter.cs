﻿
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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.DataAnnotations;

namespace Xpandables.Net.Executions.Minimals;

/// <summary>
/// Represents a filter that validates endpoints in a minimal API.
/// </summary>
public sealed class MinimalValidationFilter : IEndpointFilter
{
    /// <summary>
    /// Invokes the filter asynchronously.
    /// </summary>
    /// <param name="context">The context for the endpoint filter invocation.</param>
    /// <param name="next">The delegate to invoke the next filter in the pipeline.</param>
    /// <returns>A task that represents the asynchronous execution, containing 
    /// the result of the filter invocation.</returns>
    public ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        IEndpointValidator validator = context
            .HttpContext
            .RequestServices
            .GetRequiredService<IEndpointValidator>();

        return validator.ValidateAsync(context, next);
    }
}
