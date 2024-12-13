
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
using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Operations;
using Xpandables.Net.Operations.Controllers;
using Xpandables.Net.Operations.Executors;
using Xpandables.Net.Operations.Minimal;

using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace Xpandables.Net.DependencyInjection;
/// <summary>
/// Provides extension methods for adding execution result execution services 
/// to the service collection.
/// </summary>
public static class ServiceCollectionOperationExtensions
{
    /// <summary>
    /// Adds execution result execution services to the service collection 
    /// for minimal APIs.
    /// </summary>
    /// <param name="services">The service collection to add the services
    /// to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXExecutionResultMinimalApi(
        this IServiceCollection services) =>
        services.AddXExecutionResultExecutors(typeof(IExecutionResultExecutor).Assembly)
            .AddXExecutionResultJsonOptions()
            .AddXExecutionResultExecute()
            .AddXExecutionResultValidator()
            .AddXExecutionResultMiddleware()
            .AddXValidatorDefault();

    /// <summary>
    /// Adds execution result executors to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the executors to.</param>
    /// <param name="assemblies">The assemblies to scan for executors. 
    /// If none are provided, the calling assembly is used.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXExecutionResultExecutors(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        List<Type> executorTypes = [.. assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSealed
                && type.IsAssignableTo(typeof(IExecutionResultExecutor)))];

        foreach (Type executorType in executorTypes)
        {
            _ = services.AddScoped(typeof(IExecutionResultExecutor), executorType);
        }

        return services;
    }

    /// <summary>
    /// Adds a scoped service of the type specified in 
    /// <typeparamref name="TOperationResultExecute"/> 
    /// with an implementation type of 
    /// <typeparamref name="TOperationResultExecute"/> to the specified 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TOperationResultExecute">The type of the service to add. 
    /// This class must implement <see cref="IExecutionResultExecute"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add 
    /// the service to.</param>
    /// <returns>A reference to this instance after the execution has 
    /// completed.</returns>
    public static
        IServiceCollection AddXExecutionResultExecute<TOperationResultExecute>(
        this IServiceCollection services)
        where TOperationResultExecute : class, IExecutionResultExecute =>
        services.AddScoped<IExecutionResultExecute, TOperationResultExecute>();

    /// <summary>
    /// Adds a scoped service of the type <see cref="ExecutionResultExecute"/> 
    /// to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add 
    /// the service to.</param>
    /// <returns>A reference to this instance after the execution has 
    /// completed.</returns>
    public static IServiceCollection AddXExecutionResultExecute(
        this IServiceCollection services) =>
        services.AddXExecutionResultExecute<ExecutionResultExecute>();

    /// <summary>
    /// Adds a scoped service of the type specified in 
    /// <typeparamref name="TOperationResultValidator"/> 
    /// with an implementation type of 
    /// <typeparamref name="TOperationResultValidator"/> to the specified 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TOperationResultValidator">The type of the service to add. 
    /// This class must implement <see cref="IExecutionResultValidator"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add 
    /// the service to.</param>
    /// <returns>A reference to this instance after the execution has 
    /// completed.</returns>
    public static
        IServiceCollection AddXExecutionResultValidator<TOperationResultValidator>(
        this IServiceCollection services)
        where TOperationResultValidator : class, IExecutionResultValidator =>
        services.AddScoped<IExecutionResultValidator, TOperationResultValidator>();

    /// <summary>
    /// Adds a scoped service of the type <see cref="ExecutionResultValidator"/> 
    /// to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add 
    /// the service to.</param>
    /// <returns>A reference to this instance after the execution has 
    /// completed.</returns>
    public static IServiceCollection AddXExecutionResultValidator(
        this IServiceCollection services) =>
        services.AddXExecutionResultValidator<ExecutionResultValidator>();

    /// <summary>
    /// Adds a scoped service of the type <see cref="ExecutionResultMiddleware"/> 
    /// to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add 
    /// the service to.</param>
    /// <returns>A reference to this instance after the execution has 
    /// completed.</returns>
    public static IServiceCollection AddXExecutionResultMiddleware(
        this IServiceCollection services) =>
        services.AddScoped<ExecutionResultMiddleware>();

    /// <summary>
    /// Adds the <see cref="ExecutionResultMiddleware"/> to the application's 
    /// request pipeline.
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> to configure.</param>
    /// <returns>A reference to the <see cref="IApplicationBuilder"/> after the 
    /// execution has completed.</returns>
    public static IApplicationBuilder UseXExecutionResultMiddleware(
        this IApplicationBuilder builder) =>
        builder.UseMiddleware<ExecutionResultMiddleware>();

    /// <summary>
    /// Applies the execution result filter to the response of the target route(s),
    /// adding the <see cref="ExecutionResultFilter"/> to the endpoint 
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
    public static TBuilder WithXExecutionResultFilter<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.AddEndpointFilter<TBuilder, ExecutionResultFilter>();

    /// <summary>
    /// Applies the validation filter factory to the request of the target route(s),
    /// adding the <see cref="ExecutionResultValidationFilterFactory"/> to the endpoint 
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
    /// <para>The request must implement the <see cref="IApplyValidation"/>.
    /// You can implement the <see cref="IValidator{TArgument}"/> specific
    /// to your request, otherwise you must use the built in one : Register the
    /// generic validator using <see langword="AddXValidatorGenerics"/> method.</para>
    /// <para>You may register the <see cref="IProblemDetailsService"/> in 
    /// order to customize the response.</para></remarks>
    public static TBuilder WithXValidationFilterFactory<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.AddEndpointFilterFactory(
            ExecutionResultValidationFilterFactory.FilterFactory);

    /// <summary>
    /// Applies the validation filter to the request of the target route(s),
    /// adding the <see cref="ExecutionResultValidationFilter"/> to the endpoint 
    /// convention builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the endpoint convention builder.</typeparam>
    /// <param name="builder">The endpoint convention builder to configure.</param>
    /// <returns>A reference to the <typeparamref name="TBuilder"/> after the 
    /// execution has completed.</returns>
    /// <remarks>To be applied on many routes, please use 
    /// <see langword="MapGroup"/> with empty prefix 
    /// (<see cref="string.Empty"/>).
    /// <para>The request must implement the <see cref="IApplyValidation"/>.
    /// You can implement the <see cref="IValidator{TArgument}"/> specific
    /// to your request, otherwise you must use the built in one : Register the
    /// generic validator using <see langword="AddXValidatorGenerics"/> method.</para>
    /// <para>You can register the <see cref="IProblemDetailsService"/> in 
    /// order to customize the response.</para></remarks>
    public static TBuilder WithXValidationFilter<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.AddEndpointFilter(
            new ExecutionResultValidationFilter().InvokeAsync);

    /// <summary>
    /// Applies the execution result filter and validation filter to the 
    /// response of the target route(s),
    /// adding the <see cref="ExecutionResultFilter"/> and 
    /// <see cref="ExecutionResultValidationFilter"/> 
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
        builder.WithXExecutionResultFilter()
            .WithXValidationFilter();

    /// <summary>
    /// Adds the <see cref="ExecutionResultJsonOptions"/> to the 
    /// specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the 
    /// service to.</param>
    /// <returns>A reference to this instance after the execution has completed.</returns>
    public static IServiceCollection AddXExecutionResultJsonOptions(
        this IServiceCollection services) =>
        services.AddSingleton<IConfigureOptions<JsonOptions>,
            ExecutionResultJsonOptions>();

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
