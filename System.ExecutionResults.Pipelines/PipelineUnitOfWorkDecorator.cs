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
using System.Data.Repositories;

namespace System.ExecutionResults.Pipelines;

/// <summary>
/// The PipelineUnitOfWorkDecorator class is a pipeline decorator that ensures
/// changes are saved to the database context via the <see cref="IUnitOfWork"/>
/// after processing the request and response in a pipeline execution.
/// </summary>
/// <typeparam name="TRequest">The type of the request object that must implement <see cref="IRequiresUnitOfWork"/>.</typeparam>
public sealed class PipelineUnitOfWorkDecorator<TRequest>(IUnitOfWork? unitOfWork = default) :
    IPipelineDecorator<TRequest>
    where TRequest : class, IRequest, IRequiresUnitOfWork
{
    /// <inheritdoc/>
    public async Task<OperationResult> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler nextHandler,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(nextHandler);

        try
        {
            OperationResult response = await nextHandler(cancellationToken).ConfigureAwait(false);

            return response;
        }
        finally
        {
            if (unitOfWork is not null)
            {
                await unitOfWork
                    .SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
