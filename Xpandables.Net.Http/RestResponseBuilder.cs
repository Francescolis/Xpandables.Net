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
using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.Http;

/// <summary>
/// Provides a builder that constructs REST responses by delegating to one of multiple response composers based on the
/// request context.
/// </summary>
/// <param name="serviceProvider">The service provider used to resolve the available response composers.</param>
/// <remarks>The builder selects the first composer that can handle the provided context. If no suitable composer
/// is found, an exception is thrown. This class is sealed and cannot be inherited.</remarks>
public sealed class RestResponseBuilder(IServiceProvider serviceProvider) : IRestResponseBuilder
{
    /// <inheritdoc />
    public async ValueTask<RestResponse> BuildResponseAsync(
        RestResponseContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var composers = serviceProvider.GetServices<IRestResponseComposer>();

        IRestResponseComposer composer =
            composers.FirstOrDefault(c => c.CanCompose(context))
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