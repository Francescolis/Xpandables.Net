﻿
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// When implemented by a class, provides with a method to register services 
/// to service collection.
/// </summary>
/// <remarks>All the implementation classes get registered using the 
/// <see langword="AddXRegisters(IServiceCollection, IConfiguration?, System.Reflection.Assembly[])"/>.</remarks>
public interface IServiceRegister
{
    /// <summary>
    /// Use the specified collection to register services.
    /// </summary>
    /// <param name="services">The service collection to act on.</param>
    public void RegisterServices(IServiceCollection services)
    {
        // add your code here
    }

    /// <summary>
    /// Use the specified collection and configuration to register services.
    /// </summary>
    /// <param name="services">The service collection to act on.</param>
    /// <param name="configuration">The current service configuration.</param>
    public void RegisterServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        // add your code here
    }
}
