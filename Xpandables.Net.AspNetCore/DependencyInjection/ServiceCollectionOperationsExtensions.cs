
/************************************************************************************************************
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
************************************************************************************************************/
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides method to register services.
/// </summary>
public static class ServiceCollectionOperationsExtensions
{
    /// <summary>
    /// Adds the default <see cref="OperationResultSerializationConfigureOptions"/> to the services 
    /// that configures the <see cref="Microsoft.AspNetCore.Http.Json.JsonOptions"/>
    /// converters with <see cref="JsonStringEnumConverter"/>, 
    /// <see cref="OperationResultJsonConverterFactory"/>,<see cref="JsonDateOnlyConverter"/>,
    /// <see cref="JsonNullableDateOnlyConverter"/>, <see cref="JsonTimeOnlyConverter"/> 
    /// and <see cref="JsonNullableTimeOnlyConverter"/>.    
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <remarks>For <see cref="DateOnly"/> and <see cref="TimeOnly"/> types, you need to 
    /// configure swagger with builder.Services.AddSwaggerGen(options =>
    /// <para>options.MapType{DateOnly}(() => new OpenApiSchema { Type = "string", Format = "date" }); </para>
    /// <para>options.MapType{TimeOnly}(() => new OpenApiSchema { Type = "string", Format = "time" });); </para>
    /// </remarks>
    public static IServiceCollection AddXOperationResultSerializationConfigureOptions(this IServiceCollection services)
        => services.AddXOperationResultSerializationConfigureOptions<OperationResultSerializationConfigureOptions>();

    /// <summary>
    /// Adds the specified <typeparamref name="TOperationResultSerializationConfigureOptions"/> 
    /// to the services.
    /// </summary>
    /// <typeparam name="TOperationResultSerializationConfigureOptions">the type 
    /// of operation result JSON configure.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXOperationResultSerializationConfigureOptions
        <TOperationResultSerializationConfigureOptions>(this IServiceCollection services)
        where TOperationResultSerializationConfigureOptions : OperationResultSerializationConfigureOptions
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services
            .AddSingleton<IConfigureOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>,
            TOperationResultSerializationConfigureOptions>();
        return services;
    }

    /// <summary>
    /// Adds the default <see cref="OperationResultControllerConfigureFormatterOptions"/> to 
    /// the services that configures the <see cref="JsonOptions"/>
    /// converters with <see cref="JsonStringEnumConverter"/>, <see cref="OperationResultJsonConverterFactory"/> 
    /// and <see cref="JsonDateOnlyConverter"/>.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXOperationResultControllerConfigureFormatterOptions(
        this IServiceCollection services)
        => services.AddXOperationResultControllerConfigureFormatterOptions<OperationResultControllerConfigureFormatterOptions>();

    /// <summary>
    /// Adds the specified <typeparamref name="TOperationResultConfigureJsonOptions"/> to the services.
    /// </summary>
    /// <typeparam name="TOperationResultConfigureJsonOptions">the type of operation result JSON configure.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXOperationResultControllerConfigureFormatterOptions
        <TOperationResultConfigureJsonOptions>(this IServiceCollection services)
        where TOperationResultConfigureJsonOptions : OperationResultControllerConfigureFormatterOptions
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddSingleton<IConfigureOptions<JsonOptions>, TOperationResultConfigureJsonOptions>();
        return services;
    }

    /// <summary>
    /// Adds the default <see cref="OperationResultControllerConfigureMvcOptions"/> to the services that configures the <see cref="MvcOptions"/>
    /// with filters <see cref="OperationResultControllerValidationFilterAttribute"/>, <see cref="OperationResultControllerFilter"/> and
    /// the binder <see cref="FromModelBinderProvider"/>.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXOperationResultControllerConfigureMvcOptions(this IServiceCollection services)
        => services.AddXOperationResultControllerConfigureMvcOptions<OperationResultControllerConfigureMvcOptions>();

    /// <summary>
    /// Adds the specified <typeparamref name="TOperationResultControllerConfigureMvcOptions"/> to the services.
    /// </summary>
    /// <typeparam name="TOperationResultControllerConfigureMvcOptions">the type of operation result MVC configure.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXOperationResultControllerConfigureMvcOptions
        <TOperationResultControllerConfigureMvcOptions>(this IServiceCollection services)
        where TOperationResultControllerConfigureMvcOptions : OperationResultControllerConfigureMvcOptions
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddSingleton<IConfigureOptions<MvcOptions>, TOperationResultControllerConfigureMvcOptions>();
        return services;
    }

    /// <summary>
    /// Adds the <see cref="OperationResultController"/> to the services.
    /// This controller is used to handle exceptions before target controller get called.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXOperationResultController(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddScoped<OperationResultController>();
        return services;
    }

    /// <summary>
    /// Registers the <see cref="OperationResultControllerMiddleware"/> and 
    /// <see cref="OperationResultController"/>to the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXOperationResultControllerMiddleware(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddScoped<OperationResultControllerMiddleware>();
        _ = services.AddScoped<OperationResultControllerMiddleware>();
        return services;
    }

    /// <summary>
    /// Registers the <see cref="OperationResultMinimalMiddleware"/> to the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXOperationResultMinimalMiddleware(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddScoped<OperationResultMinimalMiddleware>();
        return services;
    }

    /// <summary>
    /// Adds the <see cref="OperationResultControllerMiddleware"/>  type to the application's request pipeline.
    /// <para></para>
    /// <para>Make sure to register the <see cref="OperationResultControllerMiddleware"/> 
    /// using the <see cref="AddXOperationResultControllerMiddleware(IServiceCollection)"/> method.</para>
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> instance.</param>
    /// <returns>The <see cref="WebApplication"/> instance.</returns>
    public static WebApplication UseXOperationResultControllerMiddleware(this WebApplication builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        _ = builder.UseMiddleware<OperationResultControllerMiddleware>();

        return builder;
    }

    /// <summary>
    /// Adds the <see cref="OperationResultMinimalMiddleware"/>  type to the minimal application's request pipeline.
    /// <para></para>
    /// <para>Make sure to register the <see cref="OperationResultMinimalMiddleware"/> 
    /// using the <see cref="AddXOperationResultMinimalMiddleware(IServiceCollection)"/> method.</para>
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> instance.</param>
    /// <returns>The <see cref="WebApplication"/> instance.</returns>
    public static WebApplication UseXOperationResultMinimalMiddleware(this WebApplication builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        _ = builder.UseMiddleware<OperationResultMinimalMiddleware>();

        return builder;
    }
}
