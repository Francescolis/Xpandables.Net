
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

namespace Xpandables.Net.Operations.Minimals;
/// <summary>  
/// Represents a filter that processes the result of an endpoint invocation and 
/// converts it to a minimal result if it implements <see cref="IOperationResult"/>.  
/// </summary>  
public sealed class OperationResultFilter : IEndpointFilter
{
    /// <inheritdoc/>  
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        object? result = await next(context).ConfigureAwait(false);

        if (result is IOperationResult operationResult)
        {
            return operationResult.ToMinimalResult();
        }

        return result;
    }
}
