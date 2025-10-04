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
using System.Diagnostics.CodeAnalysis;

using Xpandables.Net.Async;

namespace Xpandables.Net.Rests;

/// <summary>
/// Defines a contract for RESTful requests. 
/// It serves as a blueprint for implementing REST request functionalities.
/// </summary>
public interface IRestRequest
{
    /// <summary>
    /// Represents the date and time when the object was created, 
    /// set to the current UTC time at initialization.
    /// </summary>
    public DateTime CreatedAt => DateTime.UtcNow;

    /// <summary>
    /// Returns the name of the type of the current instance as a string.
    /// This is typically the class name.
    /// </summary>
    public string Name => GetType().Name;

    /// <summary>
    /// Returns the default value of the ResultType, which can be null. 
    /// It indicates the type of the result.
    /// </summary>
    public Type? ResultType => default;

    /// <summary>
    /// Indicates whether the request is a stream.
    /// </summary>
    public bool IsRequestStream => false;
}

/// <summary>
/// Defines a contract for REST requests that return a specific result type.
/// </summary>
/// <typeparam name="TResult">Represents the class type of the result expected from the REST request.</typeparam>
public interface IRestRequest<TResult> : IRestRequest
    where TResult : notnull
{   /// <summary>
    /// Returns the default value of the ResultType, which can be null. 
    /// It indicates the type of the result.
    /// </summary>
    public new Type? ResultType => typeof(TResult);

    [EditorBrowsable(EditorBrowsableState.Never)]
    Type? IRestRequest.ResultType => ResultType;
}

/// <summary>
/// Defines a contract for a request that streams data and returns an <see cref="IAsyncEnumerable{T}"/> of a specified type.
/// </summary>
/// <typeparam name="TResult">Specifies the type of result that must not be null.</typeparam>
/// <remarks>A custom implementation of <see cref="IRestResponseStreamComposer{TResult}"/> can return <see cref="IAsyncPagedEnumerable{T}"/>.</remarks>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "<Pending>")]
public interface IRestRequestStream<TResult> : IRestRequest<TResult>
    where TResult : notnull
{
    /// <summary>
    /// Indicates whether the request stream is available.
    /// </summary>
    public new bool IsRequestStream => true;

    [EditorBrowsable(EditorBrowsableState.Never)]
    bool IRestRequest.IsRequestStream => IsRequestStream;
}
