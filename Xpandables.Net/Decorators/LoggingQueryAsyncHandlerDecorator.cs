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
using Microsoft.Extensions.Logging;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Operations;
using Xpandables.Net.Responsibilities;

namespace Xpandables.Net.Decorators;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class LoggingQueryAsyncHandlerDecorator<TQuery, TResult> :
    IAsyncPipelineDecorator<TQuery, TResult>
    where TQuery : class, IQueryAsync<TResult>
{
    private readonly ILogger<LoggingQueryAsyncHandlerDecorator<TQuery, TResult>> _logger;

#pragma warning disable IDE0290 // Use primary constructor
    public LoggingQueryAsyncHandlerDecorator(
        ILogger<LoggingQueryAsyncHandlerDecorator<TQuery, TResult>> logger) =>
        _logger = logger;

    public IAsyncEnumerable<TResult> HandleAsync(
        TQuery query,
        RequestAsyncHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling {RequestName} with request: " +
            "{@Request}", typeof(TQuery).Name, query);

        try
        {
            IAsyncEnumerable<TResult> response = next();
            _logger.LogInformation("Handled {RequestName} with response: " +
                "{@Response}", typeof(TQuery).Name, response);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling {RequestName} with request: " +
                "{@Request}", typeof(TQuery).Name, query);
            throw;
        }
    }
}

public sealed class ValidationQueryAsyncHandlerDecorator<TQuery, TResult>(
    ICompositeValidator<TQuery> validators) :
    IAsyncPipelineDecorator<TQuery, TResult>
    where TQuery : class, IQueryAsync<TResult>, IUseValidation
{
    public IAsyncEnumerable<TResult> HandleAsync(
        TQuery query,
        RequestAsyncHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        IOperationResult result = validators.Validate(query);

        if (!result.IsSuccessStatusCode)
        {
            throw new OperationResultException(result);
        }

        return next();
    }
}