
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
using System.ComponentModel.Composition;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.Compositions;

/// <summary>
/// Provides with an interface that allows external libraries to register 
/// types to the services collection.
/// This interface is used with MEF : Managed Extensibility Framework.
/// The implementation class must be decorated with the attribute 
/// <see cref="ExportAttribute"/> attribute,
/// with <see cref="IAddServiceExport"/> type as contract type.
/// </summary>
public interface IAddServiceExport
{
    /// <summary>
    /// When implemented, this method should add types to the services collection.
    /// </summary>
    /// <param name="services">The services collection to act on.</param>
    /// <param name="configuration">The application configuration.</param>
    void AddServices(IServiceCollection services, IConfiguration configuration);
}
