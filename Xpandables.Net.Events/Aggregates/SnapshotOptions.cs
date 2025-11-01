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
namespace Xpandables.Net.Events.Aggregates;

/// <summary>
/// Represents the options used to configure the behavior of snapshots.
/// Provides settings that control whether snapshots are enabled and the frequency of snapshot creation.
/// </summary>
public sealed record SnapshotOptions
{
    /// <summary>
    /// Gets a value indicating whether snapshot is enabled.
    /// </summary>
    public bool IsSnapshotEnabled { get; set; } = true;

    /// <summary>
    /// Gets the frequency of snapshots.
    /// </summary>
    public long SnapshotFrequency { get; set; } = 50;
}
