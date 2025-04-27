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

using Xpandables.Net.Text;

namespace Xpandables.Net.Executions.Rests;

/// <summary>
/// Asynchronously builds a response from an HTTP response message.
/// It returns a task containing the constructed response.
/// </summary>
/// <typeparam name="TRestRequest"> The type of the REST request.</typeparam>
public interface IRestResponseBuilder<in TRestRequest>
    where TRestRequest : class, IRestRequest
{
    /// <summary>
    /// Asynchronously builds a response based on the provided HTTP response message.
    /// </summary>
    /// <param name="request"> The REST request object used to create the response.</param>
    /// <param name="response">The HTTP response message used to create the response object.</param>
    /// <param name="cancellationToken">Used to signal the cancellation of the asynchronous operation.</param>
    /// <returns>Returns a task that represents the asynchronous operation, containing the constructed response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="response" /> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the operation fails.</exception>
    Task<RestResponse> BuildResponseAsync(TRestRequest request, HttpResponseMessage response,
        CancellationToken cancellationToken = default);
}

internal sealed class RestResponseBuilder<TRestRequest>(IEnumerable<IRestResponseComposer<TRestRequest>> composers) :
    IRestResponseBuilder<TRestRequest>
    where TRestRequest : class, IRestRequest
{
    private readonly IEnumerable<IRestResponseComposer<TRestRequest>> _composers = composers;

    /// <inheritdoc />
    public async Task<RestResponse> BuildResponseAsync(
        TRestRequest request, HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        RestResponseContext<TRestRequest> context = new()
        {
            Request = request, Message = response, SerializerOptions = DefaultSerializerOptions.Defaults
        };

        IRestResponseComposer<TRestRequest>? composer =
            _composers.FirstOrDefault(c => c.CanCompose(context))
            ?? throw new InvalidOperationException(
                $"{nameof(BuildResponseAsync)}: No composer found for the provided context.");

        try
        {
            return await composer
                .ComposeAsync(context, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException
                      and not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "The response builder failed to build the response.",
                exception);
        }
    }
}