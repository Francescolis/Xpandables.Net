
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
using System.Security.Claims;

namespace Xpandables.Net.Primitives.Text;

/// <summary>
///  Defines a method to decode a collection of claims from a token.
/// </summary>
public interface ITokenDecoder
{
    /// <summary>
    /// Decodes the token and returns a collection of claims.
    /// </summary>
    /// <param name="token">The token string to act on.</param>
    /// <returns>An collection of claims.</returns>
    /// <exception cref="ArgumentNullException">
    /// The <paramref name="token"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Unable to decode the 
    /// specified token. See inner exception.</exception>
    IEnumerable<Claim> DecodeToken(string token);
}