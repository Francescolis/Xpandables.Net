/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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
using System.Diagnostics.CodeAnalysis;

namespace Xpandables.Net.Operations;

/// <summary>
/// Represents a delegate that finalizes an <see cref="IOperationResult"/>.
/// </summary>
/// <param name="operationResult">The operation result to finalize.</param>
/// <returns>The finalized operation result.</returns>
public delegate IOperationResult OperationResultFinalizeDelegate(
    IOperationResult operationResult);

/// <summary>
/// Defines a mechanism for finalizing an <see cref="IOperationResult"/>.
/// </summary>
public interface IOperationResultFinalizer
{
    /// <summary>
    /// Gets or sets a value indicating whether to call the finalize method 
    /// on exception.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Finalize))]
    bool CallFinalizeOnException { get; set; }

    /// <summary>
    /// Gets or sets the delegate to finalize the operation result.
    /// </summary>
    OperationResultFinalizeDelegate? Finalize { get; set; }
}

internal sealed class OperationResultFinalizer : IOperationResultFinalizer
{
    [MemberNotNullWhen(true, nameof(Finalize))]
    public bool CallFinalizeOnException { get; set; }
    public OperationResultFinalizeDelegate? Finalize { get; set; }
}