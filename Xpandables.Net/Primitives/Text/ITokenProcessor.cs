
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
///  Defines a set of methods that can be used to build a token from a collection of claims
///  and return back this collection from that token.
/// </summary>
public interface ITokenProcessor
{
    /// <summary>
    /// Uses the collection of claims to build a token.
    /// </summary>
    /// <param name="claims">collection of claims to be used to build token string.</param>
    /// <returns>An instance of <see cref="AccessToken"/> token if OK.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="claims"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Unable to write a token with the specified claims. See inner exception.</exception>
    AccessToken WriteToken(IEnumerable<Claim> claims);

    /// <summary>
    /// Returns without validation, the collection of claims from the specified token.
    /// </summary>
    /// <param name="token">The token string.</param>
    /// <returns>An collection of claims if OK.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="token"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Unable to read claims from the specified token. See inner exception.</exception>
    IEnumerable<Claim> ReadUnsafeToken(string token);

    /// <summary>
    /// Returns after validation the collection of claims from the specified token.
    /// </summary>
    /// <param name="token">The token string.</param>
    /// <returns>An collection of claims if OK.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="token"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Unable to read claims from the specified token. See inner exception.</exception>
    IEnumerable<Claim> ReadToken(string token);

    /// <summary>
    /// Returns after validation the collection of claims from the specified access token.
    /// </summary>
    /// <param name="token">The access token instance.</param>
    /// <returns>An collection of claims if OK.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="token"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Unable to write a token with the specified claims. See inner exception.</exception>
    public virtual IEnumerable<Claim> ReadToken(AccessToken token)
    {
        ArgumentNullException.ThrowIfNull(token);

        return ReadToken(token.Value);
    }
}