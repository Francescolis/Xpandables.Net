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
using System.Net;

using Microsoft.Extensions.Primitives;

namespace System.ExecutionResults;

/// <summary>
/// Represents a builder for creating successful operation results.
/// </summary>
public sealed class OperationResultSuccessBuilder :
    OperationResultBuilder<IOperationResultSuccessBuilder>, IOperationResultSuccessBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OperationResultSuccessBuilder"/> class 
    /// with the specified status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code indicating a 
    /// successful operation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status 
    /// code is not between 200 and 299.</exception>
    public OperationResultSuccessBuilder(HttpStatusCode statusCode) :
        base(statusCode) => statusCode.EnsureSuccess();
}

/// <summary>
/// Represents a builder for creating successful operation results with a 
/// specified result type.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public sealed class OperationResultSuccessBuilder<TResult> :
    ExecutionResultBuilder<IOperationResultSuccessBuilder<TResult>, TResult>,
    IOperationResultSuccessBuilder<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OperationResultSuccessBuilder{TResult}"/> class 
    /// with the specified status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code indicating a 
    /// successful operation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status 
    /// code is not between 200 and 299.</exception>
    public OperationResultSuccessBuilder(HttpStatusCode statusCode) :
        base(statusCode) => statusCode.EnsureSuccess();
}

/// <summary>  
/// Represents a builder for creating failure operation results.  
/// </summary>  
public sealed class OperationResultFailureBuilder :
    OperationResultBuilder<IOperationResultFailureBuilder>, IOperationResultFailureBuilder
{
    /// <summary>  
    /// Initializes a new instance of the <see cref="OperationResultFailureBuilder"/> class 
    /// with the specified status code.  
    /// </summary>  
    /// <param name="statusCode">The HTTP status code for the failure.</param>  
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status 
    /// code is between 200 and 299.</exception>  
    public OperationResultFailureBuilder(HttpStatusCode statusCode) :
        base(statusCode) => statusCode.EnsureFailure();
}

/// <summary>  
/// Represents a builder for creating failure operation results with a specific 
/// result type.  
/// </summary>  
/// <typeparam name="TResult">The type of the result.</typeparam>  
public sealed class OperationResultFailureBuilder<TResult> :
   ExecutionResultBuilder<IOperationResultFailureBuilder<TResult>, TResult>,
   IOperationResultFailureBuilder<TResult>
{
    /// <summary>  
    /// Initializes a new instance of the <see cref="OperationResultFailureBuilder{TResult}"/> class  
    /// with the specified status code.  
    /// </summary>  
    /// <param name="statusCode">The HTTP status code for the failure.</param>  
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status  
    /// code is between 200 and 299.</exception>  
    public OperationResultFailureBuilder(HttpStatusCode statusCode) :
        base(statusCode) => statusCode.EnsureFailure();
}

/// <summary>
/// Represents a builder for creating operation results with various properties.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public abstract class OperationResultBuilder<TBuilder>(HttpStatusCode statusCode) :
    IOperationResultObjectBuilder<TBuilder>,
    IOperationResultHeaderBuilder<TBuilder>,
    IOperationResultLocationBuilder<TBuilder>,
    IOperationResultErrorBuilder<TBuilder>,
    IOperationResultDetailBuilder<TBuilder>,
    IOperationResultTitleBuilder<TBuilder>,
    IOperationResultMergeBuilder<TBuilder>,
    IOperationResultStatusBuilder<TBuilder>,
    IOperationResultExtensionBuilder<TBuilder>,
    IOperationResultClearBuilder<TBuilder>,
    IOperationResultBuilder
    where TBuilder : class, IOperationResultBuilder
{
    private readonly bool _isSuccess = statusCode.IsSuccess;

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
    /// Gets the exception associated with the operation result.
    /// </summary>
    protected Exception? Exception { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code for the operation result.
    /// </summary>
    protected HttpStatusCode StatusCode { get; set; } = statusCode;

    /// <summary>
    /// Gets or sets the title for the operation result.
    /// </summary>
    protected string? Title { get; set; }

    /// <summary>  
    /// Gets or sets the detail for the operation result.  
    /// </summary>  
    protected string? Detail { get; set; }

    /// <summary>
    /// Gets or sets the result object of the operation.
    /// </summary>
    protected object? Result { get; set; }

    /// <summary>
    /// Gets or sets the location URI for the operation result.
    /// </summary>
    protected Uri? Location { get; set; }

    /// <summary>
    /// Returns the current builder instance cast to TBuilder.
    /// </summary>
    private TBuilder AsBuilder => (this as TBuilder)!;

    /// <inheritdoc/>
    public OperationResult Build() =>
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

        StatusCode = _isSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
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
    public TBuilder Merge(OperationResult operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        if (operation.StatusCode != StatusCode)
        {
            throw new InvalidOperationException(
                "Both operation results must have the same status code to merge them.");
        }

        StatusCode = operation.StatusCode;
        Title = operation.Title ?? Title;
        Detail = operation.Detail ?? Detail;
        Location = operation.Location ?? Location;
        Headers.Merge(operation.Headers);
        Extensions.Merge(operation.Extensions);
        Errors.Merge(operation.Errors);
        Exception = (operation.Exception, Exception) switch
        {
            (null, null) => null,
            (not null, null) => operation.Exception,
            (null, not null) => Exception,
            (not null, not null) => CombineExceptions(operation.Exception, Exception)
        };

        return AsBuilder;

        static Exception CombineExceptions(Exception executionException, Exception currentException)
        {
            var exceptions = new List<Exception>();

            // Flatten the first exception
            if (executionException is AggregateException aggEx1)
            {
                exceptions.AddRange(aggEx1.InnerExceptions);
            }
            else
            {
                exceptions.Add(executionException);
            }

            // Flatten the second exception
            if (currentException is AggregateException aggEx2)
            {
                exceptions.AddRange(aggEx2.InnerExceptions);
            }
            else
            {
                exceptions.Add(currentException);
            }

            return new AggregateException(exceptions);
        }
    }

    /// <inheritdoc/>
    public TBuilder WithResult(object result)
    {
        ArgumentNullException.ThrowIfNull(result);

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
        if (_isSuccess)
        {
            statusCode.EnsureSuccess();
        }
        else
        {
            statusCode.EnsureFailure();
        }

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
/// Represents a builder for creating operation results with a specific result type.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <param name="statusCode">The HTTP status code for the operation result.</param>
public abstract class ExecutionResultBuilder<TBuilder, TResult>(HttpStatusCode statusCode) :
    OperationResultBuilder<TBuilder>(statusCode),
    IOperationResultResultBuilder<TBuilder, TResult>,
    IOperationResultBuilder<TResult>
    where TBuilder : class, IOperationResultBuilder<TResult>
{
    /// <summary>
    /// Gets or sets the result object of the operation.
    /// </summary>
    protected new TResult? Result
    {
        get => base.Result is TResult value ? value : default;
        set => base.Result = value;
    }

    /// <inheritdoc/>
    public TBuilder WithResult(TResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        Result = result;
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public new OperationResult<TResult> Build() =>
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