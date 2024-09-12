
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
using Microsoft.AspNetCore.Routing;

using Xpandables.Net.DependencyInjection;

namespace Xpandables.Net.Operations;

/// <summary>
/// When implemented by a class, provides with a method to add routes and a 
/// method to add services for an application when targeting minimal Api.
/// Inherits <see cref="IServiceRegister"/> interface.
/// </summary>
/// <remarks>All the implementation classes get registered using the 
/// <see cref="ServiceCollectionEndpointExtensions.AddXEndpointRoutes(Microsoft.Extensions.DependencyInjection.IServiceCollection, System.Reflection.Assembly[])"/>
/// and applied with <see langword="ServiceCollectionEndpointExtensions.UseXEndpointRoutes(WebApplication, params System.Reflection.Assembly[])"/>.</remarks>
public interface IEndpointRoute : IServiceRegister
{
    /// <summary>
    /// Used the specified route builder to add routes to the current application.
    /// </summary>
    /// <param name="app">The route builder to act with.</param>
    void AddRoutes(IEndpointRouteBuilder app);
}
