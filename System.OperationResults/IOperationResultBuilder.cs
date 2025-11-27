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
using System.Collections;
using System.ComponentModel;
using System.Net;

using Microsoft.Extensions.Primitives;

namespace System.OperationResults;

/// <summary>
/// Base interface for all operation result builders with common functionality.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IOperationResultBuilderBase<out TBuilder> :
    IOperationResultHeaderBuilder<TBuilder>,
    IOperationResultLocationBuilder<TBuilder>,
    IOperationResultStatusBuilder<TBuilder>,
    IOperationResultExtensionBuilder<TBuilder>,
    IOperationResultClearBuilder<TBuilder>
    where TBuilder : class, IOperationResultBuilder;

/// <summary>  
/// Provides a builder interface for constructing failure operation results.  
/// </summary>  
public interface IOperationResultFailureBuilder :
    IOperationResultBuilderBase<IOperationResultFailureBuilder>,
    IOperationResultErrorBuilder<IOperationResultFailureBuilder>,
    IOperationResultDetailBuilder<IOperationResultFailureBuilder>,
    IOperationResultTitleBuilder<IOperationResultFailureBuilder>,
    IOperationResultMergeBuilder<IOperationResultFailureBuilder>,
    IOperationResultBuilder;

/// <summary>  
/// Provides a builder interface for constructing failure operation results 
/// with a specific result type.  
/// </summary>  
/// <typeparam name="TResult">The type of the result.</typeparam>  
public interface IOperationResultFailureBuilder<TResult> :
    IOperationResultBuilderBase<IOperationResultFailureBuilder<TResult>>,
    IOperationResultErrorBuilder<IOperationResultFailureBuilder<TResult>>,
    IOperationResultDetailBuilder<IOperationResultFailureBuilder<TResult>>,
    IOperationResultTitleBuilder<IOperationResultFailureBuilder<TResult>>,
    IOperationResultMergeBuilder<IOperationResultFailureBuilder<TResult>>,
    IOperationResultBuilder<TResult>;

/// <summary>
/// Interface for building a success operation result.
/// </summary>
public interface IOperationResultSuccessBuilder :
    IOperationResultBuilderBase<IOperationResultSuccessBuilder>,
    IOperationResultObjectBuilder<IOperationResultSuccessBuilder>,
    IOperationResultBuilder;

/// <summary>
/// Interface for building a success operation result with a specific result type.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IOperationResultSuccessBuilder<TResult> :
    IOperationResultBuilderBase<IOperationResultSuccessBuilder<TResult>>,
    IOperationResultResultBuilder<IOperationResultSuccessBuilder<TResult>, TResult>,
    IOperationResultBuilder<TResult>;

/// <summary>
/// Represents a method for building an <see cref="OperationResult"/>.
/// </summary>
public interface IOperationResultBuilder
{
    /// <summary>
    /// Builds an instance that matches the builder information.
    /// </summary>
    /// <returns>An instance of <see cref="OperationResult"/>.</returns>
    OperationResult Build();
}

/// <summary>
/// Represents a method for building an <see cref="OperationResult{TResult}"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IOperationResultBuilder<TResult> : IOperationResultBuilder
{
    /// <summary>
    /// Builds an instance that matches the builder information.
    /// </summary>
    /// <returns>An instance of <see cref="OperationResult{TResult}"/>.</returns>
    new OperationResult<TResult> Build();

    [EditorBrowsable(EditorBrowsableState.Never)]
    OperationResult IOperationResultBuilder.Build() => Build().ToOperationResult();
}

/// <summary>
/// Represents a method for merging the current operation with another operation result.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IOperationResultMergeBuilder<out TBuilder>
    where TBuilder : class, IOperationResultBuilder
{
    /// <summary>
    /// Merges the current operation with the specified operation.
    /// </summary>
    /// <param name="operation">The operation to merge with.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder Merge(OperationResult operation);
}

/// <summary>
/// Represents a method for setting the status code of an operation being built.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IOperationResultStatusBuilder<out TBuilder>
    where TBuilder : class, IOperationResultBuilder
{
    /// <summary>
    /// Sets the status code of the operation being built.
    /// </summary>
    /// <param name="statusCode">The status code of the operation.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithStatusCode(HttpStatusCode statusCode);
}

