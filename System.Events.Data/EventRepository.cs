/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Diagnostics.CodeAnalysis;
using System.Entities.EntityFramework;

namespace System.Events.Data;

/// <summary>
/// Provides a repository for managing event entities within the specified event data context.
/// </summary>
/// <remarks>This class is sealed and cannot be inherited. It supports any entity type that implements
/// IEntityEvent, enabling flexible event management within the data context.</remarks>
/// <typeparam name="TEntityEvent">The type of event entity managed by the repository. Must implement the IEntityEvent interface.</typeparam>
/// <param name="context">The event data context that supplies access to the underlying data store for event entities.</param>
public sealed class EventRepository<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntityEvent>(EventDataContext context) : EntityRepository<TEntityEvent>(context)
    where TEntityEvent : class, IEntityEvent;
