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
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json;

namespace Xpandables.Net.Operations;

/// <summary>
/// Represents an exception that occurs during an executionResult result.
/// </summary>
public sealed class ExecutionResultException : Exception
{
    private static readonly JsonSerializerOptions CachedJsonSerializerOptions =
        new() { WriteIndented = true };

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionResultException"/> 
    /// class with the specified executionResult result.
    /// </summary>
    /// <param name="executionResult">The executionResult result that caused the exception.</param>
    /// <exception cref="ArgumentNullException">Thrown when the 
    /// <paramref name="executionResult"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status 
    /// code of the <paramref name="executionResult"/> is between 200 and 299.</exception>
    public ExecutionResultException(IExecutionResult executionResult)
        : base(string.Join(
            Environment.NewLine, executionResult.Errors.SelectMany(e => e.Values)))
    {
        ArgumentNullException.ThrowIfNull(executionResult);

        if ((int)executionResult.StatusCode is >= 200 and <= 299)
        {
            throw new ArgumentOutOfRangeException(
                nameof(executionResult),
                executionResult.StatusCode,
                "The status code for exception must not be between 200 and 299.");
        }

        ExecutionResult = executionResult;
    }

    /// <summary>
    /// Gets the executionResult associated with this exception.
    /// </summary>
    public IExecutionResult ExecutionResult { get; }

    [Obsolete("Use constructor with IOperationResult")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    private ExecutionResultException(
        SerializationInfo serializationInfo,
        StreamingContext streamingContext)
        : base(serializationInfo, streamingContext)
    {
        ArgumentNullException.ThrowIfNull(serializationInfo);
        ExecutionResult = (IExecutionResult)serializationInfo
            .GetValue(nameof(ExecutionResult), typeof(IExecutionResult))!;
    }

    ///<inheritdoc/>
    ///<remarks>Use the constructor with 
    ///<see cref="ExecutionResult"/> parameter</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ExecutionResultException()
        => throw new NotSupportedException();

    ///<inheritdoc/>
    ///<remarks>Use the constructor with 
    ///<see cref="ExecutionResult"/> parameter</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ExecutionResultException(string message) : base(message)
        => throw new NotSupportedException();

    ///<inheritdoc/>
    ///<remarks>Use the constructor with 
    ///<see cref="ExecutionResult"/> parameter</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ExecutionResultException(string message, Exception innerException)
        : base(message, innerException) => throw new NotSupportedException();

    ///<inheritdoc/>
    public override string ToString() => JsonSerializer.Serialize(
        ExecutionResult, ExecutionResult.GetType(), CachedJsonSerializerOptions);
}
