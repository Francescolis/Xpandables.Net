﻿/************************************************************************************************************
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
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Represents the unique identifier for an aggregate that contains a <see cref="Guid"/> as key.
/// </summary>
public interface IAggregateId : IPrimitive<Guid> { }

/// <summary>
/// Represents the unique identifier for an aggregate that contains a <see cref="Guid"/> as key.
/// </summary>
/// <typeparam name="TAggregateId">The type that implements this interface</typeparam>
public interface IAggregateId<TAggregateId> : IAggregateId, IPrimitive<TAggregateId, Guid>
    where TAggregateId : struct, IAggregateId<TAggregateId>
{
}