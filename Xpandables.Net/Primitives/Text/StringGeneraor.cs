/************************************************************************************************************
 * Copyright (C) 2022 Francis-Black EWANE
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

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Xpandables.Net.Primitives.Text;

/// <summary>
/// Provides with methods to generate strings from random characters.
/// </summary>
public static class StringGenerator
{
    /// <summary>
    /// The lookup characters used to generate random string.
    /// </summary>
    public const string LookupCharacters = "abcdefghijklmonpqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789,;!(-è_çàà)=@%µ£¨//?§/.?";

    /// <summary>
    /// Generates a string of the specified length that contains random characters.
    /// <para>The implementation uses the <see cref="RNGCryptoServiceProvider"/>.</para>
    /// </summary>
    /// <param name="length">The length of the expected string value.</param>
    /// <returns>A new string of the specified length with random characters.</returns>
    /// <exception cref="ArgumentException">The <paramref name="length"/> must be greater than zero
    /// and lower or equal to <see cref="ushort.MaxValue"/>.</exception>
    public static string Generate(ushort length) => Generate(length, LookupCharacters);

    /// <summary>
    /// Generates a string of the specified length that contains random characters from the lookup characters.
    /// <para>The implementation uses the <see cref="RNGCryptoServiceProvider"/>.</para>
    /// </summary>
    /// <param name="length">The length of the expected string value.</param>
    /// <param name="lookupCharacters">The string to be used to pick characters from or default one.</param>
    /// <returns>A new string of the specified length with random characters.</returns>
    /// <exception cref="ArgumentException">The <paramref name="length"/> must be greater than zero
    /// and lower or equal to <see cref="ushort.MaxValue"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="lookupCharacters"/> is null.</exception>
    public static string Generate(ushort length, string lookupCharacters)
    {
        if (length == 0)
            throw new ArgumentException(
                $"{nameof(length)} must be greater than zero and lower or equal to {ushort.MaxValue}");

        if (string.IsNullOrWhiteSpace(lookupCharacters))
            throw new ArgumentNullException(nameof(lookupCharacters));

        StringBuilder stringResult = new(length);

        using RandomNumberGenerator random = RandomNumberGenerator.Create();

        int count = (int)Math.Ceiling(Math.Log(lookupCharacters.Length, 2) / 8.0);
        Debug.Assert(count <= sizeof(uint));

        int offset = BitConverter.IsLittleEndian ? 0 : sizeof(uint) - count;
        int max = (int)(Math.Pow(2, count * 8) / lookupCharacters.Length) * lookupCharacters.Length;

        byte[] uintBuffer = new byte[sizeof(uint)];

        while (stringResult.Length < length)
        {
            random.GetBytes(uintBuffer, offset, count);
            var number = BitConverter.ToUInt32(uintBuffer, 0);

            if (number < max)
                stringResult.Append(lookupCharacters[(int)(number % lookupCharacters.Length)]);
        }

        return stringResult.ToString();
    }
}
