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
