
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
namespace Xpandables.Net.Repositories;

/// <summary>
/// Determines the state of the target entity.
/// </summary>
public static class EntityStatus
{
    /// <summary>
    /// it is currently functioning (is available)
    /// </summary>
    public static string ACTIVE => new(nameof(ACTIVE));

    /// <summary>
    /// It is not currently functioning (has been not active for a long time)
    /// </summary>
    public static string INACTIVE => new(nameof(INACTIVE));

    /// <summary>
    /// It is deleted (logical deletion)
    /// </summary>
    public static string DELETED => new(nameof(DELETED));

    /// <summary>
    /// It is suspended (for any reason)
    /// </summary>
    public static string SUSPENDED => new(nameof(SUSPENDED));

    internal static string UPDATED => new(nameof(UPDATED));
}
