
/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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
using System.ComponentModel.DataAnnotations;

namespace Xpandables.Net.Repositories;
/// <summary>
/// Represents an entity that supports concurrency control.
/// </summary>
/// <typeparam name="TVersion">The type of the version property.</typeparam>
public interface IEntityConcurrency<out TVersion>
    where TVersion : notnull, IComparable
{
    /// <summary>
    /// Gets the version of the entity for concurrency control.
    /// </summary>
    [ConcurrencyCheck]
    TVersion Version { get; }
}
