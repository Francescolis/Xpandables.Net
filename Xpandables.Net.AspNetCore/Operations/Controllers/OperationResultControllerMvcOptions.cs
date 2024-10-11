
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Xpandables.Net.Operations.Controllers;

/// <summary>
/// Configures MVC options for the OperationResultController.
/// </summary>
public sealed class OperationResultControllerMvcOptions :
    IConfigureOptions<MvcOptions>
{
    /// <inheritdoc/>
    public void Configure(MvcOptions options)
    {
        options.EnableEndpointRouting = false;
        options.RespectBrowserAcceptHeader = true;
        options.ReturnHttpNotAcceptable = true;

        _ = options.Filters.Add<OperationResultControllerValidationFilterAttribute>();
        _ = options.Filters.Add<OperationResultControllerFilter>(int.MinValue);
    }
}
