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

namespace System;

/// <summary>
/// Represents the base type for operation results, providing a common abstraction for success and failure outcomes.
/// </summary>
/// <remarks>Derived types typically encapsulate additional information about the result, such as error details or
/// returned values. Use this type as a base for modeling the outcome of operations that may succeed or fail.</remarks>
public abstract record Result;

/// <summary>
/// Represents the result of an operation that produces a value of the specified type, indicating success or failure.
/// </summary>
/// <remarks>Use this type to encapsulate the outcome of an operation that may succeed or fail, along with an
/// associated value when successful. This pattern enables callers to handle both success and failure cases in a
/// type-safe manner without relying on exceptions for control flow.</remarks>
/// <typeparam name="TValue">The type of the value returned by a successful operation.</typeparam>
public abstract record Result<TValue> : Result;
