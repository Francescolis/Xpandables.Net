/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc;

/// <summary>
/// Provides configuration options for MVC controllers, customizing behavior such as endpoint routing and response
/// formatting.
/// </summary>
/// <remarks>This class is typically used to configure MVC options during application startup. It disables
/// endpoint routing, enforces respect for browser Accept headers, and ensures that HTTP 406 Not Acceptable responses
/// are returned when appropriate. Additionally, it adds filters for controller result validation and processing. This
/// configuration is intended to be registered with the application's dependency injection system.</remarks>
public sealed class ControllerResulMvcOptions : IConfigureOptions<MvcOptions>
{
    /// <inheritdoc/>
    public void Configure(MvcOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.EnableEndpointRouting = false;
        options.RespectBrowserAcceptHeader = true;
        options.ReturnHttpNotAcceptable = true;

        _ = options.Filters.Add<ControllerResultValidationFilterAttribute>();
        _ = options.Filters.Add<ControllerResultFilter>(int.MinValue);
    }
}
