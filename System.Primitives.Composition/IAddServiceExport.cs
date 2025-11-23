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
using System.ComponentModel.Composition;
using System.Composition;

namespace System.Primitives.Composition;

/// <summary>
/// Interface for adding service exports to the service collection.
/// <code>
/// [Export(typeof(IAddServiceExport))]
/// public class MyServiceExport : IAddServiceExport
/// </code>
/// </summary>
/// <remarks>That interface allows external libraries to register 
/// types to the services collection.
/// It's used with MEF : Managed Extensibility Framework.
/// The implementation class must be decorated with the attribute 
/// <see cref="ExportAttribute"/> attribute,
/// with <see cref="IAddServiceExport"/> type as contract type.</remarks>
public interface IAddServiceExport : IAddService
{
}
