
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xpandables.Net.Collections;
using Xpandables.Net.Controllers;
using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Executions;
using Xpandables.Net.Minimals;

using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace Xpandables.Net.DependencyInjection;
/// <summary>
/// Provides extension methods for adding execution result execution services 
/// to the service collection.
/// </summary>
public static class ServiceCollectionExecutionExtensions
{
    /// <summary>
    /// Adds execution result execution services to the service collection 
    /// for minimal APIs.
    /// </summary>
    /// <param name="services">The service collection to add the services
    /// to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXMinimalApi(
        this IServiceCollection services) =>
        services
            .AddXMinimalJsonOptions()
            .AddXEndpointProcessor()
            .AddXValidatorProvider()
            .AddXEndpointValidator()
            .AddXMinimalMiddleware()
            .AddXValidatorDefault();

    /// <summary>
    /// Adds a scoped service of the type specified in 
    /// <typeparamref name="TEndpointProcessor"/> 
    /// with an implementation type of 
    /// <see cref="IEndpointProcessor"/> to the specified 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TEndpointProcessor">The type of the service to add. 
    /// This class must implement <see cref="IEndpointProcessor"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add 
    /// the service to.</param>
    /// <returns>A reference to this instance after the execution has 
    /// completed.</returns>
    public static IServiceCollection AddXEndpointProcessor<TEndpointProcessor>(
        this IServiceCollection services)
        where TEndpointProcessor : class, IEndpointProcessor =>
        services.AddScoped<IEndpointProcessor, TEndpointProcessor>();

    /// <summary>
    /// Adds a scoped service of the type <see cref="EndpointProcessor"/> 
    /// to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add 
    /// the service to.</param>
    /// <returns>A reference to this instance after the execution has 
    /// completed.</returns>
    public static IServiceCollection AddXEndpointProcessor(
        this IServiceCollection services) =>
        services.AddXEndpointProcessor<EndpointProcessor>();

    /// <summary>
    /// Adds a scoped service of the type specified in 
    /// <typeparamref name="TEndpointValidator"/> 
    /// with an implementation type of 
    /// <see cref="IEndpointValidator"/> to the specified 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TEndpointValidator">The type of the service to add. 
    /// This class must implement <see cref="IEndpointValidator"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add 
    /// the service to.</param>
    /// <returns>A reference to this instance after the execution has 
    /// completed.</returns>
    public static IServiceCollection AddXEndpointValidator<TEndpointValidator>(
        this IServiceCollection services)
        where TEndpointValidator : class, IEndpointValidator =>
        services.AddScoped<IEndpointValidator, TEndpointValidator>();

    /// <summary>
    /// Adds a scoped service of the type <see cref="EndpointValidator"/> 
    /// to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add 
    /// the service to.</param>
    /// <returns>A reference to this instance after the execution has 
    /// completed.</returns>
    public static IServiceCollection AddXEndpointValidator(
        this IServiceCollection services) =>
        services.AddXEndpointValidator<EndpointValidator>();

    /// <summary>
    /// Adds a scoped service of the type <see cref="MinimalMiddleware"/> 
    /// to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add 
    /// the service to.</param>
    /// <returns>A reference to this instance after the execution has 
    /// completed.</returns>
    public static IServiceCollection AddXMinimalMiddleware(
        this IServiceCollection services) =>
        services.AddScoped<MinimalMiddleware>();

    /// <summary>
    /// Adds the <see cref="MinimalMiddleware"/> to the application's 
    /// request pipeline.
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> to configure.</param>
    /// <returns>A reference to the <see cref="IApplicationBuilder"/> after the 
    /// execution has completed.</returns>
    public static IApplicationBuilder UseXMinimalMiddleware(
        this IApplicationBuilder builder) =>
        builder.UseMiddleware<MinimalMiddleware>();

    /// <summary>
    /// Applies the execution result filter to the response of the target route(s),
    /// adding the <see cref="MinimalFilter"/> to the endpoint 
    /// convention builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the endpoint convention 
    /// builder.</typeparam>
    /// <param name="builder">The endpoint convention builder to configure.</param>
    /// <returns>A reference to the <typeparamref name="TBuilder"/> after the 
    /// execution has completed.</returns>
    /// <remarks>To be applied on many routes, please use 
    /// <see langword="MapGroup"/> with empty prefix 
    /// (<see cref="string.Empty"/>).
    /// <para>You can register the <see cref="IProblemDetailsService"/> in 
    /// order to customize the response.</para></remarks>
    public static TBuilder WithXMinimalFilter<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.AddEndpointFilter<TBuilder, MinimalFilter>();

    /// <summary>
    /// Applies the validation filter factory to the request of the target route(s),
    /// adding the <see cref="MinimalValidationFilterFactory"/> to the endpoint 
    /// convention builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the endpoint convention 
    /// builder.</typeparam>
    /// <param name="builder">The endpoint convention builder to configure.</param>
    /// <returns>A reference to the <typeparamref name="TBuilder"/> after the 
    /// execution has completed.</returns>
    /// <remarks>To be applied on many routes, please use 
    /// <see langword="MapGroup"/> with empty prefix 
    /// (<see cref="string.Empty"/>).
    /// <para>The request must implement the <see cref="IRequiresValidation"/>.
    /// You can implement the <see cref="IValidator{TArgument}"/> specific
    /// to your request, otherwise you must use the built in one : Register the
    /// generic validator using <see langword="AddXValidatorGenerics"/> method.</para>
    /// <para>You may register the <see cref="IProblemDetailsService"/> in 
    /// order to customize the response.</para></remarks>
    public static TBuilder WithXValidationFilterFactory<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.AddEndpointFilterFactory(
            MinimalValidationFilterFactory.FilterFactory);

    /// <summary>
    /// Applies the validation filter to the request of the target route(s),
    /// adding the <see cref="MinimalValidationFilter"/> to the endpoint 
    /// convention builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the endpoint convention builder.</typeparam>
    /// <param name="builder">The endpoint convention builder to configure.</param>
    /// <returns>A reference to the <typeparamref name="TBuilder"/> after the 
    /// execution has completed.</returns>
    /// <remarks>To be applied on many routes, please use 
    /// <see langword="MapGroup"/> with empty prefix 
    /// (<see cref="string.Empty"/>).
    /// <para>The request must implement the <see cref="IRequiresValidation"/>.
    /// You can implement the <see cref="IValidator{TArgument}"/> specific
    /// to your request, otherwise you must use the built in one : Register the
    /// generic validator using <see langword="AddXValidatorGenerics"/> method.</para>
    /// <para>You can register the <see cref="IProblemDetailsService"/> in 
    /// order to customize the response.</para></remarks>
    public static TBuilder WithXValidationFilter<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.AddEndpointFilter(
            new MinimalValidationFilter().InvokeAsync);

    /// <summary>
    /// Applies the execution result filter and validation filter to the 
    /// response of the target route(s),
    /// adding the <see cref="MinimalFilter"/> and 
    /// <see cref="MinimalValidationFilter"/> 
    /// to the endpoint convention builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the endpoint convention 
    /// builder.</typeparam>
    /// <param name="builder">The endpoint convention builder to configure.</param>
    /// <returns>A reference to the <typeparamref name="TBuilder"/> after 
    /// the execution has completed.</returns>
    /// <remarks>To be applied on many routes, please use <see langword="MapGroup"/> with empty prefix 
    /// (<see cref="string.Empty"/>).
    /// <para>You can register the <see cref="IProblemDetailsService"/> in order
    /// to customize the response.</para></remarks>
    public static TBuilder WithXMinimalApi<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.WithXMinimalFilter()
            .WithXValidationFilter();

    /// <summary>
    /// Adds the <see cref="MinimalJsonOptions"/> to the 
    /// specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the 
    /// service to.</param>
    /// <returns>A reference to this instance after the execution has completed.</returns>
    /// <remarks>The default <see cref="MaterializedPagedDataJsonConverterFactory"/> is configured to use the
    /// body serializer options.</remarks>
    public static IServiceCollection AddXMinimalJsonOptions(
        this IServiceCollection services) =>
        services.AddSingleton<IConfigureOptions<JsonOptions>,
            MinimalJsonOptions>();

    /// <summary>
    /// Adds the <see cref="ControllerMvcOptions"/> to the 
    /// specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the 
    /// service to.</param>
    /// <returns>A reference to this instance after the execution has completed.</returns>
    public static IServiceCollection AddXControllerMvcOptions(
        this IServiceCollection services) =>
        services.AddSingleton<IConfigureOptions<MvcOptions>,
            ControllerMvcOptions>();
}
