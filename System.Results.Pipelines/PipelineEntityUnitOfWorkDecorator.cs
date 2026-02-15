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
using System.Entities;
using System.Results.Requests;

namespace System.Results.Pipelines;

/// <summary>
/// The PipelineEntityUnitOfWorkDecorator class is a pipeline decorator that ensures
/// changes are saved to the database context via the <see cref="IEntityUnitOfWork"/>
/// after processing the request and response in a pipeline execution.
/// </summary>
/// <typeparam name="TRequest">The type of the request object that must implement <see cref="IEntityRequiresUnitOfWork"/>.</typeparam>
public sealed class PipelineEntityUnitOfWorkDecorator<TRequest>(IEntityUnitOfWork? unitOfWork = default) :
    IPipelineDecorator<TRequest>
    where TRequest : class, IRequest, IEntityRequiresUnitOfWork
{
    /// <inheritdoc/>
    public async Task<Result> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler nextHandler,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(nextHandler);

        Result response = await nextHandler(cancellationToken).ConfigureAwait(false);

        if (response.IsSuccess && unitOfWork is not null)
        {
            await unitOfWork
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return response;
    }
}
