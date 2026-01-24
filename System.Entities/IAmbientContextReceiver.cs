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
namespace System.Entities;

/// <summary>
/// Defines a contract for components that can receive an ambient data context,
/// enabling them to participate in coordinated data operations managed by a unit of work.
/// </summary>
/// <typeparam name="TContext">The type of the context to receive.</typeparam>
public interface IAmbientContextReceiver<in TContext>
    where TContext : class
{
    /// <summary>
    /// Sets the ambient context for this component.
    /// </summary>
    /// <param name="context">The context instance to use for data operations.</param>
    void SetAmbientContext(TContext context);
}
