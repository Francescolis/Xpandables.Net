
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
/// Provides extension methods for adding operation result execution services 
/// to the service collection.
/// </summary>
public static class ServiceCollectionOperationExtensions
{
    /// <summary>
    /// Adds operation result execution services to the service collection 
    /// for minimal APIs.
    /// </summary>
    /// <param name="services">The service collection to add the services
    /// to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXOperationResultMinimalApi(
        this IServiceCollection services) =>
        services.AddXOperationResultExecutors(typeof(IOperationResultExecutor).Assembly)
            .AddXOperationResultJsonOptions()
            .AddXOperationResultExecute()
            .AddXOperationResultValidator()
            .AddXOperationResultMiddleware()
            .AddXValidatorDefault();

    /// <summary>
    /// Adds operation result executors to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the executors to.</param>
    /// <param name="assemblies">The assemblies to scan for executors. 
    /// If none are provided, the calling assembly is used.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXOperationResultExecutors(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        List<Type> executorTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsInterface
                && type.IsSealed
                && type.IsAssignableTo(typeof(IOperationResultExecutor)))
            .ToList();

        foreach (Type executorType in executorTypes)
        {
            _ = services.AddScoped(typeof(IOperationResultExecutor), executorType);
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
    /// This class must implement <see cref="IOperationResultExecute"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add 
    /// the service to.</param>
    /// <returns>A reference to this instance after the operation has 
    /// completed.</returns>
    public static
        IServiceCollection AddXOperationResultExecute<TOperationResultExecute>(
        this IServiceCollection services)
        where TOperationResultExecute : class, IOperationResultExecute =>
        services.AddScoped<IOperationResultExecute, TOperationResultExecute>();

    /// <summary>
    /// Adds a scoped service of the type <see cref="OperationResultExecute"/> 
    /// to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add 
    /// the service to.</param>
    /// <returns>A reference to this instance after the operation has 
    /// completed.</returns>
    public static IServiceCollection AddXOperationResultExecute(
        this IServiceCollection services) =>
        services.AddXOperationResultExecute<OperationResultExecute>();

    /// <summary>
    /// Adds a scoped service of the type specified in 
    /// <typeparamref name="TOperationResultValidator"/> 
    /// with an implementation type of 
    /// <typeparamref name="TOperationResultValidator"/> to the specified 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TOperationResultValidator">The type of the service to add. 
    /// This class must implement <see cref="IOperationResultValidator"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add 
    /// the service to.</param>
    /// <returns>A reference to this instance after the operation has 
    /// completed.</returns>
    public static
        IServiceCollection AddXOperationResultValidator<TOperationResultValidator>(
        this IServiceCollection services)
        where TOperationResultValidator : class, IOperationResultValidator =>
        services.AddScoped<IOperationResultValidator, TOperationResultValidator>();

    /// <summary>
    /// Adds a scoped service of the type <see cref="OperationResultValidator"/> 
    /// to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add 
    /// the service to.</param>
    /// <returns>A reference to this instance after the operation has 
    /// completed.</returns>
    public static IServiceCollection AddXOperationResultValidator(
        this IServiceCollection services) =>
        services.AddXOperationResultValidator<OperationResultValidator>();

    /// <summary>
    /// Adds a scoped service of the type <see cref="OperationResultMiddleware"/> 
    /// to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add 
    /// the service to.</param>
    /// <returns>A reference to this instance after the operation has 
    /// completed.</returns>
    public static IServiceCollection AddXOperationResultMiddleware(
        this IServiceCollection services) =>
        services.AddScoped<OperationResultMiddleware>();

    /// <summary>
    /// Adds the <see cref="OperationResultMiddleware"/> to the application's 
    /// request pipeline.
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> to configure.</param>
    /// <returns>A reference to the <see cref="IApplicationBuilder"/> after the 
    /// operation has completed.</returns>
    public static IApplicationBuilder UseXOperationResultMiddleware(
        this IApplicationBuilder builder) =>
        builder.UseMiddleware<OperationResultMiddleware>();

    /// <summary>
    /// Applies the operation result filter to the response of the target route(s),
    /// adding the <see cref="OperationResultFilter"/> to the endpoint 
    /// convention builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the endpoint convention 
    /// builder.</typeparam>
    /// <param name="builder">The endpoint convention builder to configure.</param>
    /// <returns>A reference to the <typeparamref name="TBuilder"/> after the 
    /// operation has completed.</returns>
    /// <remarks>To be applied on many routes, please use 
    /// <see langword="MapGroup"/> with empty prefix 
    /// (<see cref="string.Empty"/>).
    /// <para>You can register the <see cref="IProblemDetailsService"/> in 
    /// order to customize the response.</para></remarks>
    public static TBuilder WithXOperationResultFilter<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.AddEndpointFilter<TBuilder, OperationResultFilter>();

    /// <summary>
    /// Applies the validation filter factory to the request of the target route(s),
    /// adding the <see cref="OperationResultValidationFilterFactory"/> to the endpoint 
    /// convention builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the endpoint convention 
    /// builder.</typeparam>
    /// <param name="builder">The endpoint convention builder to configure.</param>
    /// <returns>A reference to the <typeparamref name="TBuilder"/> after the 
    /// operation has completed.</returns>
    /// <remarks>To be applied on many routes, please use 
    /// <see langword="MapGroup"/> with empty prefix 
    /// (<see cref="string.Empty"/>).
    /// <para>The request must implement the <see cref="IUseValidator"/>.
    /// You can implement the <see cref="IValidator{TArgument}"/> specific
    /// to your request, otherwise you must use the built in one : Register the
    /// generic validator using <see langword="AddXValidatorGenerics"/> method.</para>
    /// <para>You may register the <see cref="IProblemDetailsService"/> in 
    /// order to customize the response.</para></remarks>
    public static TBuilder WithXValidationFilterFactory<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.AddEndpointFilterFactory(
            OperationResultValidationFilterFactory.FilterFactory);

    /// <summary>
    /// Applies the validation filter to the request of the target route(s),
    /// adding the <see cref="OperationResultValidationFilter"/> to the endpoint 
    /// convention builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the endpoint convention builder.</typeparam>
    /// <param name="builder">The endpoint convention builder to configure.</param>
    /// <returns>A reference to the <typeparamref name="TBuilder"/> after the 
    /// operation has completed.</returns>
    /// <remarks>To be applied on many routes, please use 
    /// <see langword="MapGroup"/> with empty prefix 
    /// (<see cref="string.Empty"/>).
    /// <para>The request must implement the <see cref="IUseValidator"/>.
    /// You can implement the <see cref="IValidator{TArgument}"/> specific
    /// to your request, otherwise you must use the built in one : Register the
    /// generic validator using <see langword="AddXValidatorGenerics"/> method.</para>
    /// <para>You can register the <see cref="IProblemDetailsService"/> in 
    /// order to customize the response.</para></remarks>
    public static TBuilder WithXValidationFilter<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.AddEndpointFilter(
            new OperationResultValidationFilter().InvokeAsync);

    /// <summary>
    /// Applies the operation result filter and validation filter to the 
    /// response of the target route(s),
    /// adding the <see cref="OperationResultFilter"/> and 
    /// <see cref="OperationResultValidationFilter"/> 
    /// to the endpoint convention builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the endpoint convention 
    /// builder.</typeparam>
    /// <param name="builder">The endpoint convention builder to configure.</param>
    /// <returns>A reference to the <typeparamref name="TBuilder"/> after 
    /// the operation has completed.</returns>
    /// <remarks>To be applied on many routes, please use <see langword="MapGroup"/> with empty prefix 
    /// (<see cref="string.Empty"/>).
    /// <para>You can register the <see cref="IProblemDetailsService"/> in order
    /// to customize the response.</para></remarks>
    public static TBuilder WithXOperationResultMinimalApi<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.WithXOperationResultFilter()
            .WithXValidationFilter();

    /// <summary>
    /// Adds the <see cref="OperationResultJsonOptions"/> to the 
    /// specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the 
    /// service to.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddXOperationResultJsonOptions(
        this IServiceCollection services) =>
        services.AddSingleton<IConfigureOptions<JsonOptions>,
            OperationResultJsonOptions>();

    /// <summary>
    /// Adds the <see cref="OperationResultControllerMvcOptions"/> to the 
    /// specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the 
    /// service to.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddXOperationResultControllerMvcOptions(
        this IServiceCollection services) =>
        services.AddSingleton<IConfigureOptions<MvcOptions>,
            OperationResultControllerMvcOptions>();
}
