
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
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Xpandables.Net.Primitives;

/// <summary>
/// Contains the properties : <see cref="Value"/> and <see cref="Expiry"/> 
/// of a refresh token.
/// </summary>
[DebuggerDisplay("Value = {Value}, Expiry = {Expiry}")]
public readonly record struct RefreshToken
{
    /// <summary>
    /// Gets the value of the token.
    /// </summary>
    [Required, DataType(DataType.Text)]
    public required string Value { get; init; }

    /// <summary>
    /// Gets the token expiry date.
    /// </summary>
    [Required, DataType(DataType.DateTime)]
    public required DateTime Expiry { get; init; }
}
