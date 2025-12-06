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

namespace System.Results;

/// <summary>
/// Represents an exception that is thrown when a result has an unsuccessful status code.
/// </summary>
public sealed class ResultException : Exception
{
    /// <summary>
    /// Gets the Result associated with this exception.
    /// </summary>
    public Result Result { get; }

    /// <summary>
    /// Initializes a new instance of the ResultException class using the specified result. This
    /// exception indicates that an operation has failed, as represented by the provided Result.
    /// </summary>
    /// <remarks>The exception message includes the status code from the provided Result. The
    /// Result property will reference the same instance passed to this constructor.</remarks>
    /// <param name="result">The Result instance that describes the failed operation. Must not be null and must represent a failure
    /// status.</param>
    /// <exception cref="ArgumentNullException">Thrown if the operation parameter is null.</exception>
    public ResultException(Result result)
        : base($"Execution failed with status code: {result?.StatusCode ?? throw new ArgumentNullException(nameof(result))}")
    {
        result.StatusCode.EnsureFailure();
        Result = result;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResultException"/> 
    /// class with a specified error message and result.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="result">The result that caused the exception.</param>
    public ResultException(string message, Result result)
        : base(message)
    {
        ArgumentNullException.ThrowIfNull(result);
        result.StatusCode.EnsureFailure();
        Result = result;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResultException"/> 
    /// class with a specified error message, result, and a reference 
    /// to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="result">The result that caused the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ResultException(
        string message, Result result, Exception innerException)
        : base(message, innerException)
    {
        ArgumentNullException.ThrowIfNull(result);
        result.StatusCode.EnsureFailure();
        Result = result;
    }

    [Obsolete("Use constructor with Result")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    private ResultException(
        SerializationInfo serializationInfo,
        StreamingContext streamingContext)
        : base(serializationInfo, streamingContext)
    {
        ArgumentNullException.ThrowIfNull(serializationInfo);
        Result = (Result)serializationInfo
            .GetValue(nameof(Result), typeof(Result))!;
    }

    /// <summary>
    /// Sets the <see cref="SerializationInfo"/> with information about the exception.
    /// </summary>
    /// <param name="info">The object that holds the serialized object data.</param>
    /// <param name="context">The contextual information about the source or destination.</param>
    [Obsolete("Use constructor with Result")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(Result), Result, typeof(Result));
    }

    ///<inheritdoc/>
    ///<remarks>Use the constructor with 
    ///<see cref="Result"/> parameter</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ResultException() => throw new NotSupportedException();

    ///<inheritdoc/>
    ///<remarks>Use the constructor with 
    ///<see cref="Result"/> parameter</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ResultException(string message) : base(message)
        => throw new NotSupportedException();

    ///<inheritdoc/>
    ///<remarks>Use the constructor with 
    ///<see cref="Result"/> parameter</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ResultException(string message, Exception innerException)
        : base(message, innerException) => throw new NotSupportedException();

    ///<inheritdoc/>
    public override string ToString() => $"{base.ToString()}{Environment.NewLine}{Result}";
}