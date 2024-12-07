
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

namespace Xpandables.Net.Text;
/// <summary>
/// Provides functionality to encode a collection of claims into a token.
/// </summary>
public interface ITokenEncoder
{
    /// <summary>
    /// Encodes the specified claims into a token.
    /// </summary>
    /// <param name="claims">The claims to encode.</param>
    /// <returns>A <see cref="TokenValue"/> representing the encoded token.</returns>
    /// <exception cref="InvalidOperationException">Unable to encode a token.
    /// See inner exception.</exception>
    TokenValue Encode(IEnumerable<Claim> claims);

    /// <summary>
    /// Encodes a token as a refresh token.
    /// </summary>
    /// <returns>A <see cref="RefreshTokenValue"/> representing the refresh token.</returns>
    /// <exception cref="InvalidOperationException">Unable to encode a refresh token.
    /// See inner exception.</exception>
    RefreshTokenValue Encode();
}
