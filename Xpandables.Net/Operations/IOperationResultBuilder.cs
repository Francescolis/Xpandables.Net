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
using System.Net;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Operations;

/// <summary>
/// Interface for building an <see cref="IOperationResult"/>.
/// </summary>
public interface IBuilder
{
    /// <summary>
    /// Builds an instance that matches the builder information.
    /// </summary>
    /// <returns>An instance of <see cref="IOperationResult"/>.</returns>
    IOperationResult Build();
}

/// <summary>
/// Interface for building an <see cref="IOperationResult{TResult}"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IBuilder<TResult> : IBuilder
{
    /// <summary>
    /// Builds an instance that matches the builder information.
    /// </summary>
    /// <returns>An instance of <see cref="IOperationResult{TResult}"/>.</returns>
    new IOperationResult<TResult> Build();

    [EditorBrowsable(EditorBrowsableState.Never)]
    IOperationResult IBuilder.Build() => Build();
}

/// <summary>
/// Interface for merging the current operation with another operation.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IMergeBuilder<out TBuilder>
    where TBuilder : class, IBuilder
{
    /// <summary>
    /// Merges the current operation with the specified operation.
    /// </summary>
    /// <param name="operation">The operation to merge with.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder Merge(IOperationResult operation);
}

/// <summary>
/// Interface for setting the status code of an operation being built.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IStatusBuilder<out TBuilder>
    where TBuilder : class, IBuilder
{
    /// <summary>
    /// Sets the status code of the operation being built.
    /// </summary>
    /// <param name="statusCode">The status code of the operation.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithStatusCode(HttpStatusCode statusCode);
}

/**
 * <summary>
 * Interface for setting the title of an operation being built.
 * </summary>
 * <typeparam name="TBuilder">The type of the builder.</typeparam>
 */
public interface ITitleBuilder<out TBuilder>
    where TBuilder : class, IBuilder
{
    /**
     * <summary>
     * Sets the title of the operation being built.
     * </summary>
     * <param name="title">The title of the operation.</param>
     * <returns>The current builder instance.</returns>
     */
    TBuilder WithTitle(string title);
}

/// <summary>
/// Interface for setting the detail of an operation being built.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IDetailBuilder<out TBuilder>
   where TBuilder : class, IBuilder
{
    /// <summary>
    /// Sets the detail of the operation being built.
    /// </summary>
    /// <param name="detail">The detail of the operation.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithDetail(string detail);
}

/// <summary>
/// Interface for setting the location of an operation being built.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface ILocationBuilder<out TBuilder>
    where TBuilder : class, IBuilder
{
    /// <summary>
    /// Sets the location of the operation using a URI.
    /// </summary>
    /// <param name="location">The URI location of the operation.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithLocation(Uri location);

    /// <summary>
    /// Sets the location of the operation using a string.
    /// </summary>
    /// <param name="location">The string location of the operation.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithLocation(string location);
}

/// <summary>
/// Interface for setting the result of an operation being built.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IResultBuilder<out TBuilder, in TResult>
    where TBuilder : class, IBuilder
{
    /// <summary>
    /// Sets the result of the operation being built.
    /// </summary>
    /// <param name="result">The result of the operation.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithResult(TResult result);
}

/// <summary>
/// Interface for setting headers of an operation being built.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IHeaderBuilder<out TBuilder>
    where TBuilder : class, IBuilder
{
    /// <summary>
    /// Sets a header for the operation being built.
    /// </summary>
    /// <param name="key">The header key.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithHeader(string key, string value);

    /// <summary>
    /// Sets a header with multiple values for the operation being built.
    /// </summary>
    /// <param name="key">The header key.</param>
    /// <param name="values">The header values.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithHeader(string key, params string[] values);

    /// <summary>
    /// Sets multiple headers for the operation being built.
    /// </summary>
    /// <param name="headers">The headers dictionary.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithHeaders(IDictionary<string, string> headers);

    /// <summary>
    /// Sets multiple headers for the operation being built.
    /// </summary>
    /// <param name="headers">The headers collection.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithHeaders(ElementCollection headers);
}

/**
 * <summary>
 * Interface for building an <see cref="IOperationResult"/> with error details.
 * </summary>
 * <typeparam name="TBuilder">The type of the builder.</typeparam>
 */
public interface IErrorBuilder<out TBuilder>
    where TBuilder : class, IBuilder
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
    /// <param name="error">The error entry.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithError(ElementEntry error);

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
    /// Adds an exception as an error.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns>The current builder instance.</returns>
    TBuilder WithException(Exception exception);
}

/// <summary>
/// Interface for building an <see cref="IOperationResult"/> with extensions.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IExtensionBuilder<out TBuilder>
    where TBuilder : class, IBuilder
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
}

/**
 * <summary>
 * Interface for clearing various elements of an operation being built.
 * </summary>
 * <typeparam name="TBuilder">The type of the builder.</typeparam>
 */
public interface IClearBuilder<out TBuilder>
    where TBuilder : class, IBuilder
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
