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
/// Aspect finalizer attribute, when applied to a class that implements the
/// <paramref name="interfaceType"/>, specifies that the finalizer will get called
/// after the method invocation.
/// </summary>
/// <param name="interfaceType">The interface type to intercept.</param>
/// <exception cref="ArgumentNullException">The interface type is null.
/// </exception>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = true)]
public sealed class AspectFinalizerAttribute(Type interfaceType) :
    AspectAttribute(interfaceType)
{
    /// <summary>
    /// Defines whether the finalizer should be called in case of exception.
    /// </summary>
    /// <remarks>If set to <see langword="true"/>, the finalizer is responsible
    /// to return the right result in the expected type and
    /// must be defined.</remarks>
    public bool CallFinalizerOnException { get; set; }

    /// <inheritdoc/>
    public override IInterceptor Create(IServiceProvider serviceProvider)
        => serviceProvider.GetRequiredService<OnAspectFinalizer>();
}
