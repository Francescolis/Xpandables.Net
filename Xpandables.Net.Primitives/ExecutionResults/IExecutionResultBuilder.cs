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
using System.Net;

using Microsoft.Extensions.Primitives;

using Xpandables.Net.Collections;

namespace Xpandables.Net.ExecutionResults;

/// <summary>
/// Base interface for all execution result builders with common functionality.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IExecutionResultBuilderBase<out TBuilder> :
    IExecutionResultHeaderBuilder<TBuilder>,
    IExecutionResultLocationBuilder<TBuilder>,
    IExecutionResultStatusBuilder<TBuilder>,
    IExecutionResultExtensionBuilder<TBuilder>,
    IExecutionResultClearBuilder<TBuilder>
    where TBuilder : class, IExecutionResultBuilder;

/// <summary>  
/// Provides a builder interface for constructing failure execution results.  
/// </summary>  
public interface IExecutionResultFailureBuilder :
    IExecutionResultBuilderBase<IExecutionResultFailureBuilder>,
    IExecutionResultErrorBuilder<IExecutionResultFailureBuilder>,
    IExecutionResultDetailBuilder<IExecutionResultFailureBuilder>,
    IExecutionResultTitleBuilder<IExecutionResultFailureBuilder>,
    IExecutionResultMergeBuilder<IExecutionResultFailureBuilder>,
    IExecutionResultBuilder;

/// <summary>  
/// Provides a builder interface for constructing failure execution results 
/// with a specific result type.  
/// </summary>  
/// <typeparam name="TResult">The type of the result.</typeparam>  
public interface IExecutionResultFailureBuilder<TResult> :
    IExecutionResultBuilderBase<IExecutionResultFailureBuilder<TResult>>,
    IExecutionResultErrorBuilder<IExecutionResultFailureBuilder<TResult>>,
    IExecutionResultDetailBuilder<IExecutionResultFailureBuilder<TResult>>,
    IExecutionResultTitleBuilder<IExecutionResultFailureBuilder<TResult>>,
    IExecutionResultMergeBuilder<IExecutionResultFailureBuilder<TResult>>,
    IExecutionResultBuilder<TResult>;

/// <summary>
/// Interface for building a success execution result.
/// </summary>
public interface IExecutionResultSuccessBuilder :
    IExecutionResultBuilderBase<IExecutionResultSuccessBuilder>,
    IExecutionResultObjectBuilder<IExecutionResultSuccessBuilder>,
    IExecutionResultBuilder;

/// <summary>
/// Interface for building a success execution result with a specific result type.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IExecutionResultSuccessBuilder<TResult> :
    IExecutionResultBuilderBase<IExecutionResultSuccessBuilder<TResult>>,
    IExecutionResultResultBuilder<IExecutionResultSuccessBuilder<TResult>, TResult>,
    IExecutionResultBuilder<TResult>;

/// <summary>
/// Represents a method for building an <see cref="ExecutionResult"/>.
/// </summary>
public interface IExecutionResultBuilder
{
    /// <summary>
    /// Builds an instance that matches the builder information.
    /// </summary>
    /// <returns>An instance of <see cref="ExecutionResult"/>.</returns>
    ExecutionResult Build();
}

/// <summary>
/// Represents a method for building an <see cref="ExecutionResult{TResult}"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IExecutionResultBuilder<TResult> : IExecutionResultBuilder
{
    /// <summary>
    /// Builds an instance that matches the builder information.
    /// </summary>
    /// <returns>An instance of <see cref="ExecutionResult{TResult}"/>.</returns>
    new ExecutionResult<TResult> Build();

    [EditorBrowsable(EditorBrowsableState.Never)]
    ExecutionResult IExecutionResultBuilder.Build() => Build();
}

/// <summary>
/// Represents a method for merging the current execution with another execution result.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IExecutionResultMergeBuilder<out TBuilder>
    where TBuilder : class, IExecutionResultBuilder
{
    /// <summary>
    /// Merges the current execution with the specified execution.
    /// </summary>
    /// <param name="execution">The execution to merge with.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder Merge(ExecutionResult execution);
}

/// <summary>
/// Represents a method for setting the status code of an execution being built.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IExecutionResultStatusBuilder<out TBuilder>
    where TBuilder : class, IExecutionResultBuilder
{
    /// <summary>
    /// Sets the status code of the execution being built.
    /// </summary>
    /// <param name="statusCode">The status code of the execution.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithStatusCode(HttpStatusCode statusCode);
}

/// <summary>
/// Represents a method for setting the title of an execution being built.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IExecutionResultTitleBuilder<out TBuilder>
    where TBuilder : class, IExecutionResultBuilder
{
    /// <summary>
    /// Sets the title of the execution being built.
    /// </summary>
    /// <param name="title">The title of the execution.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithTitle(string title);
}

/// <summary>
/// Represents a method for setting the detail of an execution being built.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IExecutionResultDetailBuilder<out TBuilder>
   where TBuilder : class, IExecutionResultBuilder
{
    /// <summary>
    /// Sets the detail of the execution being built.
    /// </summary>
    /// <param name="detail">The detail of the execution.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithDetail(string detail);
}

