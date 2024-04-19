
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
using System.ComponentModel;
using System.Runtime.Serialization;

using Xpandables.Net.Primitives.Text;

namespace Xpandables.Net.Operations;

/// <summary>
/// Represents an exception that holds a failed <see cref="Operation"/>.
/// Useful when you don't want to return an <see cref="Operation"/>.
/// </summary>
[Serializable]
public sealed class OperationResultException : Exception
{
    /// <summary>
    /// Constructs a new instance of the 
    /// <see cref="OperationResultException"/> class that
    /// contains the specified operation result.
    /// </summary>
    /// <param name="operation">The result for the exception.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="operation"/> is null.</exception>
    public OperationResultException(IOperationResult operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        _ = operation.StatusCode.EnsureFailureStatusCode();
        Operation = operation;
    }

    /// <summary>
    /// Gets the operation result for the exception.
    /// </summary>
    public IOperationResult Operation { get; }

    [Obsolete("Use contrcutor with IOperationResult")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    private OperationResultException(
        SerializationInfo serializationInfo,
        StreamingContext streamingContext)
        : base(serializationInfo, streamingContext)
    {
        ArgumentNullException.ThrowIfNull(serializationInfo);
        Operation = (IOperationResult)serializationInfo
            .GetValue(nameof(Operation), typeof(IOperationResult))!;
    }

    ///<inheritdoc/>
    ///<remarks>Use the constructor with 
    ///<see cref="Operation"/> parameter</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public OperationResultException()
    {
        throw new NotSupportedException();
    }

    ///<inheritdoc/>
    ///<remarks>Use the constructor with 
    ///<see cref="Operation"/> parameter</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public OperationResultException(string message) : base(message)
    {
        throw new NotSupportedException();
    }

    ///<inheritdoc/>
    ///<remarks>Use the constructor with 
    ///<see cref="Operation"/> parameter</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public OperationResultException(string message, Exception innerException)
        : base(message, innerException)
    {
        throw new NotSupportedException();
    }

    ///<inheritdoc/>
    public override string ToString() => Operation.ToJsonString();
}
