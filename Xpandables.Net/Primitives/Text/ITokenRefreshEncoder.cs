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
namespace Xpandables.Net.Primitives.Text;

/// <summary>
///  Defines a method to encode to encode a refresh token.
/// </summary>
public interface ITokenRefreshEncoder
{
    /// <summary>
    /// Encodes a token to be used as a refresh token.
    /// </summary>
    /// <returns>An instance of <see cref="RefreshToken"/>.</returns>
    /// <exception cref="InvalidOperationException">Unable to encode a refresh 
    /// token. See inner exception.</exception>
    RefreshToken EncodeToken();
}