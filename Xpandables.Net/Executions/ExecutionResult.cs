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
using System.Net;
using System.Text.Json.Serialization;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Executions;

/// <summary>
/// Represents the result of an execution, including status code, title, detail,
/// location, result, errors, headers, extensions, and status.
/// </summary>
internal record ExecutionResult : IExecutionResult
{
    [JsonConstructor]
    internal ExecutionResult() { }

    /// <inheritdoc/>
    public required HttpStatusCode StatusCode { get; init; } = HttpStatusCode.OK;

    /// <inheritdoc/>
    [MaybeNull, AllowNull]
    public string Title { get; init; }

    /// <inheritdoc/>
    [MaybeNull, AllowNull]
    public string Detail { get; init; }

    /// <inheritdoc/>
    [MaybeNull, AllowNull]
    public Uri Location { get; init; }

    /// <inheritdoc/>
    [MaybeNull, AllowNull]
    public object Result { get; init; }

    /// <inheritdoc/>
    public ElementCollection Errors { get; init; } = [];

    /// <inheritdoc/>
    public ElementCollection Headers { get; init; } = [];

    /// <inheritdoc/>
    public ElementCollection Extensions { get; init; } = [];
}

/// <summary>  
/// Represents the result of an execution with a specific result type, including 
/// status code, title, detail,  location, result, errors, headers, extensions, 
/// and status.  
/// </summary>  
/// <typeparam name="TResult">The type of the result object.</typeparam>  
internal record ExecutionResult<TResult> : ExecutionResult, IExecutionResult<TResult>
{
    [JsonConstructor]
    internal ExecutionResult() { }

    /// <inheritdoc/>
    [MaybeNull, AllowNull]
    public new TResult Result
    {
        get => (TResult?)base.Result;
        init => base.Result = value;
    }
}