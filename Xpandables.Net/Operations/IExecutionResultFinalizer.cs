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

using Xpandables.Net.Executions;

namespace Xpandables.Net.Operations;

/// <summary>
/// Represents a delegate that finalizes an <see cref="IExecutionResult"/>.
/// </summary>
/// <param name="executionResult">The execution result to finalize.</param>
/// <returns>The finalized execution result.</returns>
public delegate IExecutionResult ExecutionResultFinalize(
    IExecutionResult executionResult);

/// <summary>
/// Defines a mechanism for finalizing an <see cref="IExecutionResult"/>.
/// </summary>
public interface IExecutionResultFinalizer
{
    /// <summary>
    /// Gets or sets a value indicating whether to call the finalize method 
    /// on exception.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Finalize))]
    bool CallFinalizeOnException { get; set; }

    /// <summary>
    /// Gets or sets the delegate to finalize the execution result.
    /// </summary>
    ExecutionResultFinalize? Finalize { get; set; }
}

internal sealed class ExecutionResultFinalizer : IExecutionResultFinalizer
{
    [MemberNotNullWhen(true, nameof(Finalize))]
    public bool CallFinalizeOnException { get; set; }
    public ExecutionResultFinalize? Finalize { get; set; }
}