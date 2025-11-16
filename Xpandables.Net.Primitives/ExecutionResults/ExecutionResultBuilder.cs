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
using System.Net;

using Microsoft.Extensions.Primitives;

using Xpandables.Net.Collections;

namespace Xpandables.Net.ExecutionResults;

/// <summary>
/// Represents a builder for creating successful execution results.
/// </summary>
public sealed class ExecutionResultSuccessBuilder :
    ExecutionResultBuilder<IExecutionResultSuccessBuilder>, IExecutionResultSuccessBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionResultSuccessBuilder"/> class 
    /// with the specified status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code indicating a 
    /// successful execution.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status 
    /// code is not between 200 and 299.</exception>
    public ExecutionResultSuccessBuilder(HttpStatusCode statusCode) :
        base(statusCode) => statusCode.EnsureSuccess();
}

/// <summary>
/// Represents a builder for creating successful execution results with a 
/// specified result type.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public sealed class ExecutionResultSuccessBuilder<TResult> :
    ExecutionResultBuilder<IExecutionResultSuccessBuilder<TResult>, TResult>,
    IExecutionResultSuccessBuilder<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionResultSuccessBuilder{TResult}"/> class 
    /// with the specified status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code indicating a 
    /// successful execution.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status 
    /// code is not between 200 and 299.</exception>
    public ExecutionResultSuccessBuilder(HttpStatusCode statusCode) :
        base(statusCode) => statusCode.EnsureSuccess();
}

/// <summary>  
/// Represents a builder for creating failure execution results.  
/// </summary>  
public sealed class ExecutionResultFailureBuilder :
    ExecutionResultBuilder<IExecutionResultFailureBuilder>, IExecutionResultFailureBuilder
{
    /// <summary>  
    /// Initializes a new instance of the <see cref="ExecutionResultFailureBuilder"/> class 
    /// with the specified status code.  
    /// </summary>  
    /// <param name="statusCode">The HTTP status code for the failure.</param>  
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status 
    /// code is between 200 and 299.</exception>  
    public ExecutionResultFailureBuilder(HttpStatusCode statusCode) :
        base(statusCode) => statusCode.EnsureFailure();
}

/// <summary>  
/// Represents a builder for creating failure execution results with a specific 
/// result type.  
/// </summary>  
/// <typeparam name="TResult">The type of the result.</typeparam>  
public sealed class ExecutionResultFailureBuilder<TResult> :
   ExecutionResultBuilder<IExecutionResultFailureBuilder<TResult>, TResult>,
   IExecutionResultFailureBuilder<TResult>
{
    /// <summary>  
    /// Initializes a new instance of the <see cref="ExecutionResultFailureBuilder{TResult}"/> class  
    /// with the specified status code.  
    /// </summary>  
    /// <param name="statusCode">The HTTP status code for the failure.</param>  
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status  
    /// code is between 200 and 299.</exception>  
    public ExecutionResultFailureBuilder(HttpStatusCode statusCode) :
        base(statusCode) => statusCode.EnsureFailure();
}

