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
using Xpandables.Net.Operations;

namespace Xpandables.Net.Aspects;

/// <summary>
/// Aspect validator attribute, when applied to a class that implements the
/// <typeparamref name="TInterface"/>,specifies that, for all the methods of 
/// this class, arguments should be validated.
/// </summary>
/// <remarks>The decorated method should return <see cref="IOperationResult"/>
/// or you must enable the <see cref="ThrowException"/>.</remarks>
/// <typeparam name="TInterface">The type of the interface.</typeparam>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = true)]
public sealed class AspectValidatorAttribute<TInterface> :
    AspectAttribute<TInterface>
    where TInterface : class
{
    /// <summary>
    /// Gets or sets a value indicating whether to throw an exception of type
    /// <see cref="OperationResultException"/> when the validation fails. 
    /// If not set, the validator will return an implementation of 
    /// <see cref="IOperationResult"/>.
    /// </summary>
    /// <remarks>The attribute set on the method takes priority over the one
    /// from the class.</remarks>
    public bool ThrowException { get; set; }

    ///<inheritdoc/>
    public override IInterceptor Create(IServiceProvider serviceProvider)
        => serviceProvider
            .GetRequiredService<OnAspectValidator<TInterface>>();
}
