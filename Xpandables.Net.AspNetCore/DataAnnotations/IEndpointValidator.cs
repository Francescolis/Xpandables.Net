
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
/// Validates an endpoint asynchronously and returns the validation result. 
/// The process involves the context of the invocation and a delegate.
/// </summary>
public interface IEndpointValidator
{
    /// <summary>
    /// Validates an asynchronous operation within an endpoint filter context.
    /// </summary>
    /// <param name="context">Provides the context for the current endpoint filter invocation.</param>
    /// <param name="next">Represents the next delegate in the endpoint filter pipeline to be invoked.</param>
    /// <returns>Returns a ValueTask that resolves to an object or null after validation.</returns>
    ValueTask<object?> ValidateAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next);
}
