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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Xpandables.Net.Text;

/// <summary>
/// Represents a token with a value, type, and expiration date.
/// </summary>
[DebuggerDisplay("Value = {Value}, Type = {Type}, Expiration = {Expiration}")]
[StructLayout(LayoutKind.Auto)]
public readonly record struct TokenValue
{
    /// <summary>
    /// Gets the value of the token.
    /// </summary>
    public required string Value { get; init; }
    /// <summary>
    /// Gets the type of the token.
    /// </summary>
    public required string Type { get; init; }
    /// <summary>
    /// Gets the expiration date of the token.
    /// </summary>
    public required DateTime Expiration { get; init; }

    /// <summary>
    /// Gets a value indicating whether the token is expired.
    /// </summary>
    public readonly bool GetIsExpired() => DateTime.UtcNow > Expiration;
}
