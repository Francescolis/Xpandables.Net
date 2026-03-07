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
namespace System.Data;

/// <summary>
/// Defines a contract for objects that expose their disposal state.
/// </summary>
/// <remarks>Implementing this interface allows consumers to check whether an object has been disposed before
/// performing operations that require the object to be active. This can help prevent exceptions caused by accessing
/// members of a disposed object.</remarks>
public interface IDisposableCheck
{
	/// <summary>
	/// Gets a value indicating whether the object has been disposed.
	/// </summary>
	/// <remarks>This property is useful for determining the state of the object before performing operations that
	/// require the object to be active. Accessing members of a disposed object may lead to exceptions.</remarks>
	bool IsDisposed { get; }
}
