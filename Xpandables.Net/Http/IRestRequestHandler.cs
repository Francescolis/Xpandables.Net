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
using Microsoft.Extensions.Options;

using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Pipelines;
using Xpandables.Net.Http.Builders;

namespace Xpandables.Net.Http;

/// <summary>
/// Asynchronously builds an HTTP request message from a provided REST request. 
/// It can be canceled using a cancellation token.
/// </summary>
public interface IRestRequestHandler<TRestRequest>
    where TRestRequest : class, IRestRequest
{
    /// <summary>
    /// Asynchronously builds an HTTP request message based on the provided REST request.
    /// </summary>
    /// <param name="request">The input that defines the details of the HTTP request to be created.</param>
    /// <param name="cancellationToken">Used to signal the cancellation of the asynchronous operation if needed.</param>
    /// <returns>Returns a task that represents the asynchronous operation, containing the constructed HTTP request message.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the operation fails.</exception>
    Task<HttpRequestMessage> BuildRequestAsync(TRestRequest request, CancellationToken cancellationToken = default);
}

internal sealed class RestRequestHandler<TRestRequest> : Disposable, IRestRequestHandler<TRestRequest>
    where TRestRequest : class, IRestRequest
{
    private RestOptions _requestOptions;
    private readonly IDisposable? _disposable;
    private readonly HttpRequestMessage _message = new();
    private readonly IEnumerable<IRestRequestBuilder<TRestRequest>> _requestBuilders;
    public RestRequestHandler(
        IOptionsMonitor<RestOptions> options,
        IEnumerable<IRestRequestBuilder<TRestRequest>> requestBuilders)
    {
        _requestOptions = options.CurrentValue;
        _requestBuilders = requestBuilders;
        _disposable = options.OnChange(newOptions => _requestOptions = newOptions);
    }

    ///<inheritdoc/>
    public Task<HttpRequestMessage> BuildRequestAsync(TRestRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _RestAttribute attribute = _requestOptions.GetMapRestAttribute(request);

        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<HttpRequestMessage>(cancellationToken);

        if (!_requestBuilders.Any())
            throw new InvalidOperationException(
                $"No request builder found for the request type {request.GetType()}.");

        using RestRequestContext<TRestRequest> context = new()
        {
            Attribute = attribute,
            Message = _message,
            Request = request,
            SerializerOptions = _requestOptions.SerializerOptions
        };

        try
        {
            Task<ExecutionResult> result = _requestBuilders
                 .Reverse()
                 .Aggregate<IPipelineDecorator<RestRequestContext<TRestRequest>, ExecutionResult>,
                 RequestHandler<ExecutionResult>>(
                     Handler,
                     (next, builder) => () => builder.HandleAsync(
                         context,
                         next,
                         cancellationToken))();

            return Task.FromResult(context.Message);

            static Task<ExecutionResult> Handler() => Task.FromResult(ExecutionResults.Success());
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException
                and not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "An error occurred while building the request message.",
                exception);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _disposable?.Dispose();
            _message?.Dispose();
        }

        base.Dispose(disposing);
    }
}