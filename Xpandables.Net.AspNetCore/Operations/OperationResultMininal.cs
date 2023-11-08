
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
    private readonly IOperationResult _operationResult = operationResult
        ?? throw new ArgumentNullException(nameof(operationResult));

    ///<inheritdoc/>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.StatusCode = (int)_operationResult.StatusCode;

        httpContext.AddLocationUrlIfAvailable(_operationResult);
        httpContext.AddHeadersIfAvailable(_operationResult);
        await httpContext.AddHeaderIfUnauthorized(_operationResult).ConfigureAwait(false);

        if (_operationResult.StatusCode == System.Net.HttpStatusCode.Created)
        {
            await httpContext.ResultCreatedAsync(_operationResult).ConfigureAwait(false);
            return;
        }

        if (_operationResult.Result.IsNotEmpty && _operationResult.Result.Value is BinaryEntry { Content: not null } file)
        {
            await httpContext.WriteFileBodyAsync(file).ConfigureAwait(false);
            return;
        }

        if (IOperationResult.IsSuccessStatusCode(_operationResult.StatusCode))
        {
            await httpContext.WriteBodyIfAvailableAsync(_operationResult).ConfigureAwait(false);
            return;
        }

        if (IOperationResult.IsFailureStatusCode(_operationResult.StatusCode))
        {
            var result = _operationResult.GetValidationProblemDetails(httpContext);

            await result.ExecuteAsync(httpContext).ConfigureAwait(false);
        }
    }
}
