
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
using System.ComponentModel;

using Xpandables.Net.Primitives;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Represents the unique identifier for an aggregate with a <see cref="Guid"/> 
/// as key.
/// </summary>
/// <remarks>This interface inherits from <see cref="IPrimitive{TValue}"/>
/// where <see cref="IPrimitive{TValue}.Value"/> is <see cref="Guid"/></remarks>
public interface IAggregateId : IPrimitive<Guid>
{
    /// <summary>
    /// Gets a value indicating whether or not the underlying 
    /// identifier is new (Value == Guid.Empty).
    /// </summary>
    public new bool IsNew() => Value == Guid.Empty;

    [EditorBrowsable(EditorBrowsableState.Never)]
    bool IPrimitive.IsNew() => IsNew();
}

/// <summary>
/// Represents the unique identifier of a specific type for an aggregate 
/// with a <see cref="Guid"/> as key.
/// </summary>
/// <typeparam name="TAggregateId">The type that implements this 
/// interface.</typeparam>
/// <remarks>This interface inherits from <see cref="IAggregateId"/>
/// and <see cref="IPrimitive{TPrimitive, TValue}"/>.</remarks>
public interface IAggregateId<TAggregateId>
    : IAggregateId, IPrimitive<TAggregateId, Guid>
    where TAggregateId : notnull, IAggregateId<TAggregateId>
{
}
