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

using Xpandables.Net.Aggregates;
using Xpandables.Net.Distribution;
using Xpandables.Net.Interceptions;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Aspects;

/// <summary>
/// Aspect aggregate handler attribute, when applied to a class that implements 
/// the <see cref="IRequestAggregateHandler{TRequest, TAggregate}"/>, 
/// specifies that the aspect will get called before the method invocation 
/// providing the expected parameters and apply the persistence process.
/// </summary>
/// <remarks>You can set the <see cref="ContinueWhenNotFound"/> to
/// <see langword="true"/>, to allow process to continue and provide in
/// the code, the value of the aggregate. Usefull when you are creating
/// a new aggregate.</remarks>
/// <exception cref="ArgumentNullException">The interface type is null.
/// </exception>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class AspectAggregateAttribute<TRequest, TAggregate> :
    AspectAttribute, IAggregateDecorator
    where TAggregate : class, IAggregate
    where TRequest : class, IRequestAggregate<TAggregate>
{
    /// <summary>
    /// Determines whether the aspect should continue when the aggregate 
    /// is not found.
    /// </summary>
    /// <remarks>The default value is <see langword="false"/>.
    /// Usefull when you are creating
    /// a new aggregate.</remarks>
    public bool ContinueWhenNotFound { get; set; }

    /// <summary>
    /// Constructs the aspect aggregate handler attribute.
    /// </summary>
    public AspectAggregateAttribute()
        : base(typeof(IRequestAggregateHandler<TRequest, TAggregate>))
    { }

    /// <inheritdoc/>
    public override IInterceptor Create(
        IServiceProvider serviceProvider)
        => serviceProvider
        .GetRequiredService<OnAspectAggregate<TRequest, TAggregate>>();
}
