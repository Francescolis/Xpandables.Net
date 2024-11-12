﻿
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
using System.ComponentModel;

namespace Xpandables.Net.Responsibilities;

/// <summary>
/// Represents a command that contains a dependency type and its key 
/// identifier.
/// </summary>
/// <typeparam name="TDependency">The type of the aggregate.</typeparam>
public abstract record Command<TDependency> : IDeciderCommand<TDependency>
    where TDependency : class
{
    /// <inheritdoc/>
    public Type Type => typeof(TDependency);

    /// <inheritdoc/>
    public required object KeyId { get; init; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    object IDeciderCommand.Dependency { get; set; } = default!;
}
