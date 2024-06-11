
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

namespace Xpandables.Net.Primitives;

/// <summary>
/// Defines the struct that holds data for a binary content result.
/// </summary>
public readonly record struct BinaryResult : IDisposable
{
    /// <summary>
    /// Gets the name.
    /// </summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the stream content.
    /// </summary>
    [Required]
    public required Stream Stream { get; init; }

    /// <summary>
    /// Gets the content type.
    /// </summary>
    [Required]
    public required string ContentType { get; init; }

    /// <summary>
    /// Gets the extension.
    /// </summary>
    [Required]
    public required string Extension { get; init; }

    /// <summary>
    /// Convert the content to base64 string.
    /// </summary>
    /// <returns>The string representation, in base 64, 
    /// of the content.</returns>
    public string ConvertToBase64String()
    {
        if (Stream is MemoryStream ms)
        {
            return Convert.ToBase64String(ms.ToArray());
        }

        using MemoryStream memoryStream = new();
        Stream.CopyTo(memoryStream);
        byte[] content = memoryStream.ToArray();

        return Convert.ToBase64String(content);
    }

    /// <summary>
    /// Convert the content to base64 data string.
    /// </summary>
    /// <remarks>Example : "data:image/png;base64,DIKJ1245JDKkhlSKLLKS.."</remarks>
    /// <returns>The string representation, in base 64 data, 
    /// of the content.</returns>
    public string ConvertToBase64Data()
        => $"data:{ContentType};base64,{ConvertToBase64String()}";

    void IDisposable.Dispose() => Stream.Dispose();
}