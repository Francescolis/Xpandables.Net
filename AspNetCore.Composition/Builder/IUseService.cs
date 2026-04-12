/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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
namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Defines a contract for configuring and initializing services and middleware within a web application, supporting
/// both synchronous and asynchronous operations.
/// </summary>
/// <remarks>Implementations of this interface can be used to modularize service and middleware configuration in
/// ASP.NET Core applications. The interface allows specifying the order in which services are applied, which can be
/// important when multiple service modules are used together.</remarks>
public interface IUseService
{
	/// <summary>
	/// Gets the order or priority of the item within a collection or sequence.
	/// </summary>
	int Order { get; }

	/// <summary>  
	/// Configures the middleware for the specified <see cref="WebApplication"/>.  
	/// </summary>  
	/// <param name="application">The <see cref="WebApplication"/> 
	/// to configure.</param>  
	void UseServices(WebApplication application);

	/// <summary>
	/// Configures and initializes application services asynchronously for the specified web application.
	/// </summary>
	/// <param name="application">The web application instance to configure and initialize services for. Cannot be null.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task UseServicesAsync(WebApplication application, CancellationToken cancellationToken = default);
}
