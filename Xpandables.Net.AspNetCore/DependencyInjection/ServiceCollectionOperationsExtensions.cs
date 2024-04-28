
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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

// Ignore Spelling: Mvc Middleware

using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xpandables.Net.Operations;
using Xpandables.Net.Primitives.Converters;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides method to register services.
/// </summary>
public static class ServiceCollectionOperationsExtensions
{
    /// <summary>
    /// Registers the operation result response builder of specific type.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> 
    /// to add the service to.</param>
    /// <typeparam name="TOperationResultResponseBuilder">The type of 
    /// the operation result response builder.</typeparam>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <remarks>You can register the <see cref="IProblemDetailsService"/> 
    /// in order to customize the response.</remarks>
    public static IServiceCollection AddXOperationResultResponseBuilder
        <TOperationResultResponseBuilder>(
        this IServiceCollection services)
        where TOperationResultResponseBuilder :
        class, IOperationResultResponseBuilder
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .AddScoped
            <IOperationResultResponseBuilder, TOperationResultResponseBuilder>();
    }

    /// <summary>
    /// Registers the default operation result response builder.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the 
    /// service to.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <remarks>You can register the <see cref="IProblemDetailsService"/> in 
    /// order to customize the response.</remarks>
    public static IServiceCollection AddXOperationResultResponseBuilder(
        this IServiceCollection services)
        => services
            .AddXOperationResultResponseBuilder<OperationResultResponseBuilder>();

    /// <summary>
    /// Adds the default 
    /// <see cref="OperationResultSerializationConfigureOptions"/> to the 
    /// services that configures the 
    /// <see cref="Microsoft.AspNetCore.Http.Json.JsonOptions"/>
    /// converters with <see cref="JsonStringEnumConverter"/>, 
    /// <see cref="OperationResultAspJsonConverterFactory"/>,
    /// <see cref="DateOnlyJsonConverter"/>,
    /// <see cref="JsonNullableDateOnlyConverter"/>, 
    /// <see cref="TimeOnlyJsonConverter"/> 
    /// and <see cref="JsonNullableTimeOnlyConverter"/>.    
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    /// <remarks>For <see cref="DateOnly"/> and <see cref="TimeOnly"/> types, 
    /// you need to configure swagger with 
    /// builder.Services.AddSwaggerGen(options =>
    /// <para>options.MapType{DateOnly}(() => new OpenApiSchema { Type = "string", Format = "date" }); </para>
    /// <para>options.MapType{TimeOnly}(() => new OpenApiSchema { Type = "string", Format = "time" });); </para>
    /// </remarks>
    public static IServiceCollection
        AddXOperationResultSerializationConfigureOptions(
        this IServiceCollection services)
        => services
        .AddXOperationResultSerializationConfigureOptions
        <OperationResultSerializationConfigureOptions>();

    /// <summary>
    /// Adds the specified 
    /// <typeparamref name="TOperationResultSerializationConfigureOptions"/> 
    /// to the services.
    /// </summary>
    /// <typeparam name="TOperationResultSerializationConfigureOptions">the type 
    /// of operation result JSON configure.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection
        AddXOperationResultSerializationConfigureOptions
        <TOperationResultSerializationConfigureOptions>(
        this IServiceCollection services)
        where TOperationResultSerializationConfigureOptions :
        OperationResultSerializationConfigureOptions
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services
            .AddSingleton<IConfigureOptions
            <Microsoft.AspNetCore.Http.Json.JsonOptions>,
            TOperationResultSerializationConfigureOptions>();
        return services;
    }

    /// <summary>
    /// Adds the default 
    /// <see cref="OperationResultControllerConfigureFormatterOptions"/> to 
    /// the services that configures the <see cref="JsonOptions"/>
    /// converters with <see cref="JsonStringEnumConverter"/>, 
    /// <see cref="OperationResultAspJsonConverterFactory"/> 
    /// and <see cref="DateOnlyJsonConverter"/>.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXOperationResultConfigureFormatterOptions(
        this IServiceCollection services)
        => services.AddXOperationResultConfigureFormatterOptions
        <OperationResultControllerConfigureFormatterOptions>();

    /// <summary>
    /// Adds the specified 
    /// <typeparamref name="TOperationResultConfigureJsonOptions"/> to the
    /// services.
    /// </summary>
    /// <typeparam name="TOperationResultConfigureJsonOptions">the type of 
    /// operation result JSON configure.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXOperationResultConfigureFormatterOptions
        <TOperationResultConfigureJsonOptions>(
        this IServiceCollection services)
        where TOperationResultConfigureJsonOptions :
        OperationResultControllerConfigureFormatterOptions
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddSingleton<IConfigureOptions<JsonOptions>,
            TOperationResultConfigureJsonOptions>();
        return services;
    }

    /// <summary>
    /// Adds the default 
    /// <see cref="OperationResultControllerConfigureMvcOptions"/> to the services 
    /// that configures the <see cref="MvcOptions"/>
    /// with filters 
    /// <see cref="OperationResultControllerValidationFilterAttribute"/>, 
    /// <see cref="OperationResultControllerFilter"/> and
    /// the binder <see cref="FromModelBinderProvider"/>.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXOperationResultConfigureMvcOptions(
        this IServiceCollection services)
        => services.AddXOperationResultConfigureMvcOptions
        <OperationResultControllerConfigureMvcOptions>();

    /// <summary>
    /// Adds the specified 
    /// <typeparamref name="TOperationResultControllerConfigureMvcOptions"/> 
    /// to the services.
    /// </summary>
    /// <typeparam name="TOperationResultControllerConfigureMvcOptions">the 
    /// type of operation result MVC configure.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// is null.</exception>
    public static IServiceCollection AddXOperationResultConfigureMvcOptions
        <TOperationResultControllerConfigureMvcOptions>(
        this IServiceCollection services)
        where TOperationResultControllerConfigureMvcOptions :
        OperationResultControllerConfigureMvcOptions
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddSingleton<IConfigureOptions<MvcOptions>,
            TOperationResultControllerConfigureMvcOptions>();
        return services;
    }

    /// <summary>
    /// Registers the <see cref="OperationResultMiddleware"/> to the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    /// <remarks>You need to register the 
    /// <see cref="IOperationResultResponseBuilder"/>.</remarks>
    public static IServiceCollection AddXOperationResultMiddleware(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddScoped<OperationResultMiddleware>();
        return services;
    }

    /// <summary>
    /// Adds the <see cref="OperationResultMiddleware"/>  type to the minimal 
    /// application's request pipeline.
    /// <para></para>
    /// <para>Make sure to register the <see cref="OperationResultMiddleware"/> 
    /// using the <see cref="AddXOperationResultMiddleware(IServiceCollection)"/> 
    /// method.</para>
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> instance.</param>
    /// <returns>The <see cref="WebApplication"/> instance.</returns>
    public static WebApplication UseXOperationResultMiddleware(
        this WebApplication builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        _ = builder.UseMiddleware<OperationResultMiddleware>();

        return builder;
    }

    /// <summary>
    /// Applies the operation result filter to the response of the target route(s).
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/> to 
    /// add the filter to.</param>
    /// <returns>The <see cref="IEndpointConventionBuilder"/> instance.</returns>
    /// <remarks>To be applied on many routes, please use 
    /// <see langword="MapGroup"/> with empty prefix 
    /// (<see cref="string.Empty"/>).
    /// <para>You need to register the 
    /// <see cref="IOperationResultResponseBuilder"/> and you can also 
    /// register the <see cref="IProblemDetailsService"/> in order to customize 
    /// the response.</para></remarks>
    public static TBuilder WithXOperationResultFilter<TBuilder>(
        this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        _ = builder.AddEndpointFilter<TBuilder, OperationResultEndpointFilter>();
        return builder;
    }

}
