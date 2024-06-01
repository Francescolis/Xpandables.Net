/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
using Xpandables.Net.Interceptions;

namespace Xpandables.Net.Aspects;

/// <summary>
/// This class adds retry functionality to the method of the implementation of
/// the interface decorated with <see cref="AspectRetryAttribute{TInterface}"/>.
/// </summary>
/// <typeparam name="TInterface">The type of the interface.</typeparam>
/// <param name="aspectRetry">The aspect retry.</param>
public sealed class OnAspectRetry<TInterface>
    (IAspectRetry? aspectRetry = default) : OnAspect<TInterface>
    where TInterface : class
{
    /// <summary>
    /// Gets or sets the maximum number of retries.
    /// </summary>
    public int MaxRetries { get; internal set; }

    /// <summary>
    /// Gets or sets the delay between retries.
    /// </summary>
    public TimeSpan Delay { get; internal set; }

    ///<inheritdoc/>
    public override void Intercept(IInvocation invocation)
    {
        if (AspectAttribute is AspectRetryAttribute<TInterface> retryAttribute)
        {
            if (retryAttribute.IsDisabled)
            {
                invocation.Proceed();
                return;
            }

            if (MaxRetries <= 0)
            {
                MaxRetries = retryAttribute.MaxRetries;
                Delay = retryAttribute.Delay;
            }
        }

        for (int attempt = 0; ; attempt++)
        {
            try
            {
                invocation.Proceed();
                return;
            }
            catch (Exception exception)
                when (attempt < MaxRetries - 1)
            {
                aspectRetry?.OnException(exception, attempt);
                Thread.Sleep(Delay);
            }
        }
    }
}
