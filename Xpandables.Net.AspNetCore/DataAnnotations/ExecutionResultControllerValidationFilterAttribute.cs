
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
using Microsoft.AspNetCore.Mvc.Filters;

using Xpandables.Net.Executions;

namespace Xpandables.Net.DataAnnotations;

/// <summary>
/// An action filter that validates the model state of the incoming request and sets a  <see
/// cref="BadRequestObjectResult"/> containing an <see cref="ExecutionResult"/> if the model state is invalid.
/// </summary>
/// <remarks>This filter checks the <see langword="ActionExecutingContext.ModelState"/> for validation errors.  If the
/// model state is invalid, it converts the errors into an <see cref="ExecutionResult"/>  and sets the <see
/// cref="ActionExecutingContext.Result"/> to a <see cref="BadRequestObjectResult"/>  containing the <see
/// cref="ExecutionResult"/>. If the model state is valid, the filter does nothing.</remarks>
public sealed class ExecutionResultControllerValidationFilterAttribute : ActionFilterAttribute
{
    /// <inheritdoc/>  
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.ModelState.IsValid)
        {
            return;
        }

        ExecutionResult executionResult =
            context.ModelState.ToExecutionResult();

        context.Result = new BadRequestObjectResult(executionResult);
    }
}
