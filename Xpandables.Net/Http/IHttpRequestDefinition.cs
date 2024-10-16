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
namespace Xpandables.Net.Http;

/// <summary>
/// Provides with the base interface for all HTTP content request definition.
/// </summary>
public interface IHttpRequestDefinition { }

/// <summary>
/// Provides with the interface to start the HTTP request definition.
/// </summary>
public interface IHttpRequestDefinitionStart : IHttpRequestDefinition { }

/// <summary>
/// Provides with the interface to complete the built.
/// </summary>
public interface IHttpRequestDefinitionComplete : IHttpRequestDefinition { }
