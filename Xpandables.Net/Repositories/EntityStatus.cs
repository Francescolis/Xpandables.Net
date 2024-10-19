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
namespace Xpandables.Net.Repositories;

/// <summary>
/// Provides status constants for entities.
/// </summary>
public static class EntityStatus
{
    /// <summary>
    /// It is currently functioning (is available).
    /// </summary>
    public const string ACTIVE = nameof(ACTIVE);

    /// <summary>
    /// It is pending (for any reason).
    /// </summary>
    public const string PENDING = nameof(PENDING);

    /// <summary>
    /// It is deleted (logical deletion).
    /// </summary>
    public const string DELETED = nameof(DELETED);

    /// <summary>
    /// It is suspended (for any reason).
    /// </summary>
    public const string SUSPENDED = nameof(SUSPENDED);

    /// <summary>
    /// It is in an error state.
    /// </summary>
    public const string ONERROR = nameof(ONERROR);

    /// <summary>
    /// It is published.
    /// </summary>
    public const string PUBLISHED = nameof(PUBLISHED);
}
