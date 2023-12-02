
/************************************************************************************************************
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
************************************************************************************************************/
using System.ComponentModel.DataAnnotations;

namespace Xpandables.Net.Primitives;

/// <summary>
/// Defines the struct that holds data for a binary content result.
/// </summary>
/// <param name="Title">The title.</param>
/// <param name="Content">The byte content.</param>
/// <param name="ContentType">The Content-Type header of the file.</param>
/// <param name="Extension">The file format of this picture.</param>
public readonly record struct BinaryEntry(
    [Required] string Title,
#pragma warning disable CA1819 // Properties should not return arrays
    [Required] byte[] Content,
#pragma warning restore CA1819 // Properties should not return arrays
    [Required] string ContentType,
    [Required] string Extension) : IDisposable
{
    /// <summary>
    /// Clears the content of the <see cref="BinaryEntry"/>.
    /// </summary>
    /// <returns>The current instance without content.</returns>
    public void Clear() => Array.Clear(Content, 0, Content.Length);

    /// <summary>
    /// Convert the content to base64 string.
    /// </summary>
    /// <returns>The string representation, in base 64, of the content.</returns>
    public string ConvertToBase64String() => Convert.ToBase64String(Content);

    /// <summary>
    /// Convert the content to base64 data string.
    /// </summary>
    /// <remarks>Example : "data:image/png;base64,DIKJ1245JDKkhlSKLLKS...."</remarks>
    /// <returns>The string representation, in base 64 data, of the content.</returns>
    public string ConvertToBase64Data() => $"data:{ContentType};base64,{ConvertToBase64String()}";

    /// <summary>
    /// Returns the UTF8 encoded string of the image.
    /// </summary>
    /// <returns>An UTF8 string.</returns>
    public override string ToString() => System.Text.Encoding.UTF8.GetString(Content, 0, Content.Length);
    void IDisposable.Dispose() => Array.Clear(Content, 0, Content.Length);
}