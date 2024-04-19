
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

// Ignore Spelling: Finalizer

using System.Diagnostics.CodeAnalysis;

using Xpandables.Net.Primitives;

namespace Xpandables.Net.Operations;

/// <summary>
/// Defines the action that get applied and the end of a process 
/// before the <see cref="IOperationResult"/> is returned to the caller.
/// In order to be activated, the target event should implement 
/// the <see cref="IOperationFinalizerDecorator"/> interface, 
/// the target handling class should reference the current interface 
/// to set the delegate.
/// </summary>
/// <remarks>Be aware of the fact that the finalizer did not get called
/// in case of exception. If you want so, set the property 
/// <see cref="CallFinalizerOnException"/> to <see langword="true"/>.</remarks>
public interface IOperationFinalizer
{
    /// <summary>
    /// Defines whether the finalizer should be called in case of exception.
    /// </summary>
    /// <remarks>If set to <see langword="true"/>, the finalizer is responsible
    /// to return the right result in the expected type and
    /// must be defined.</remarks>
    [MemberNotNullWhen(true, nameof(Finalizer))]
    bool CallFinalizerOnException { get; set; }

    /// <summary>
    /// Defines the delegate that allows to finalize the operation result.
    /// </summary>
    Func<IOperationResult, IOperationResult>? Finalizer { get; set; }
}
