
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

namespace Xpandables.Net.Operations.Controllers;

/// <summary>  
/// Represents an action filter attribute that validates the model state before 
/// executing the action.  
/// </summary>  
public sealed class ExecutionResultControllerValidationFilterAttribute :
    ActionFilterAttribute
{
    /// <inheritdoc/>  
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.ModelState.IsValid)
        {
            return;
        }

        IExecutionResult executionResult =
            context.ModelState.ToExecutionResult();

        context.Result = new BadRequestObjectResult(executionResult);
    }
}