/// <summary>
/// Represents a method for setting the title of an operation being built.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IOperationResultTitleBuilder<out TBuilder>
    where TBuilder : class, IOperationResultBuilder
{
    /// <summary>
    /// Sets the title of the operation being built.
    /// </summary>
    /// <param name="title">The title of the operation.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided title is null.</exception>
    TBuilder WithTitle(string title);
}

/// <summary>
/// Represents a method for setting the detail of an operation being built.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IOperationResultDetailBuilder<out TBuilder>
   where TBuilder : class, IOperationResultBuilder
{
    /// <summary>
    /// Sets the detail of the operation being built.
    /// </summary>
    /// <param name="detail">The detail of the operation.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided detail is null.</exception>
    TBuilder WithDetail(string detail);
}

/// <summary>
/// Represents a method for setting the location of an operation being built.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IOperationResultLocationBuilder<out TBuilder>
    where TBuilder : class, IOperationResultBuilder
{
    /// <summary>
    /// Sets the location of the operation using a URI.
    /// </summary>
    /// <param name="location">The URI location of the operation.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided location is null.</exception>
    TBuilder WithLocation(Uri location);

    /// <summary>
    /// Sets the location of the operation using a string.
    /// </summary>
    /// <param name="location">The string location of the operation.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided location is null.</exception>
    TBuilder WithLocation(string location);
}

/// <summary>
/// Represents a method for setting the result of an operation being built.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IOperationResultObjectBuilder<out TBuilder>
    where TBuilder : class, IOperationResultBuilder
{
    /// <summary>
    /// Sets the result of the operation being built.
    /// </summary>
    /// <param name="result">The result of the operation.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided result is null.</exception>
    TBuilder WithResult(object result);
}

/// <summary>
/// Represents a method for setting the result of an operation being built.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IOperationResultResultBuilder<out TBuilder, in TResult> : IOperationResultObjectBuilder<TBuilder>
    where TBuilder : class, IOperationResultBuilder
{
    /// <summary>
    /// Sets the result of the operation being built.
    /// </summary>
    /// <param name="result">The result of the operation.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided result is null.</exception>
    TBuilder WithResult(TResult result);

    /// <summary>
    /// Sets the result of the operation with a specified value, ensuring it is of the correct type.
    /// </summary>
    /// <param name="result">The provided value must match the expected type for successful processing.</param>
    /// <returns>Returns an instance of the builder with the updated result.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided value does not match the expected type.</exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    new TBuilder WithResult(object result) => result switch
    {
        TResult typedResult => WithResult(typedResult),
        _ => throw new ArgumentException($"The result must be of type {typeof(TResult)}.", nameof(result))
    };
}

/// <summary>
/// Represents a method for setting headers of an operation being built.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IOperationResultHeaderBuilder<out TBuilder>
    where TBuilder : class, IOperationResultBuilder
{
    /// <summary>
    /// Sets a header for the operation being built.
    /// </summary>
    /// <param name="key">The header key.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided key or value is null.</exception>
    TBuilder WithHeader(string key, string value);

    /// <summary>
    /// Sets a header with multiple values for the operation being built.
    /// </summary>
    /// <param name="key">The header key.</param>
    /// <param name="values">The header values.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided key or values are null.</exception>
    TBuilder WithHeader(string key, params string[] values);

    /// <summary>
    /// Sets multiple headers for the operation being built.
    /// </summary>
    /// <param name="headers">The headers dictionary.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided headers are null.</exception>
    TBuilder WithHeaders(IDictionary<string, string> headers);

    /// <summary>
    /// Sets multiple headers for the operation being built.
    /// </summary>
    /// <param name="headers">The headers collection.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithHeaders(ElementCollection headers);

    /// <summary>
    /// Adds the specified HTTP headers to the builder and returns a new instance with the updated headers.
    /// </summary>
    /// <param name="headers">A read-only dictionary containing the HTTP headers to add. Each key represents a header name, and each value
    /// contains one or more header values. Cannot be null.</param>
    /// <returns>A new builder instance that includes the specified headers.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided headers are null.</exception>
    TBuilder WithHeaders(IReadOnlyDictionary<string, StringValues> headers);
}

