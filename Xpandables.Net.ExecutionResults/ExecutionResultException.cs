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
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Xpandables.Net.ExecutionResults;

/// <summary>
/// Represents an exception that is thrown when an execution result has an 
/// unsuccessful status code.
/// </summary>
public sealed class ExecutionResultException : Exception
{
    /// <summary>
    /// Gets the executionResult associated with this exception.
    /// </summary>
    public ExecutionResult ExecutionResult { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionResultException"/> 
    /// class with the specified executionResult result.
    /// </summary>
    /// <param name="executionResult">The executionResult result that caused the exception.</param>
    /// <exception cref="ArgumentNullException">Thrown when the 
    /// <paramref name="executionResult"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status 
    /// code of the <paramref name="executionResult"/> is between 200 and 299.</exception>
    public ExecutionResultException(ExecutionResult executionResult)
        : base($"Execution failed with status code: {executionResult?.StatusCode}")
    {
        ArgumentNullException.ThrowIfNull(executionResult);

        executionResult.StatusCode.EnsureFailure();

        ExecutionResult = executionResult;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionResultException"/> 
    /// class with a specified error message and execution result.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="executionResult">The execution result that caused the exception.</param>
    public ExecutionResultException(string message, ExecutionResult executionResult)
        : base(message)
    {
        ArgumentNullException.ThrowIfNull(executionResult);

        executionResult.StatusCode.EnsureFailure();

        ExecutionResult = executionResult;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionResultException"/> 
    /// class with a specified error message, execution result, and a reference 
    /// to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="executionResult">The execution result that caused the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ExecutionResultException(
        string message, ExecutionResult executionResult, Exception innerException)
        : base(message, innerException)
    {
        ArgumentNullException.ThrowIfNull(executionResult);

        executionResult.StatusCode.EnsureFailure();

        ExecutionResult = executionResult;
    }

    [Obsolete("Use constructor with ExecutionResult")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    private ExecutionResultException(
        SerializationInfo serializationInfo,
        StreamingContext streamingContext)
        : base(serializationInfo, streamingContext)
    {
        ArgumentNullException.ThrowIfNull(serializationInfo);
        ExecutionResult = (ExecutionResult)serializationInfo
            .GetValue(nameof(ExecutionResult), typeof(ExecutionResult))!;
    }

    /// <summary>
    /// Sets the <see cref="SerializationInfo"/> with information about the exception.
    /// </summary>
    /// <param name="info">The object that holds the serialized object data.</param>
    /// <param name="context">The contextual information about the source or destination.</param>
    [Obsolete("Use constructor with ExecutionResult")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ExecutionResult), ExecutionResult, typeof(ExecutionResult));
    }

    ///<inheritdoc/>
    ///<remarks>Use the constructor with 
    ///<see cref="ExecutionResult"/> parameter</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ExecutionResultException() => throw new NotSupportedException();

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
    public override string ToString() => $"{base.ToString()}{Environment.NewLine}{ExecutionResult}";
}