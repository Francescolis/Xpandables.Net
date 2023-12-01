
/************************************************************************************************************
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
************************************************************************************************************/
using System.Diagnostics;

using Xpandables.Net.Primitives;
using Xpandables.Net.Primitives.I18n;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Aggregate extension methods.
/// </summary>
public static class AggregateExtensions
{
    /// <summary>
    /// Creates a new instance of an aggregate of specific types.
    /// The type must contains a parameterless constructor.
    /// </summary>
    /// <typeparam name="TAggregate">The type of aggregate.</typeparam>
    /// <typeparam name="TAggregateId">The type of aggregate id.</typeparam>
    /// <returns>An instance of aggregate of <typeparamref name="TAggregate"/> type.</returns>
    /// <exception cref="InvalidOperationException">Unable to create the instance.</exception>
    [DebuggerStepThrough]
    public static TAggregate CreateEmptyAggregateInstance<TAggregate, TAggregateId>()
        where TAggregateId : struct, IAggregateId<TAggregateId>
        where TAggregate : class, IAggregate<TAggregateId>
    {
        try
        {
            if (Activator.CreateInstance(typeof(TAggregate), true) is not TAggregate aggregate)
                throw new InvalidOperationException(
                    I18nXpandables.AggregateFailedToCreateInstance.StringFormat(typeof(TAggregate).Name),
                    new ArgumentNullException(typeof(TAggregate).Name, "Null value."));

            return aggregate;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                I18nXpandables.AggregateFailedToCreateInstance.StringFormat(typeof(TAggregate).Name),
                exception);
        }
    }
}