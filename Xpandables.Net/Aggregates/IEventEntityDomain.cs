﻿/*******************************************************************************
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
/// Represents a domain event entity.
/// </summary>
public interface IEventEntityDomain : IEventEntity
{
    /// <summary>
    /// Contains the string representation of the .Net aggregate id type name.
    /// </summary>
    string AggregateIdTypeName { get; }

    /// <summary>
    /// Gets the aggregate identifier.
    /// </summary>
    Guid AggregateId { get; }
}