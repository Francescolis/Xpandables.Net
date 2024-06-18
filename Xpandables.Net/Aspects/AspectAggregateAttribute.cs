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

using Xpandables.Net.Aggregates;
using Xpandables.Net.Commands;
using Xpandables.Net.Interceptions;

namespace Xpandables.Net.Aspects;

/// <summary>
/// Aspect aggregate handler attribute, when applied to a class that implements 
/// the <see cref="IAggregateCommandHandler{TAggregate, TAggregateCommand}"/>, 
/// specifies that the handler will get called before the method invocation 
/// providing the expected parameters and apply the persistence process.
/// </summary>
/// <exception cref="ArgumentNullException">The interface type is null.
/// </exception>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class AspectAggregateAttribute<TAggregate, TAggregateCommand> :
        AspectAttribute
    where TAggregate : class, IAggregate
    where TAggregateCommand : notnull, IAggregateCommand
{
    /// <summary>
    /// Constructs the aspect aggregate handler attribute.
    /// </summary>
    public AspectAggregateAttribute()
        : base(typeof(IAggregateCommandHandler<TAggregate, TAggregateCommand>))
    { }

    /// <inheritdoc/>
    public override IInterceptor Create(
        IServiceProvider serviceProvider)
        => throw new NotImplementedException();
}
