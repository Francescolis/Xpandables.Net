
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
using Xpandables.Net.Collections;

namespace Xpandables.Net.Http;
/// <summary>
/// Interface for building HTTP Header requests.
/// </summary>
/// <remarks>if you want to set the header model name, you can override the 
/// <see cref="GetHeaderModelName"/> method.</remarks>
public interface IHttpRequestHeader : IRequestDefinition
{
    /// <summary>
    /// Gets the collection of headers.
    /// </summary>
    /// <returns>An <see cref="ElementCollection"/> containing the headers.</returns>
    ElementCollection GetHeaders();

    /// <summary>
    /// Gets the name of the header model.
    /// </summary>
    /// <returns>A string representing the name of the header model, 
    /// or null if not set.</returns>
    public string? GetHeaderModelName() => null;
}
