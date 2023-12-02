
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
namespace Xpandables.Net.Http;

/// <summary>
/// An interface, when implemented in a request, will return a class representing
/// an <see cref="HttpClientAttribute"/> to be dynamically applied on the implementing class.
/// This interface takes priority over the <see cref="HttpClientAttribute"/> declaration.
/// </summary>
public interface IHttpClientAttributeBuilder
{
    /// <summary>
    /// Returns the <see cref="HttpClientAttribute"/> to be applied on the implementing instance.
    /// </summary>
    /// <param name="serviceProvider">The ambient service provider.</param>
    /// <returns>A new instance of <see cref="HttpClientAttribute"/>.</returns>
    HttpClientAttribute Build(IServiceProvider serviceProvider);
}