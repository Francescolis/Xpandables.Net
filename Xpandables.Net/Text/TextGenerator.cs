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
using System.Security.Cryptography;
using System.Text;

namespace Xpandables.Net.Text;
/// <summary>  
/// Provides methods to generate random text strings.  
/// </summary>  
public static class TextGenerator
{
    /// <summary>  
    /// The lookup characters used to generate random string.  
    /// </summary>  
    // ReSharper disable once MemberCanBePrivate.Global
    public const string LookupCharacters = "abcdefghijklmonpqrstuvwxyzABCDEFGH" +
                                           "IJKLMNOPQRSTUVWXYZ0123456789,;!(-è_çàà)=@%µ£¨//?§/.?";

    /// <summary>  
    /// Generates a random string of the specified length using the default   
    /// lookup characters.  
    /// </summary>  
    /// <param name="length">The length of the random string to generate.   
    /// Must be between 1 and ushort.MaxValue.</param>  
    /// <returns>A random string of the specified length.</returns>  
    public static string Generate(int length) =>
        Generate(length, LookupCharacters);

    /// <summary>  
    /// Generates a random string of the specified length using the provided   
    /// lookup characters.  
    /// </summary>  
    /// <param name="length">The length of the random string to generate.   
    /// Must be between 1 and ushort.MaxValue.</param>  
    /// <param name="lookupCharacters">The characters to use for generating   
    /// the random string. Cannot be null or whitespace.</param>  
    /// <returns>A random string of the specified length.</returns>  
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the length   
    /// is less than 1 or greater than ushort.MaxValue.</exception>  
    /// <exception cref="ArgumentException">Thrown when the lookupCharacters   
    /// is null or whitespace.</exception>  
    public static string Generate(int length, string lookupCharacters)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(length, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, ushort.MaxValue);
        ArgumentException.ThrowIfNullOrWhiteSpace(lookupCharacters);

        StringBuilder stringResult = new(length);

        using RandomNumberGenerator random = RandomNumberGenerator.Create();

        int count = (int)Math.Ceiling(
            Math.Log(lookupCharacters.Length, 2) / 8.0);

        Debug.Assert(count <= sizeof(uint));

        int offset = BitConverter.IsLittleEndian ? 0 : sizeof(uint) - count;
        int max = (int)(Math.Pow(2, count * 8) / lookupCharacters.Length)
            * lookupCharacters.Length;

        byte[] uintBuffer = new byte[sizeof(uint)];

        while (stringResult.Length < length)
        {
            random.GetBytes(uintBuffer, offset, count);
            uint number = BitConverter.ToUInt32(uintBuffer, 0);

            if (number < max)
            {
                _ = stringResult.Append(lookupCharacters[(int)
                    (number % lookupCharacters.Length)]);
            }
        }

        return stringResult.ToString();
    }
}
