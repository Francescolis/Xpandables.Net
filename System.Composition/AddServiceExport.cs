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
namespace System.Composition;

/// <summary>
/// Provides an abstract base class for adding service exports, facilitating the implementation of service export
/// functionality.
/// </summary>
/// <remarks>This class serves as a foundation for derived classes that implement specific service export
/// behaviors. It is intended to be extended by other classes that provide concrete implementations of service
/// exports.</remarks>
public abstract class AddServiceExport : AddService, IAddServiceExport;
