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
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Interceptions;

namespace Xpandables.Net.Aspects;

/// <summary>
/// Aspect retry attribute, when applied to a class that implements the
/// <typeparamref name="TInterface"/>,specifies that, for all the methods of
/// this class, the method should be retried if it fails.
/// </summary>
/// <typeparam name="TInterface">The type of the interface.</typeparam>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = true)]
public sealed class AspectRetryAttribute<TInterface> :
    AspectAttribute<TInterface>
    where TInterface : class
{
    private int _maxRetries = 3;

    /// <summary>
    /// Gets or sets the maximum number of retries.
    /// </summary>
    /// <remarks>The default value is 3.</remarks>
    public int MaxRetries
    {
        get => _maxRetries;
        set => _maxRetries = value <= 0
            ? throw new ArgumentOutOfRangeException(nameof(value))
            : value;
    }

    /// <summary>
    /// Gets or sets the delay between retries.
    /// </summary>
    /// <remarks>The default value is 1000.</remarks>
    public TimeSpan Delay { get; set; } = TimeSpan.FromMilliseconds(1000);

    /// <summary>
    /// Determines whether the aspect is disabled.
    /// </summary>
    /// <remarks>The default value is <see langword="false"/>.</remarks>
    public bool IsDisabled { get; set; }

    ///<inheritdoc/>
    public override IInterceptor Create(IServiceProvider serviceProvider)
    {
        OnAspectRetry<TInterface> aspectRetry = serviceProvider
            .GetRequiredService<OnAspectRetry<TInterface>>();

        aspectRetry.MaxRetries = MaxRetries;
        aspectRetry.Delay = Delay;

        return aspectRetry;
    }
}
