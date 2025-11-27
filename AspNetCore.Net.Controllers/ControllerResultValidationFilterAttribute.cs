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

using System.ExecutionResults;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AspNetCore.Net;

/// <summary>
/// An action filter attribute that validates the model state before executing a controller action and returns a
/// standardized error response if validation fails.
/// </summary>
/// <remarks>When applied to a controller or action, this attribute checks the model state before the action
/// executes. If the model state is invalid, it sets the result to a <see cref="BadRequestObjectResult"/> containing an
/// <see cref="OperationResult"/> with validation errors, preventing the action from running. This ensures that clients
/// receive consistent error responses for invalid input.</remarks>
public sealed class ControllerResultValidationFilterAttribute : ActionFilterAttribute
{
    /// <inheritdoc/>  
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.ModelState.IsValid)
        {
            return;
        }

        OperationResult executionResult = context.ModelState.ToOperationResult();

        context.Result = new BadRequestObjectResult(executionResult);
    }
}
