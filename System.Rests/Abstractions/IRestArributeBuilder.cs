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
/// Defines a builder for creating <see cref="RestAttribute"/> at runtime.
/// </summary>
/// <remarks>This interface take priority over the static <see cref="RestAttribute"/>.</remarks>
public interface IRestAttributeBuilder : IRestRequest
{
    /// <summary>
    /// Creates a RestAttribute instance using the provided service provider.
    /// </summary>
    /// <param name="serviceProvider">An object that provides access to application services.</param>
    /// <returns>An instance of RestAttribute configured with the specified services.</returns>
    RestAttribute Build(IServiceProvider serviceProvider);
}
