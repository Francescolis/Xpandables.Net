
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

using Xpandables.Net.Primitives.Text;

namespace Xpandables.Net.Primitives;

/// <summary>
/// Contains the properties : <see cref="Key"/>, <see cref="Value"/> and
/// <see cref="Salt"/> of an encrypted data.
/// </summary>
/// <remarks>Used with <see cref="TextCryptography"/></remarks>
[DebuggerDisplay("Key = {Key}, Value = {Value}, Salt = {Salt}")]
public readonly record struct EncryptedValue
{
    /// <summary>
    /// Gets the encryption key.
    /// </summary>
    [Required, DataType(DataType.Text)]
    public required string Key { get; init; }

    /// <summary>
    /// Gets the base64 encrypted value.
    /// </summary>
    [Required, DataType(DataType.Text)]
    public required string Value { get; init; }

    /// <summary>
    /// Gets the base64 salt value.
    /// </summary>
    [Required, DataType(DataType.Text)]
    public required string Salt { get; init; }
}
