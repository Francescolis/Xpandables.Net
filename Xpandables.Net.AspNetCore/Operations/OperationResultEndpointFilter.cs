
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
/// Intercepts the operation result and transforms it to a minimal response.
/// </summary>
/// <remarks>To be applied on many routes, please use <see langword="MapGroup"/>
/// with empty prefix (<see cref="string.Empty"/>).</remarks>
public sealed class OperationResultEndpointFilter : IEndpointFilter
{
    ///<inheritdoc/>
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        object? result = await next(context).ConfigureAwait(false);

        if (result is IOperationResult operationResult)
        {
            return operationResult.ToMinimalResult();
        }

        return result;
    }
}
