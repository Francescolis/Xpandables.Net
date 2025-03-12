
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

namespace Xpandables.Net.DataAnnotations;
/// <summary>
/// Defines a validator for endpoints.
/// </summary>
public interface IEndpointValidator
{
    /// <summary>
    /// Validates the endpoint asynchronously.
    /// </summary>
    /// <param name="context">The context of the endpoint filter invocation.</param>
    /// <param name="next">The next delegate to invoke.</param>
    /// <returns>A task that represents the asynchronous validation execution. 
    /// The task result contains the validation result.</returns>
    ValueTask<object?> ValidateAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next);
}
