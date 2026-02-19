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
using System.Data;
using System.Results.Requests;

namespace System.Results.Pipelines;

/// <summary>
/// Provides a pipeline decorator that manages ADO.NET transactions for requests requiring data unit of work.
/// </summary>
/// <remarks>
/// <para>
/// This decorator wraps request execution within an ADO.NET transaction, ensuring that all database
/// operations performed during the request are either committed on success or rolled back on failure.
/// </para>
/// <para>
/// Unlike the EF Core <see cref="PipelineEntityUnitOfWorkDecorator{TRequest}"/> which uses SaveChangesAsync,
/// this decorator explicitly manages transactions since ADO.NET operations execute immediately.
/// </para>
/// </remarks>
/// <typeparam name="TRequest">The type of the request object that must implement <see cref="IDataRequiresUnitOfWork"/>.</typeparam>
/// <param name="unitOfWork">The ADO.NET unit of work for transaction management.</param>
public sealed class PipelineDataUnitOfWorkDecorator<TRequest>(IDataUnitOfWork? unitOfWork = default) :
    IPipelineDecorator<TRequest>
    where TRequest : class, IRequest, IDataRequiresUnitOfWork
{
    /// <inheritdoc/>
    public async Task<Result> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler nextHandler,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(nextHandler);

        if (unitOfWork is null)
        {
            return await nextHandler(cancellationToken).ConfigureAwait(false);
        }

		// Begin transaction
		IDataTransaction transaction = await unitOfWork
            .BeginTransactionAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        await using (transaction.ConfigureAwait(false))
        {
            try
            {
                Result response = await nextHandler(cancellationToken).ConfigureAwait(false);

                if (response.IsSuccess)
                {
                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                }

                return response;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }
    }
}
