
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
namespace Xpandables.Net.Aggregates;

/// <summary>
/// Defines a contract for an aggregate store that supports snapshot 
/// operations.
/// </summary>
/// <typeparam name="TAggregateId">The type of the aggregate identifier
/// .</typeparam>
/// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
/// <remarks>This interface is used because AspNetCore DI does not
/// support Open Generic decorators.</remarks>
public interface IAggregateStoreSnapshot<TAggregate, TAggregateId>
    : IAggregateStore<TAggregate, TAggregateId>
    where TAggregate : class, IAggregate<TAggregateId>
    where TAggregateId : struct, IAggregateId<TAggregateId>
{
}
