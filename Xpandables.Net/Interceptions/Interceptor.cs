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
namespace Xpandables.Net.Interceptions;

/// <summary>
/// This helper class allows the application author to 
/// implement the <see cref="IInterceptor"/> interface.
/// You must derive from this class in order to customize its behaviors.
/// </summary>
public abstract class Interceptor : IInterceptor
{
    /// <inheritdoc/>>
    public virtual bool CanHandle(IInvocation invocation) => true;

    /// <inheritdoc/>>
    public virtual void Intercept(IInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(invocation);

        invocation.Proceed();
    }

    /// <inheritdoc/>>
    public virtual async Task InterceptAsync(IInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(invocation);

        await Task.Yield();

        Intercept(invocation);
    }
}
