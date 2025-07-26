﻿
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
using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Executions.Pipelines;

/// <summary>
/// The PipelineUnitOfWorkEventDecorator class is a pipeline decorator that ensures
/// aggregate events changes are saved to the database context via the <see cref="IUnitOfWorkEvent"/>
/// after processing the request and response in a pipeline execution.
/// </summary>
/// <typeparam name="TRequest">The type of the request object that must implement <see cref="IRequiresEventStorage"/>.</typeparam>
public sealed class PipelineUnitOfWorkEventDecorator<TRequest>(IUnitOfWorkEvent unitOfWork) :
    IPipelineDecorator<TRequest>
    where TRequest : class, IRequest, IRequiresEventStorage
{
    /// <inheritdoc/>
    public async Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler next,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ExecutionResult response = await next(cancellationToken).ConfigureAwait(false);

            return response;
        }
        finally
        {
            await unitOfWork
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
