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
namespace System.Collections.Generic;

/// <summary>
/// Provides extension methods for working with asynchronous paged enumerables.
/// </summary>
/// <remarks>This static class contains helper methods that extend the functionality of generic asynchronous paged sequences.</remarks>
public static class IAsyncPagedEnumerableExtensions
{
	extension<T>(IAsyncPagedEnumerable<T> source)
	{
		/// <summary>
		/// Gets the runtime type of the generic argument parameter.
		/// </summary>
		/// <returns>A <see cref="Type"/> object representing the type parameter <c>T</c>.</returns>
		public Type ArgumentType => typeof(T);
	}
}
