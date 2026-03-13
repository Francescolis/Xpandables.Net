/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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
namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Provides an abstract base class for implementing the <see cref="IUseService"/> interface, allowing derived classes to
/// implement custom middleware configuration logic for a <see cref="WebApplication"/>.
/// </summary>
public abstract class UseService : IUseService
{
	/// <inheritdoc/>
	public abstract void UseServices(WebApplication application);
}
