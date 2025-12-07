/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
namespace System.Rests.Abstractions;

/// <summary>
/// Provides a method to retrieve a RestAttribute based on a given IRestRequest. 
/// This allows for the customization of REST attributes.
/// </summary>
public interface IRestAttributeProvider
{
    /// <summary>
    /// Retrieves the RestAttribute associated with a specific REST request.
    /// </summary>
    /// <param name="request">The input represents a REST request for which the attribute is being retrieved.</param>
    /// <returns>Returns the corresponding RestAttribute for the provided request.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the request is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the request is not decorated with RestAttribute or 
    /// does not implement IRestAttributeBuilder.</exception>
    RestAttribute GetRestAttribute(IRestRequest request);
}