/// <summary>
/// Represents a method for building an <see cref="OperationResult"/> with error details.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IOperationResultErrorBuilder<out TBuilder>
    where TBuilder : class, IOperationResultBuilder
{
    /// <summary>
    /// Adds an error with the specified key and error message.
    /// </summary>
    /// <param name="key">The key of the error.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided key or error message is null.</exception>
    TBuilder WithError(string key, string errorMessage);

    /// <summary>
    /// Adds an error with the specified key and multiple error messages.
    /// </summary>
    /// <param name="key">The key of the error.</param>
    /// <param name="errorMessages">The error messages.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided key or error messages are null.</exception>
    TBuilder WithError(string key, params string[] errorMessages);

    /// <summary>
    /// Adds an error entry.
    /// </summary>
    /// <param name="entry">The error entry.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithError(ElementEntry entry);

    /// <summary>
    /// Adds multiple errors from a dictionary.
    /// </summary>
    /// <param name="errors">The errors dictionary.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided errors are null.</exception>
    TBuilder WithErrors(IDictionary<string, string> errors);

    /// <summary>
    /// Adds multiple errors from a collection.
    /// </summary>
    /// <param name="errors">The errors collection.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithErrors(ElementCollection errors);

    /// <summary>
    /// Adds the specified collection of error entries to the builder and returns a new builder instance with these
    /// errors included.
    /// </summary>
    /// <param name="errors">A read-only span containing the error entries to associate with the builder. Each entry represents an individual
    /// error to be tracked.</param>
    /// <returns>A new builder instance that includes the specified error entries.</returns>
    TBuilder WithErrors(ReadOnlySpan<ElementEntry> errors);

    /// <summary>
    /// Adds one or more error messages associated with the specified key to the builder and returns the updated builder
    /// instance.
    /// </summary>
    /// <param name="key">The key that identifies the field or property to associate with the error messages. Cannot be null or empty.</param>
    /// <param name="errorMessages">A collection of error messages to add for the specified key. Must not be empty.</param>
    /// <returns>The builder instance with the specified error messages added for the given key.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided key or error messages are null.</exception>
    TBuilder WithError(string key, in StringValues errorMessages);

    /// <summary>
    /// Adds an exception as an error.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided exception is null.</exception>
    TBuilder WithException(Exception exception);
}

/// <summary>
/// Represents a method for building an <see cref="OperationResult"/> with extensions.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IOperationResultExtensionBuilder<out TBuilder>
    where TBuilder : class, IOperationResultBuilder
{
    /// <summary>
    /// Adds an extension with the specified key and value.
    /// </summary>
    /// <param name="key">The key of the extension.</param>
    /// <param name="value">The value of the extension.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided key or value is null.</exception>
    TBuilder WithExtension(string key, string value);

    /// <summary>
    /// Adds an extension with the specified key and multiple values.
    /// </summary>
    /// <param name="key">The key of the extension.</param>
    /// <param name="values">The values of the extension.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided key or values are null.</exception>
    TBuilder WithExtension(string key, params string[] values);

    /// <summary>
    /// Adds an extension entry.
    /// </summary>
    /// <param name="extension">The extension entry.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithExtension(ElementEntry extension);

    /// <summary>
    /// Adds multiple extensions from a dictionary.
    /// </summary>
    /// <param name="extensions">The extensions dictionary.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided extensions are null.</exception>
    TBuilder WithExtensions(IDictionary<string, string> extensions);

    /// <summary>
    /// Adds multiple extensions from a collection.
    /// </summary>
    /// <param name="extensions">The extensions collection.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithExtensions(ElementCollection extensions);

    /// <summary>
    /// Adds the specified extensions to the builder and returns the updated builder instance.
    /// </summary>
    /// <remarks>If an extension key already exists, its value will be replaced with the new value from the
    /// dictionary. This method enables fluent configuration of OpenAPI extensions.</remarks>
    /// <param name="extensions">A read-only dictionary containing extension key-value pairs to associate with the builder. Keys must be non-null
    /// and unique within the dictionary.</param>
    /// <returns>The builder instance with the provided extensions applied.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided extensions are null.</exception>
    TBuilder WithExtensions(IReadOnlyDictionary<string, StringValues> extensions);
}

/// <summary>
/// Represents a method for clearing various elements of an operation being built.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IOperationResultClearBuilder<out TBuilder>
    where TBuilder : class, IOperationResultBuilder
{
    /// <summary>
    /// Clears all errors from the operation being built.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    TBuilder ClearErrors();

    /// <summary>
    /// Clears all headers from the operation being built.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    TBuilder ClearHeaders();

    /// <summary>
    /// Clears all extensions from the operation being built.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    TBuilder ClearExtensions();

    /// <summary>
    /// Clears all elements from the operation being built.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    TBuilder ClearAll();
}
