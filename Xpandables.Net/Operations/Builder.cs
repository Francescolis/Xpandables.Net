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

using System.Net;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Operations;

/// <summary>
/// Represents a builder for creating operation results with various properties.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public abstract class Builder<TBuilder>(HttpStatusCode statusCode) :
    IHeaderBuilder<TBuilder>,
    ILocationBuilder<TBuilder>,
    IErrorBuilder<TBuilder>,
    IDetailBuilder<TBuilder>,
    ITitleBuilder<TBuilder>,
    IMergeBuilder<TBuilder>,
    IStatusBuilder<TBuilder>,
    IExtensionBuilder<TBuilder>,
    IClearBuilder<TBuilder>,
    IBuilder
    where TBuilder : class, IBuilder
{
    /// <summary>
    /// Gets the collection of headers.
    /// </summary>
    protected readonly ElementCollection Headers = [];

    /// <summary>
    /// Gets the collection of extensions.
    /// </summary>
    protected readonly ElementCollection Extensions = [];

    /// <summary>
    /// Gets the collection of errors.
    /// </summary>
    protected readonly ElementCollection Errors = [];

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

    /// <inheritdoc/>
    public IOperationResult Build() =>
        new OperationResult
        {
            StatusCode = StatusCode,
            Title = Title,
            Detail = Detail,
            Result = Result,
            Location = Location,
            Headers = Headers,
            Extensions = Extensions,
            Errors = Errors
        };

    /// <inheritdoc/>
    public TBuilder ClearAll()
    {
        _ = ClearErrors();
        _ = ClearExtensions();
        _ = ClearHeaders();

        StatusCode = default;
        Title = default;
        Detail = default;
        Location = default;
        Result = default;

        return (this as TBuilder)!;
    }
    /// <inheritdoc/>
    public TBuilder ClearErrors()
    {
        Errors.Clear();
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public TBuilder ClearExtensions()
    {
        Extensions.Clear();
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public TBuilder ClearHeaders()
    {
        Headers.Clear();
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public TBuilder Merge(IOperationResult operation)
    {
        if (operation.IsSuccessStatusCode
            || (int)StatusCode is >= 200 and <= 299)
        {
            throw new InvalidOperationException(
                "Both operation results must be failure to merge them.");
        }

        StatusCode = operation.StatusCode;
        Title = operation.Title ?? Title;
        Detail = operation.Detail ?? Detail;
        Location = operation.Location ?? Location;
        Headers.Merge(operation.Headers);
        Extensions.Merge(operation.Extensions);
        Errors.Merge(operation.Errors);

        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public TBuilder WithDetail(string detail)
    {
        Detail = detail;
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public TBuilder WithError(string key, string errorMessage)
    {
        Errors.Add(key, errorMessage);
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public TBuilder WithError(string key, params string[] errorMessages)
    {
        Errors.Add(key, errorMessages);
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public TBuilder WithError(ElementEntry error)
    {
        Errors.Add(error);
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public TBuilder WithErrors(IDictionary<string, string> errors)
    {
        Errors.AddRange(errors);
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public TBuilder WithErrors(ElementCollection errors)
    {
        Errors.Merge(errors);
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public TBuilder WithException(Exception exception)
    {
        ElementEntry? entry = Errors[OperationResultExtensions.ExceptionKey];
        if (entry.HasValue)
        {
            _ = Errors.Remove(entry.Value.Key);
            entry = entry.Value with
            {
                Values = new string[] { BuildErrorMessage(exception) }
                    .Union(entry.Value.Values).ToArray()
            };
        }
        else
        {
            entry = new ElementEntry(
                OperationResultExtensions.ExceptionKey,
                BuildErrorMessage(exception));
        }

        Errors.Add(entry.Value);

        return (this as TBuilder)!;

        static string BuildErrorMessage(Exception exception)
        {
            if (exception is AggregateException aggregateException)
            {
                return string.Join(
                    Environment.NewLine,
                    aggregateException.InnerExceptions
                        .Select(e => e.ToString()));
            }

            return exception.ToString();
        }
    }

    /// <inheritdoc/>
    public TBuilder WithExtension(string key, string value)
    {
        Extensions.Add(key, value);
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public TBuilder WithExtension(string key, params string[] values)
    {
        Extensions.Add(key, values);
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public TBuilder WithExtension(ElementEntry extension)
    {
        Extensions.Add(extension);
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public TBuilder WithExtensions(IDictionary<string, string> extensions)
    {
        Extensions.AddRange(extensions);
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public TBuilder WithExtensions(ElementCollection extensions)
    {
        Extensions.Merge(extensions);
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public TBuilder WithHeader(string key, string value)
    {
        Headers.Add(key, value);
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public TBuilder WithHeader(string key, params string[] values)
    {
        Headers.Add(key, values);
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public TBuilder WithHeaders(IDictionary<string, string> headers)
    {
        Headers.AddRange(headers);
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public TBuilder WithHeaders(ElementCollection headers)
    {
        Headers.Merge(headers);
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public TBuilder WithLocation(Uri location)
    {
        Location = location;
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public TBuilder WithLocation(string location)
    {
        Location = new Uri(location);
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public TBuilder WithStatusCode(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public TBuilder WithTitle(string title)
    {
        Title = title;
        return (this as TBuilder)!;
    }
}

public abstract class Builder<TBuilder, TResult>(HttpStatusCode statusCode) :
    Builder<TBuilder>(statusCode),
    IResultBuilder<TBuilder, TResult>,
    IBuilder<TResult>
    where TBuilder : class, IBuilder<TResult>
{
    /// <summary>
    /// Gets or sets the result object of the operation.
    /// </summary>
    protected new TResult? Result
    {
        get => (TResult?)base.Result;
        set => base.Result = value;
    }

    /// <inheritdoc/>
    public TBuilder WithResult(TResult result)
    {
        Result = result;
        return (this as TBuilder)!;
    }

    /// <inheritdoc/>
    public new IOperationResult<TResult> Build() =>
        new OperationResult<TResult>
        {
            StatusCode = StatusCode,
            Title = Title,
            Detail = Detail,
            Location = Location,
            Headers = Headers,
            Extensions = Extensions,
            Errors = Errors,
            Result = Result
        };
}