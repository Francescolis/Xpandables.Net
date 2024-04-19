
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

namespace Xpandables.Net.Operations;

/// <summary>
/// Provides a contract to validate the request for Asp.Net.
/// </summary>
/// <remarks>This interface is used by the 
/// <see cref="OperationResultValidatorFilter"/>
/// and <see cref="OperationResultValidatorFilterFactory"/>.</remarks>
public interface IOperationResultRequestValidator
{
    /// <summary>
    /// Validates the request found in the context.
    /// </summary>
    /// <param name="context">The HTTP context to act on.</param>
    /// <param name="next">The next delegate to execute.</param>
    /// <returns>An awaitable result of calling the handler and apply any 
    /// modifications made by filters in the pipeline.</returns>
    /// <remarks>If the validation fails, returns the result and bypass the 
    /// next delegate.</remarks>
    ValueTask<object?> ValidateAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next);
}