/// <summary>
/// Represents a method for setting the location of an execution being built.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IExecutionResultLocationBuilder<out TBuilder>
    where TBuilder : class, IExecutionResultBuilder
{
    /// <summary>
    /// Sets the location of the execution using a URI.
    /// </summary>
    /// <param name="location">The URI location of the execution.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithLocation(Uri location);

    /// <summary>
    /// Sets the location of the execution using a string.
    /// </summary>
    /// <param name="location">The string location of the execution.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithLocation(string location);
}

/// <summary>
/// Represents a method for setting the result of an execution being built.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IExecutionResultObjectBuilder<out TBuilder>
    where TBuilder : class, IExecutionResultBuilder
{
    /// <summary>
    /// Sets the result of the execution being built.
    /// </summary>
    /// <param name="result">The result of the execution.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithResult(object? result);
}

/// <summary>
/// Represents a method for setting the result of an execution being built.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IExecutionResultResultBuilder<out TBuilder, in TResult> : IExecutionResultObjectBuilder<TBuilder>
    where TBuilder : class, IExecutionResultBuilder
{
    /// <summary>
    /// Sets the result of the execution being built.
    /// </summary>
    /// <param name="result">The result of the execution.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithResult(TResult result);

    /// <summary>
    /// Sets the result of the operation with a specified value, ensuring it is of the correct type.
    /// </summary>
    /// <param name="result">The provided value must match the expected type for successful processing.</param>
    /// <returns>Returns an instance of the builder with the updated result.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided value does not match the expected type.</exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    new TBuilder WithResult(object? result) => result switch
    {
        null => WithResult(default!),
        TResult typedResult => WithResult(typedResult),
        _ => throw new ArgumentException($"The result must be of type {typeof(TResult)}.", nameof(result))
    };
}

/// <summary>
/// Represents a method for setting headers of an execution being built.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IExecutionResultHeaderBuilder<out TBuilder>
    where TBuilder : class, IExecutionResultBuilder
{
    /// <summary>
    /// Sets a header for the execution being built.
    /// </summary>
    /// <param name="key">The header key.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithHeader(string key, string value);

    /// <summary>
    /// Sets a header with multiple values for the execution being built.
    /// </summary>
    /// <param name="key">The header key.</param>
    /// <param name="values">The header values.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithHeader(string key, params string[] values);

    /// <summary>
    /// Sets multiple headers for the execution being built.
    /// </summary>
    /// <param name="headers">The headers dictionary.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithHeaders(IDictionary<string, string> headers);

    /// <summary>
    /// Sets multiple headers for the execution being built.
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
    TBuilder WithHeaders(IReadOnlyDictionary<string, StringValues> headers);
}

/// <summary>
/// Represents a method for building an <see cref="ExecutionResult"/> with error details.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IExecutionResultErrorBuilder<out TBuilder>
    where TBuilder : class, IExecutionResultBuilder
{
    /// <summary>
    /// Adds an error with the specified key and error message.
    /// </summary>
    /// <param name="key">The key of the error.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithError(string key, string errorMessage);

    /// <summary>
    /// Adds an error with the specified key and multiple error messages.
    /// </summary>
    /// <param name="key">The key of the error.</param>
    /// <param name="errorMessages">The error messages.</param>
    /// <returns>The current builder instance.</returns>
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
    TBuilder WithError(string key, in StringValues errorMessages);

    /// <summary>
    /// Adds an exception as an error.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithException(Exception exception);
}

/// <summary>
/// Represents a method for building an <see cref="ExecutionResult"/> with extensions.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IExecutionResultExtensionBuilder<out TBuilder>
    where TBuilder : class, IExecutionResultBuilder
{
    /// <summary>
    /// Adds an extension with the specified key and value.
    /// </summary>
    /// <param name="key">The key of the extension.</param>
    /// <param name="value">The value of the extension.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithExtension(string key, string value);

    /// <summary>
    /// Adds an extension with the specified key and multiple values.
    /// </summary>
    /// <param name="key">The key of the extension.</param>
    /// <param name="values">The values of the extension.</param>
    /// <returns>The current builder instance.</returns>
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
    TBuilder WithExtensions(IReadOnlyDictionary<string, StringValues> extensions);
}

/// <summary>
/// Represents a method for clearing various elements of an execution being built.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IExecutionResultClearBuilder<out TBuilder>
    where TBuilder : class, IExecutionResultBuilder
{
    /// <summary>
    /// Clears all errors from the execution being built.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    TBuilder ClearErrors();

    /// <summary>
    /// Clears all headers from the execution being built.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    TBuilder ClearHeaders();

    /// <summary>
    /// Clears all extensions from the execution being built.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    TBuilder ClearExtensions();

    /// <summary>
    /// Clears all elements from the execution being built.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    TBuilder ClearAll();
}
