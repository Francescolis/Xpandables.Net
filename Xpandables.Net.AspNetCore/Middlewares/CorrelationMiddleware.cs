﻿
/************************************************************************************************************
 * Copyright (C) 2020 Francis-Black EWANE
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
************************************************************************************************************/
using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using Xpandables.Net.Correlations;

namespace Xpandables.Net.Middlewares
{
    /// <summary>
    /// Adds the correlation header id to the current request.
    /// You can derive from this class to customize its behaviors.
    /// </summary>
    public class CorrelationMiddleware : IMiddleware
    {
        /// <summary>
        /// Request handling method after setting the correlation header id.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext" /> for the current request.</param>
        /// <param name="next">The delegate representing the remaining middleware in the request pipeline.</param>
        /// <returns>A <see cref="Task" /> that represents the execution of this middleware.</returns>
        public virtual Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var correlationId = Guid.NewGuid().ToString();
            context.Request.Headers.Add(ICorrelationContext.DefaultHeader, correlationId);

            return next(context);
        }
    }
}
