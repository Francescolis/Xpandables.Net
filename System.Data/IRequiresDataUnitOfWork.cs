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
namespace System.Data;

/// <summary>
/// Marker interface to indicate that a request requires automatic 
/// ADO.NET unit of work transaction management.
/// </summary>
/// <remarks>
/// <para>
/// Implement this interface to signal that the implementing request should be executed
/// within an ADO.NET transaction scope. The transaction will be committed on success
/// or rolled back on failure.
/// </para>
/// </remarks>
#pragma warning disable CA1040 // Avoid empty interfaces
public interface IRequiresDataUnitOfWork;
#pragma warning restore CA1040 // Avoid empty interfaces
