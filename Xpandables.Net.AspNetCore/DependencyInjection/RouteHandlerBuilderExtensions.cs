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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

using Xpandables.Net.Http.Interfaces;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides extension methods for <see cref="IEndpointConventionBuilder"/> to add 
/// metadata for request and response types.
/// </summary>
public static class RouteHandlerBuilderExtensions
{
    /// <summary>
    /// Adds <see cref="Microsoft.AspNetCore.Http.Metadata.IAcceptsMetadata"/>
    /// to the <see cref="Endpoint.Metadata"/> for all endpoints produced by the 
    /// route builder.
    /// </summary>
    /// <param name="builder">The route builder.</param>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <returns>The route builder.</returns>
    /// <remarks>The request content type will be to 
    /// <see cref="HttpClientParameters.ContentType.Json"/>.</remarks>
    public static RouteHandlerBuilder Accepts<TRequest>(
        this RouteHandlerBuilder builder)
        where TRequest : notnull =>
        builder.Accepts<TRequest>(HttpClientParameters.ContentType.Json);

    /// <summary>
    /// Adds <see cref="Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata"/>
    /// to the <see cref="Endpoint.Metadata"/> for all endpoints produced by the
    /// route builder.
    /// </summary>
    /// <param name="builder">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static TBuilder Produces200OK<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.WithMetadata(
            new ProducesResponseTypeMetadata(
                StatusCodes.Status200OK,
                typeof(void),
                [HttpClientParameters.ContentType.Json]));

    /// <summary>
    /// Adds <see cref="Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata"/>
    /// to the <see cref="Endpoint.Metadata"/> for all endpoints produced by the
    /// route builder.
    /// </summary>
    /// <param name="builder">The route builder.</param>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <returns>The route builder.</returns>
    /// <remarks>The response content type will be to
    /// <see cref="HttpClientParameters.ContentType.Json"/>.</remarks>
    public static RouteHandlerBuilder Produces200OK<TResponse>(
        this RouteHandlerBuilder builder)
        where TResponse : notnull =>
        builder.Produces<TResponse>(
            StatusCodes.Status200OK,
            HttpClientParameters.ContentType.Json);

    /// <summary>
    /// Adds <see cref="Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata"/>
    /// to the <see cref="Endpoint.Metadata"/> for all endpoints produced by the
    /// route builder.
    /// </summary>
    /// <param name="builder">The route builder.</param>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <returns>The route builder.</returns>
    /// <remarks>The response content type will be to
    /// <see cref="HttpClientParameters.ContentType.Json"/>.</remarks>
    public static RouteHandlerBuilder Produces201Created<TResponse>(
        this RouteHandlerBuilder builder)
        where TResponse : notnull =>
        builder.Produces<TResponse>(
            StatusCodes.Status201Created,
            HttpClientParameters.ContentType.Json);

    /// <summary>
    /// Adds <see cref="Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata"/>
    /// to the <see cref="Endpoint.Metadata"/> for all endpoints produced by the
    /// route builder.
    /// </summary>
    /// <param name="builder">The route builder.</param>
    /// <returns>The route builder.</returns>
    /// <remarks>The response content type will be to
    /// <see cref="HttpClientParameters.ContentType.Json"/>.</remarks>
    public static TBuilder Produces400BadRequest<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.ProducesValidationProblem(
            StatusCodes.Status400BadRequest,
            HttpClientParameters.ContentType.JsonProblem);

    /// <summary>
    /// Adds <see cref="Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata"/>
    /// to the <see cref="Endpoint.Metadata"/> for all endpoints produced by the
    /// route builder.
    /// </summary>
    /// <param name="builder">The route builder.</param>
    /// <returns>The route builder.</returns>
    /// <remarks>The response content type will be to
    /// <see cref="HttpClientParameters.ContentType.Json"/>.</remarks>
    public static TBuilder Produces404NotFound<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.ProducesValidationProblem(
            StatusCodes.Status404NotFound,
            HttpClientParameters.ContentType.JsonProblem);

    /// <summary>
    /// Adds <see cref="Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata"/>
    /// to the <see cref="Endpoint.Metadata"/> for all endpoints produced by the
    /// route builder.
    /// </summary>
    /// <param name="builder">The route builder.</param>
    /// <returns>The route builder.</returns>
    /// <remarks>The response content type will be to
    /// <see cref="HttpClientParameters.ContentType.Json"/>.</remarks>
    public static TBuilder Produces409Conflict<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.ProducesValidationProblem(
            StatusCodes.Status409Conflict,
            HttpClientParameters.ContentType.JsonProblem);

    /// <summary>
    /// Adds <see cref="Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata"/>
    /// to the <see cref="Endpoint.Metadata"/> for all endpoints produced by the
    /// route builder.
    /// </summary>
    /// <param name="builder">The route builder.</param>
    /// <returns>The route builder.</returns>
    /// <remarks>The response content type will be to
    /// <see cref="HttpClientParameters.ContentType.Json"/>.</remarks>
    public static TBuilder Produces405MethodNotAllowed<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.ProducesValidationProblem(
            StatusCodes.Status405MethodNotAllowed,
            HttpClientParameters.ContentType.JsonProblem);

    /// <summary>
    /// Adds <see cref="Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata"/>
    /// to the <see cref="Endpoint.Metadata"/> for all endpoints produced by the
    /// route builder.
    /// </summary>
    /// <param name="builder">The route builder.</param>
    /// <returns>The route builder.</returns>
    /// <remarks>The response content type will be to
    /// <see cref="HttpClientParameters.ContentType.Json"/>.</remarks>
    public static TBuilder Produces401Unauthorized<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.ProducesValidationProblem(
            StatusCodes.Status401Unauthorized,
            HttpClientParameters.ContentType.JsonProblem);

    /// <summary>
    /// Adds <see cref="Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata"/>
    /// to the <see cref="Endpoint.Metadata"/> for all endpoints produced by the
    /// route builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the route builder.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <returns>The route builder.</returns>
    /// <remarks>The response content type will be to
    /// <see cref="HttpClientParameters.ContentType.Json"/>.</remarks>
    public static TBuilder Produces500InternalServerError<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.ProducesProblem(
            StatusCodes.Status500InternalServerError,
            HttpClientParameters.ContentType.JsonProblem);
}
