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
/// Aspect visitor attribute, when applied to a class that implements the
/// <typeparamref name="TInterface"/>,specifies that, for all the methods of 
/// this class, arguments should be visited.
/// </summary>
/// <typeparam name="TInterface">The type of the interface.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AspectVisitorAttribute<TInterface> :
    AspectAttribute<TInterface>
    where TInterface : class
{
    ///<inheritdoc/>
    public override IInterceptor Create(IServiceProvider serviceProvider)
        => serviceProvider
            .GetRequiredService<OnAspectVisitor<TInterface>>();
}
