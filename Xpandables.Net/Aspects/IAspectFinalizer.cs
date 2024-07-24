/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
namespace Xpandables.Net.Aspects;

/// <summary>
/// Represents a marker interface that allows the class implementation to be
/// recognized as an aspect finalizer that will be called after the method 
/// invocation.
/// </summary>
public interface IAspectFinalizer : IAspect
{
    /// <summary>
    /// Gets or sets the function that will be called after the 
    /// method invocation.
    /// </summary>
    /// <remarks>In case of exception, the finilizer will receive the handled
    /// exception, otherwise, it will receive the method result when it's not
    /// a <see cref="void"/>, <see cref="Task"/> or <see cref="ValueTask"/> 
    /// method.</remarks>
    Func<object?, object?>? Finalize { get; set; }
}
