/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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
using System.Results.Pipelines;
using System.Results.Requests;

using Microsoft.Extensions.DependencyInjection;

namespace System.Results.Tasks;

/// <summary>
/// Provides a sealed implementation of the IMediator interface that dispatches requests to their corresponding pipeline
/// handlers using dependency injection.
/// </summary>
/// <remarks>
/// <para>This mediator is a pure dispatcher — it resolves the appropriate <see cref="IPipelineRequestHandler{TRequest}"/>
/// from the service provider and delegates execution entirely to the pipeline. Exception handling, validation, and
/// cross-cutting concerns must be handled by pipeline decorators (e.g., <c>PipelineExceptionDecorator</c>).</para>
/// <para><strong>Thread safety:</strong> This class is thread-safe. Each call to <see cref="SendAsync{TRequest}"/>
/// or <see cref="SendAsync{TRequest, TResponse}"/> resolves a fresh <see cref="IPipelineRequestHandler{TRequest}"/>
/// from the service provider. The resolved handler's <see cref="PipelineRequestHandler{TRequest}"/> pre-builds and
/// caches the decorator delegate chain at construction time (in the constructor), so no per-request pipeline assembly
/// occurs. The handler is safe for concurrent use once constructed. Scoped handlers are resolved from the current
/// scope; singleton handlers share a single pre-built pipeline across all requests. Ensure that all decorators and
/// the final handler are themselves thread-safe if registered as singletons.</para>
/// <para>Ensure that <c>PipelineExceptionDecorator</c> is registered in the pipeline to handle
/// unhandled exceptions. Without it, exceptions will propagate to the caller.</para>
/// </remarks>
/// <param name="provider">The service provider used to resolve pipeline request handlers for incoming requests.</param>
public sealed class Mediator(IServiceProvider provider) : IMediator
{
    /// <inheritdoc />
    public Task<Result> SendAsync<TRequest>(
        TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class, IRequest
    {
        ArgumentNullException.ThrowIfNull(request);

        IPipelineRequestHandler<TRequest> handler =
            provider.GetRequiredService<IPipelineRequestHandler<TRequest>>();

        return handler.HandleAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<TResponse>> SendAsync<TRequest, TResponse>(
        TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class, IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(request);

        IPipelineRequestHandler<TRequest> handler =
            provider.GetRequiredService<IPipelineRequestHandler<TRequest>>();

        Result result = await handler.HandleAsync(request, cancellationToken).ConfigureAwait(false);
        return result;
    }
}
