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
/// Represents an exception that occurs during an operation result.
/// </summary>
public sealed class OperationResultException : Exception
{
    private static readonly JsonSerializerOptions CachedJsonSerializerOptions =
        new() { WriteIndented = true };

    /// <summary>
    /// Initializes a new instance of the <see cref="OperationResultException"/> 
    /// class with the specified operation result.
    /// </summary>
    /// <param name="operationResult">The operation result that caused the exception.</param>
    /// <exception cref="ArgumentNullException">Thrown when the 
    /// <paramref name="operationResult"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status 
    /// code of the <paramref name="operationResult"/> is between 200 and 299.</exception>
    public OperationResultException(IOperationResult operationResult)
        : base(string.Join(
            Environment.NewLine, operationResult.Errors.SelectMany(e => e.Values)))
    {
        ArgumentNullException.ThrowIfNull(operationResult);

        if ((int)operationResult.StatusCode is >= 200 and <= 299)
        {
            throw new ArgumentOutOfRangeException(
                nameof(operationResult),
                operationResult.StatusCode,
                "The status code for exception must not be between 200 and 299.");
        }

        OperationResult = operationResult;
    }

    /// <summary>
    /// Gets the operation result associated with this exception.
    /// </summary>
    public IOperationResult OperationResult { get; }

    [Obsolete("Use constructor with IOperationResult")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    private OperationResultException(
        SerializationInfo serializationInfo,
        StreamingContext streamingContext)
        : base(serializationInfo, streamingContext)
    {
        ArgumentNullException.ThrowIfNull(serializationInfo);
        OperationResult = (IOperationResult)serializationInfo
            .GetValue(nameof(OperationResult), typeof(IOperationResult))!;
    }

    ///<inheritdoc/>
    ///<remarks>Use the constructor with 
    ///<see cref="OperationResult"/> parameter</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public OperationResultException()
        => throw new NotSupportedException();

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
    public override string ToString() => JsonSerializer.Serialize(
        OperationResult, OperationResult.GetType(), CachedJsonSerializerOptions);
}
