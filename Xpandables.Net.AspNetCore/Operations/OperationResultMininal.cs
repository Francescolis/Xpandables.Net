
/************************************************************************************************************
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
************************************************************************************************************/
using Microsoft.AspNetCore.Http;

namespace Xpandables.Net.Operations;

/// <summary>
/// Custom implementation of <see cref="IResult"/> interface for <see cref="IOperationResult"/>.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="OperationResultMinimal"/> that holds the specified operation result.
/// </remarks>
/// <param name="operationResult">The operation result to act on.</param>
/// <exception cref="ArgumentNullException">The <paramref name="operationResult"/> is null.</exception>
public sealed class OperationResultMinimal(IOperationResult operationResult) : IResult
{
    ///<inheritdoc/>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.StatusCode = (int)operationResult.StatusCode;

        if (operationResult.StatusCode == System.Net.HttpStatusCode.Created)
        {
            await httpContext.ResultCreatedAsync(operationResult).ConfigureAwait(false);
            return;
        }

        if (operationResult.Result.IsNotEmpty && operationResult.Result.Value is BinaryEntry { Content: not null } file)
        {
            await httpContext.WriteFileBodyAsync(file).ConfigureAwait(false);
            return;
        }

        if (IOperationResult.IsSuccessStatusCode(operationResult.StatusCode))
        {
            await httpContext.WriteBodyIfAvailableAsync(operationResult).ConfigureAwait(false);
            return;
        }

        if (IOperationResult.IsFailureStatusCode(operationResult.StatusCode))
        {
            var result = operationResult.GetValidationProblemDetails(httpContext);

            await result.ExecuteAsync(httpContext).ConfigureAwait(false);
        }
    }
}
