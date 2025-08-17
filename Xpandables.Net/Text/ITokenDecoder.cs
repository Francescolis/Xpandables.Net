
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
using System.Security.Claims;
using System.Text.Json;

namespace Xpandables.Net.Text;
/// <summary>
/// Interface for decoding tokens into a collection of claims.
/// </summary>
public interface ITokenDecoder
{
    /// <summary>
    /// Decodes the specified token into a collection of claims.
    /// </summary>
    /// <param name="token">The token to decode.</param>
    /// <returns>A collection of claims extracted from the token.</returns>
    IEnumerable<Claim> Decode(string token);
}

/// <summary>
/// Provides a set of static methods for token decoder.
/// </summary>
public static class TokenDecoderExtensions
{
    /// <summary>
    /// Parse the claims from the specified token.
    /// </summary>
    /// <param name="_">The token decoder to act with.</param>
    /// <param name="token">The JWT token to act on.</param>
    /// <returns>An collection of claims.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="token"/> 
    /// is null.</exception>
    /// <exception cref="FormatException">The length of 
    /// <paramref name="token"/>, ignoring white-space characters, is not 
    /// zero or a multiple of 4, or The format is invalid, or contains a 
    /// non-base-64 character, more than two padding characters, or a non-white 
    /// space-character among the padding characters.</exception>
    public static IEnumerable<Claim> DecodeUnsafe(
        this ITokenDecoder _,
        string token)
    {
        ArgumentNullException.ThrowIfNull(token);

        string payload = token.Split('.')[1];
        byte[] jsonBytes = ParseBase64WithoutPadding(payload);

        Dictionary<string, object> keyValuePairs =
            JsonSerializer
                .Deserialize<Dictionary<string, object>>(jsonBytes)!;

        return keyValuePairs
            .Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()!));
    }

    /// <summary>
    /// Decodes the token in <see cref="TokenValue"/>> and returns 
    /// a collection of claims.
    /// </summary>
    /// <param name="tokenDecoder">The token decoder to act with.</param> 
    /// <param name="token">The access token instance.</param>
    /// <returns>An collection of claims.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="tokenDecoder"/> or <paramref name="token"/> 
    /// is null.</exception>
    /// <exception cref="InvalidOperationException">Unable to decode the 
    /// specified token. See inner exception.</exception>
    public static IEnumerable<Claim> Decode(
        this ITokenDecoder tokenDecoder,
        TokenValue token)
    {
        ArgumentNullException.ThrowIfNull(tokenDecoder);

        return tokenDecoder.Decode(token.Value);
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        int padding = base64.Length % 4;

        if (padding == 2)
        {
            base64 += "==";
        }
        else if (padding == 3)
        {
            base64 += "=";
        }

        return Convert.FromBase64String(base64);
    }
}