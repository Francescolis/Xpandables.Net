﻿
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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.Operations.Minimals;

/// <summary>
/// Provides a factory for creating endpoint filters that validate operation results.
/// </summary>
public static class OperationResultValidationFilterFactory
{
    /// <summary>  
    /// Creates an endpoint filter delegate that validates the operation result.  
    /// </summary>  
    /// <param name="context">The context for the endpoint filter factory.</param>  
    /// <param name="next">The next endpoint filter delegate in the pipeline.</param>  
    /// <returns>An endpoint filter delegate that validates the operation result.</returns>  
    public static EndpointFilterDelegate FilterFactory(
        EndpointFilterFactoryContext context,
        EndpointFilterDelegate next)
    {
        IOperationResultValidator validator = context
            .ApplicationServices
            .GetRequiredService<IOperationResultValidator>();

        return invocationContext =>
            validator.ValidateAsync(invocationContext, next);
    }
}
