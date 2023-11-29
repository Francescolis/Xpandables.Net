
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
using System.Security.Claims;

namespace Xpandables.Net.Primitives.Text;

/// <summary>
///  Defines a set of methods that can be used to build a refresh token.
/// </summary>
public interface ITokenRefreshProcessor
{
    /// <summary>
    /// Writes a string token to be used as a refresh token.
    /// </summary>
    /// <returns>An instance of refresh token if OK.</returns>
    /// <exception cref="InvalidOperationException">Unable to write a refresh token. See inner exception.</exception>
    RefreshToken WriteToken();

    /// <summary>
    /// Returns the collection of claims from the expired token.
    /// </summary>
    /// <param name="expiredToken">The expired token string.</param>
    /// <returns>An collection of claims if OK.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="expiredToken"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Unable to read claims from the specified token. See inner exception.</exception>
    IEnumerable<Claim> ReadExpiredToken(string expiredToken);
}