
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

namespace Xpandables.Net.DependencyInjection;

/// <summary>  
/// Defines a contract for a service that configures middleware for a 
/// <see cref="WebApplication"/>.  
/// </summary>  
public interface IUseService
{
    /// <summary>  
    /// Configures the middleware for the specified <see cref="WebApplication"/>.  
    /// </summary>  
    /// <param name="application">The <see cref="WebApplication"/> 
    /// to configure.</param>  
    void UseServices(WebApplication application);
}
