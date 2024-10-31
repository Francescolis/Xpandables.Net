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
/// Represents an encrypted value with its associated key and salt.
/// </summary>
[DebuggerDisplay("Key = {Key}, Value = {Value}, Salt = {Salt}")]
[StructLayout(LayoutKind.Auto)]
public readonly record struct EncryptedValue
{
    /// <summary>
    /// Gets the key used for encryption.
    /// </summary>
    public required string Key { get; init; }
    /// <summary>
    /// Gets the encrypted value.
    /// </summary>
    public required string Value { get; init; }
    /// <summary>
    /// Gets the salt used for encryption.
    /// </summary>
    public required string Salt { get; init; }
}