/// <summary>
/// Represents a builder for creating execution results with various properties.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public abstract class ExecutionResultBuilder<TBuilder>(HttpStatusCode statusCode) :
    IExecutionResultObjectBuilder<TBuilder>,
    IExecutionResultHeaderBuilder<TBuilder>,
    IExecutionResultLocationBuilder<TBuilder>,
    IExecutionResultErrorBuilder<TBuilder>,
    IExecutionResultDetailBuilder<TBuilder>,
    IExecutionResultTitleBuilder<TBuilder>,
    IExecutionResultMergeBuilder<TBuilder>,
    IExecutionResultStatusBuilder<TBuilder>,
    IExecutionResultExtensionBuilder<TBuilder>,
    IExecutionResultClearBuilder<TBuilder>,
    IExecutionResultBuilder
    where TBuilder : class, IExecutionResultBuilder
{
    /// <summary>
    /// Gets the collection of headers.
    /// </summary>
    protected ElementCollection Headers { get; } = [];

    /// <summary>
    /// Gets the collection of extensions.
    /// </summary>
    protected ElementCollection Extensions { get; } = [];

    /// <summary>
    /// Gets the collection of errors.
    /// </summary>
    protected ElementCollection Errors { get; } = [];

    /// <summary>
    /// Gets the exception associated with the execution result.
    /// </summary>
    protected Exception? Exception { get; private set; }

    /// <summary>
    /// Gets or sets the HTTP status code for the execution result.
    /// </summary>
    protected HttpStatusCode StatusCode { get; private set; } = statusCode;

    /// <summary>
    /// Gets or sets the title for the execution result.
    /// </summary>
    protected string? Title { get; private set; }

    /// <summary>  
    /// Gets or sets the detail for the execution result.  
    /// </summary>  
    protected string? Detail { get; private set; }

    /// <summary>
    /// Gets or sets the result object of the execution.
    /// </summary>
    protected object? Result { get; set; }

    /// <summary>
    /// Gets or sets the location URI for the execution result.
    /// </summary>
    protected Uri? Location { get; private set; }

    /// <summary>
    /// Returns the current builder instance cast to TBuilder.
    /// </summary>
    private TBuilder AsBuilder => (this as TBuilder)!;

    /// <inheritdoc/>
    public ExecutionResult Build() =>
        new()
        {
            StatusCode = StatusCode,
            Title = Title,
            Detail = Detail,
            Value = Result,
            Location = Location,
            Headers = Headers,
            Extensions = Extensions,
            Errors = Errors,
            Exception = Exception
        };

    /// <inheritdoc/>
    public TBuilder ClearAll()
    {
        ClearErrors();
        ClearExtensions();
        ClearHeaders();

        StatusCode = HttpStatusCode.Continue;
        Title = null;
        Detail = null;
        Location = null;
        Result = null;
        Exception = null;

        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder ClearErrors()
    {
        Errors.Clear();
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder ClearExtensions()
    {
        Extensions.Clear();
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder ClearHeaders()
    {
        Headers.Clear();
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder Merge(ExecutionResult execution)
    {
        if (execution.IsSuccess || StatusCode.IsSuccess)
        {
            throw new InvalidOperationException(
                "Both execution results must be failure to merge them.");
        }

        StatusCode = execution.StatusCode;
        Title = execution.Title ?? Title;
        Detail = execution.Detail ?? Detail;
        Location = execution.Location ?? Location;
        Headers.Merge(execution.Headers);
        Extensions.Merge(execution.Extensions);
        Errors.Merge(execution.Errors);
        Exception = (execution.Exception, Exception) switch
        {
            (null, null) => null,
            (not null, null) => execution.Exception,
            (null, not null) => Exception,
            (not null, not null) => new AggregateException(execution.Exception, Exception)
        };

        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithResult(object? result)
    {
        Result = result;
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithDetail(string detail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(detail);
        Detail = detail.Trim();
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithError(string key, string errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);
        Errors.Add(key, errorMessage);
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithError(string key, params string[] errorMessages)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(errorMessages);
        if (errorMessages.Length == 0)
            throw new ArgumentException("At least one error message is required.", nameof(errorMessages));

        Errors.Add(key, errorMessages);
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithError(ElementEntry entry)
    {
        Errors.Add(entry);
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithErrors(IDictionary<string, string> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        Errors.AddRange(errors.AsReadOnly());
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithErrors(ElementCollection errors)
    {
        Errors.Merge(errors);
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithErrors(ReadOnlySpan<ElementEntry> errors)
    {
        for (int i = 0; i < errors.Length; i++)
        {
            Errors.Add(errors[i]);
        }
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithError(string key, in StringValues errorMessages)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentOutOfRangeException.ThrowIfZero(errorMessages.Count);

        Errors.Add(new ElementEntry(key, errorMessages));
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        Exception = exception;
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithExtension(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Extensions.Add(key, value);
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithExtension(string key, params string[] values)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(values);
        if (values.Length == 0)
            throw new ArgumentException("At least one extension value is required.", nameof(values));

        Extensions.Add(key, values);
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithExtension(ElementEntry extension)
    {
        Extensions.Add(extension);
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithExtensions(IDictionary<string, string> extensions)
    {
        ArgumentNullException.ThrowIfNull(extensions);
        Extensions.AddRange(extensions.AsReadOnly());
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithExtensions(ElementCollection extensions)
    {
        Extensions.Merge(extensions);
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithExtensions(IReadOnlyDictionary<string, StringValues> extensions)
    {
        ArgumentNullException.ThrowIfNull(extensions);
        Extensions.AddRange(extensions);
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithHeader(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Headers.Add(key, value);
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithHeader(string key, params string[] values)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(values);
        if (values.Length == 0)
            throw new ArgumentException("At least one header value is required.", nameof(values));

        Headers.Add(key, values);
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithHeaders(IDictionary<string, string> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);
        Headers.AddRange(headers.AsReadOnly());
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithHeaders(ElementCollection headers)
    {
        Headers.Merge(headers);
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithHeader(string key, in StringValues values)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentOutOfRangeException.ThrowIfZero(values.Count);

        Headers.Add(new ElementEntry(key, values));
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithHeaders(IReadOnlyDictionary<string, StringValues> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);
        Headers.AddRange(headers);
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithLocation(Uri location)
    {
        ArgumentNullException.ThrowIfNull(location);
        Location = location;
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithLocation(string location)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(location);
        try
        {
            Location = new Uri(location);
        }
        catch (UriFormatException ex)
        {
            throw new ArgumentException($"Invalid URI format: {location}", nameof(location), ex);
        }
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithStatusCode(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
        return AsBuilder;
    }

    /// <inheritdoc/>
    public TBuilder WithTitle(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        Title = title;
        return AsBuilder;
    }
}

/// <summary>
/// Represents a builder for creating execution results with a specific result type.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <param name="statusCode">The HTTP status code for the execution result.</param>
public abstract class ExecutionResultBuilder<TBuilder, TResult>(HttpStatusCode statusCode) :
    ExecutionResultBuilder<TBuilder>(statusCode),
    IExecutionResultResultBuilder<TBuilder, TResult>,
    IExecutionResultBuilder<TResult>
    where TBuilder : class, IExecutionResultBuilder<TResult>
{
    /// <summary>
    /// Gets or sets the result object of the execution.
    /// </summary>
    protected new TResult? Result
    {
        get => base.Result is TResult value ? value : default;
        set => base.Result = value;
    }

    /// <inheritdoc/>
    public TBuilder WithResult(TResult result)
    {
        Result = result;
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public new ExecutionResult<TResult> Build() =>
        new()
        {
            StatusCode = StatusCode,
            Title = Title,
            Detail = Detail,
            Location = Location,
            Headers = Headers,
            Extensions = Extensions,
            Errors = Errors,
            Value = Result,
            Exception = Exception
        };
}