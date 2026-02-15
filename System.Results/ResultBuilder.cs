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

namespace System.Results;

/// <summary>
/// Represents a builder for creating result results with various properties.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public abstract class ResultBuilder<TBuilder>(HttpStatusCode statusCode) :
    IObjectResultBuilder<TBuilder>,
    IHeaderResultBuilder<TBuilder>,
    ILocationResultBuilder<TBuilder>,
    IErrorResultBuilder<TBuilder>,
    IDetailResultBuilder<TBuilder>,
    ITitleResultBuilder<TBuilder>,
    IMergeResultBuilder<TBuilder>,
    IStatusResultBuilder<TBuilder>,
    IExtensionResultBuilder<TBuilder>,
    IClearResultBuilder<TBuilder>,
    IResultBuilder
    where TBuilder : class, IResultBuilder
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
    /// Gets the exception associated with the result result.
    /// </summary>
    protected Exception? Exception { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code for the result result.
    /// </summary>
    protected HttpStatusCode StatusCode { get; set; } = statusCode;

    /// <summary>
    /// Gets or sets the title for the result result.
    /// </summary>
    protected string? Title { get; set; }

    /// <summary>  
    /// Gets or sets the detail for the result result.  
    /// </summary>  
    protected string? Detail { get; set; }

    /// <summary>
    /// Gets or sets the result object of the result.
    /// </summary>
    protected object? Value { get; set; }

    /// <summary>
    /// Gets or sets the location URI for the result result.
    /// </summary>
    protected Uri? Location { get; set; }

    /// <summary>
    /// Returns the current builder instance cast to TBuilder.
    /// </summary>
    protected TBuilder AsBuilder => (this as TBuilder)!;

    /// <inheritdoc/>
    /// <remarks>When implementing this method in a derived class, ensure that all
    /// properties are correctly set in the returned <see cref="Result"/> instance.
    /// </remarks>
    public virtual Result Build() =>
        new()
        {
            StatusCode = StatusCode,
            Title = Title,
            Detail = Detail,
            Location = Location,
            InternalValue = Value,
            Errors = Errors,
            Headers = Headers,
            Extensions = Extensions,
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
        Value = null;
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
    public TBuilder Merge(Result result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.StatusCode != StatusCode)
        {
            throw new InvalidOperationException(
                "Both result results must have the same status code to merge them.");
        }

        StatusCode = result.StatusCode;
        Title = result.Title ?? Title;
        Detail = result.Detail ?? Detail;
        Location = result.Location ?? Location;
        Headers.Merge(result.Headers);
        Extensions.Merge(result.Extensions);
        Errors.Merge(result.Errors);
        Exception = (result.Exception, Exception) switch
        {
            (null, null) => null,
            (not null, null) => result.Exception,
            (null, not null) => Exception,
            (not null, not null) => CombineExceptions(result.Exception, Exception)
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
    public TBuilder WithValue(object value)
    {
        ArgumentNullException.ThrowIfNull(value);

        Value = value;
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
/// Represents a builder for creating result results with a specific result type.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
/// <typeparam name="TValue">The type of the result.</typeparam>
/// <param name="statusCode">The HTTP status code for the result result.</param>
public abstract class ResultBuilder<TBuilder, TValue>(HttpStatusCode statusCode) :
    ResultBuilder<TBuilder>(statusCode),
    IValueResultBuilder<TBuilder, TValue>,
    IResultBuilder<TValue>
    where TBuilder : class, IResultBuilder<TValue>
{
    /// <summary>
    /// Gets or sets the result object of the result.
    /// </summary>
    protected new TValue? Value
    {
        get => base.Value is TValue value ? value : default;
        set => base.Value = value;
    }

    /// <inheritdoc/>
    public TBuilder WithValue(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        Value = value;
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    /// <remarks>When implementing this method in a derived class, ensure that all
    /// properties are correctly set in the returned <see cref="Result{TResult}"/> instance.
    /// </remarks>
    public new virtual Result<TValue> Build() =>
        new()
        {
            StatusCode = StatusCode,
            Title = Title,
            Detail = Detail,
            Location = Location,
            Value = Value,
            Errors = Errors,
            Headers = Headers,
            Extensions = Extensions,
            Exception = Exception
        };
}