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

namespace System.ExecutionResults;

/// <summary>
/// Represents an exception that is thrown when an operation result has an 
/// unsuccessful status code.
/// </summary>
public sealed class OperationResultException : Exception
{
    /// <summary>
    /// Gets the executionResult associated with this exception.
    /// </summary>
    public OperationResult OperationResult { get; }

    /// <summary>
    /// Initializes a new instance of the OperationResultException class using the specified operation result. This
    /// exception indicates that an operation has failed, as represented by the provided OperationResult.
    /// </summary>
    /// <remarks>The exception message includes the status code from the provided OperationResult. The
    /// OperationResult property will reference the same instance passed to this constructor.</remarks>
    /// <param name="operation">The OperationResult instance that describes the failed operation. Must not be null and must represent a failure
    /// status.</param>
    /// <exception cref="ArgumentNullException">Thrown if the operation parameter is null.</exception>
    public OperationResultException(OperationResult operation)
        : base($"Execution failed with status code: {operation?.StatusCode ?? throw new ArgumentNullException(nameof(operation))}")
    {
        operation.StatusCode.EnsureFailure();
        OperationResult = operation;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OperationResultException"/> 
    /// class with a specified error message and operation result.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="operation">The operation result that caused the exception.</param>
    public OperationResultException(string message, OperationResult operation)
        : base(message)
    {
        ArgumentNullException.ThrowIfNull(operation);
        operation.StatusCode.EnsureFailure();
        OperationResult = operation;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OperationResultException"/> 
    /// class with a specified error message, operation result, and a reference 
    /// to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="operation">The operation result that caused the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public OperationResultException(
        string message, OperationResult operation, Exception innerException)
        : base(message, innerException)
    {
        ArgumentNullException.ThrowIfNull(operation);
        operation.StatusCode.EnsureFailure();
        OperationResult = operation;
    }

    [Obsolete("Use constructor with ExecutionResult")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    private OperationResultException(
        SerializationInfo serializationInfo,
        StreamingContext streamingContext)
        : base(serializationInfo, streamingContext)
    {
        ArgumentNullException.ThrowIfNull(serializationInfo);
        OperationResult = (OperationResult)serializationInfo
            .GetValue(nameof(OperationResult), typeof(OperationResult))!;
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
        info.AddValue(nameof(OperationResult), OperationResult, typeof(OperationResult));
    }

    ///<inheritdoc/>
    ///<remarks>Use the constructor with 
    ///<see cref="OperationResult"/> parameter</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public OperationResultException() => throw new NotSupportedException();

    ///<inheritdoc/>
    ///<remarks>Use the constructor with 
    ///<see cref="OperationResult"/> parameter</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public OperationResultException(string message) : base(message)
        => throw new NotSupportedException();

    ///<inheritdoc/>
    ///<remarks>Use the constructor with 
    ///<see cref="OperationResult"/> parameter</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public OperationResultException(string message, Exception innerException)
        : base(message, innerException) => throw new NotSupportedException();

    ///<inheritdoc/>
    public override string ToString() => $"{base.ToString()}{Environment.NewLine}{OperationResult}";
}