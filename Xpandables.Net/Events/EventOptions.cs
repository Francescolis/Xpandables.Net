﻿/*******************************************************************************
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
using System.Text.Json;

using Xpandables.Net.Text;

namespace Xpandables.Net.Events;
/// <summary>
/// Represents the options for configuring event handling.
/// </summary>
public sealed record EventOptions
{
    /// <summary>
    /// Gets the JSON serializer options.
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get; init; } =
        DefaultSerializerOptions.Defaults;

    /// <summary>
    /// Gets a value indicating whether the event entity should be 
    /// disposed after persistence.
    /// </summary>
    public bool DisposeEventEntityAfterPersistence { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether snapshot is enabled.
    /// </summary>
    public bool IsSnapshotEnabled { get; init; } = true;

    /// <summary>
    /// Gets the frequency of snapshots.
    /// </summary>
    public ulong SnapshotFrequency { get; init; } = 50;

    /// <summary>
    /// Returns the default <see cref="EventOptions"/>.
    /// </summary>
    /// <param name="options">The options to use as a base for the default values.</param>
    /// <returns>The default <see cref="EventOptions"/>.</returns>
    public static EventOptions Default(EventOptions options) =>
        options with
        {
            SerializerOptions = DefaultSerializerOptions.Defaults,
            DisposeEventEntityAfterPersistence = true,
            IsSnapshotEnabled = true,
            SnapshotFrequency = 50
        };
}