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
using Microsoft.AspNetCore.Http;

namespace Xpandables.Net.DataAnnotations;

/// <summary>
/// Defines a contract for validating the result of an endpoint execution within an endpoint filter pipeline.
/// </summary>
/// <remarks>Implementations of this interface can be used to inspect or modify the result of an endpoint
/// operation before it is returned to the caller. This is typically used in scenarios where additional validation,
/// transformation, or authorization logic is required as part of the endpoint filter pipeline.</remarks>
public interface IExecutionResultEndpointValidator
{
    /// <summary>
    /// Validates an asynchronous operation within an endpoint filter context.
    /// </summary>
    /// <param name="context">Provides the context for the current endpoint filter invocation.</param>
    /// <param name="nextDelegate">Represents the next delegate in the endpoint filter pipeline to be invoked.</param>
    /// <returns>Returns a ValueTask that resolves to an object or null after validation.</returns>
    ValueTask<object?> ValidateAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate nextDelegate);
}
