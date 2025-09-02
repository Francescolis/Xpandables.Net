
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

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Executions;

using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace Xpandables.Net.DependencyInjection;
/// <summary>
/// Provides extension methods for adding execution result execution services 
/// to the service collection.
/// </summary>
public static class ServiceCollectionExecutionExtensions
{
    /// <summary>
    /// Configures and registers the necessary services for the Minimal API framework.
    /// </summary>
    /// <remarks>This method registers a set of services and middleware required for the Minimal API
    /// framework,  including JSON options, validation providers, endpoint validators, and result execution strategies.
    /// It is intended to be used during application startup to configure the dependency injection container.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the Minimal API services will be added.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance, allowing for method chaining.</returns>
    public static IServiceCollection AddXMinimalApi(
        this IServiceCollection services) =>
        services
            .AddXMinimalJsonOptions()
            .AddXValidatorProvider()
            .AddXExecutionResultEndpointValidator()
            .AddXMinimalMiddleware()
            .AddXValidatorDefault()
            .AddXExecutionResultExecutor<FailureExecutionResultExecutor>()
            .AddXExecutionResultExecutor<CreatedExecutionResultExecutor>()
            .AddXExecutionResultExecutor<StreamExecutionResultExecutor>()
            .AddXExecutionResultExecutor<SuccessExecutionResultExecutor>()
            .AddXExecutionResultExecutor<AsyncPagedExecutionResultExecutor>();

    /// <summary>
    /// Registers a scoped implementation of the <see cref="IExecutionResultExecutor"/> interface in the dependency
    /// injection container.
    /// </summary>
    /// <remarks>This method is used to configure dependency injection for a specific implementation of <see
    /// cref="IExecutionResultExecutor"/>. The implementation type must be a class.</remarks>
    /// <typeparam name="TExecutionResultExecutor">The concrete type that implements <see cref="IExecutionResultExecutor"/> to be registered.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXExecutionResultExecutor<TExecutionResultExecutor>(
        this IServiceCollection services)
        where TExecutionResultExecutor : class, IExecutionResultExecutor =>
        services.AddScoped<IExecutionResultExecutor, TExecutionResultExecutor>();

    /// <summary>
    /// Adds a scoped service of the type specified in 
    /// <typeparamref name="TEndpointValidator"/> 
    /// with an implementation type of 
    /// <see cref="IExecutionResultEndpointValidator"/> to the specified 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TEndpointValidator">The type of the service to add. 
    /// This class must implement <see cref="IExecutionResultEndpointValidator"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add 
    /// the service to.</param>
    /// <returns>A reference to this instance after the execution has 
    /// completed.</returns>
    public static IServiceCollection AddXExecutionResultEndpointValidator<TEndpointValidator>(
        this IServiceCollection services)
        where TEndpointValidator : class, IExecutionResultEndpointValidator =>
        services.AddScoped<IExecutionResultEndpointValidator, TEndpointValidator>();

    /// <summary>
    /// Adds a scoped service of the type <see cref="ExecutionResultEndpointValidator"/> 
    /// to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add 
    /// the service to.</param>
    /// <returns>A reference to this instance after the execution has 
    /// completed.</returns>
    public static IServiceCollection AddXExecutionResultEndpointValidator(
        this IServiceCollection services) =>
        services.AddXExecutionResultEndpointValidator<ExecutionResultEndpointValidator>();

    /// <summary>
    /// Adds a scoped service of the type <see cref="ExecutionResultMinimalMiddleware"/> 
    /// to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add 
    /// the service to.</param>
    /// <returns>A reference to this instance after the execution has 
    /// completed.</returns>
    public static IServiceCollection AddXExecutionResultMinimalMiddleware(
        this IServiceCollection services) =>
        services.AddScoped<ExecutionResultMinimalMiddleware>();

    /// <summary>
    /// Adds the <see cref="ExecutionResultMinimalMiddleware"/> to the application's 
    /// request pipeline.
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> to configure.</param>
    /// <returns>A reference to the <see cref="IApplicationBuilder"/> after the 
    /// execution has completed.</returns>
    public static IApplicationBuilder UseXExecutionResultMinimalMiddleware(
        this IApplicationBuilder builder) =>
        builder.UseMiddleware<ExecutionResultMinimalMiddleware>();

    /// <summary>
    /// Applies the execution result filter to the response of the target route(s),
    /// adding the <see cref="ExecutionResultMinimalFilter"/> to the endpoint 
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
    public static TBuilder WithXExecutionResultMinimalFilter<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.AddEndpointFilter<TBuilder, ExecutionResultMinimalFilter>();

    /// <summary>
    /// Applies the validation filter factory to the request of the target route(s),
    /// adding the <see cref="ExecutionResultEndpointValidationFilterFactory"/> to the endpoint 
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
    public static TBuilder WithXExecutionResultValidationFilterFactory<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.AddEndpointFilterFactory(
            ExecutionResultEndpointValidationFilterFactory.FilterFactory);

    /// <summary>
    /// Applies the validation filter to the request of the target route(s),
    /// adding the <see cref="ExecutionResultEndpointValidationFilter"/> to the endpoint 
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
    public static TBuilder WithXExecutionResultValidationFilter<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.AddEndpointFilter(
            new ExecutionResultEndpointValidationFilter().InvokeAsync);

    /// <summary>
    /// Applies the execution result filter and validation filter to the 
    /// response of the target route(s),
    /// adding the <see cref="ExecutionResultMinimalFilter"/> and 
    /// <see cref="ExecutionResultEndpointValidationFilter"/> 
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
    public static TBuilder WithXExecutionResultMinimalApi<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.WithXExecutionResultMinimalFilter()
            .WithXExecutionResultValidationFilter();

    /// <summary>
    /// Adds the <see cref="ExecutionResultMinimalJsonOptions"/> to the 
    /// specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the 
    /// service to.</param>
    /// <returns>A reference to this instance after the execution has completed.</returns>
    public static IServiceCollection AddXExecutionResultMinimalJsonOptions(
        this IServiceCollection services) =>
        services.AddSingleton<IConfigureOptions<JsonOptions>,
            ExecutionResultMinimalJsonOptions>();

    /// <summary>
    /// Adds the <see cref="ExecutionResultControllerMvcOptions"/> to the 
    /// specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the 
    /// service to.</param>
    /// <returns>A reference to this instance after the execution has completed.</returns>
    public static IServiceCollection AddXExecutionResultControllerMvcOptions(
        this IServiceCollection services) =>
        services.AddSingleton<IConfigureOptions<MvcOptions>,
            ExecutionResultControllerMvcOptions>();
}
