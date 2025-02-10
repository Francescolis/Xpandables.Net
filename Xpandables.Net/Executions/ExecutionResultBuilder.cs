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
using Xpandables.Net.Operations;

namespace Xpandables.Net.Executions;

/// <summary>
/// Represents a builder for creating execution results with various properties.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public abstract class ExecutionResultBuilder<TBuilder>(HttpStatusCode statusCode) :
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
    /// Gets or sets the HTTP status code for the execution result.
    /// </summary>
    protected HttpStatusCode StatusCode { get; set; } = statusCode;

    /// <summary>
    /// Gets or sets the title for the execution result.
    /// </summary>
    protected string? Title { get; set; }

    /// <summary>  
    /// Gets or sets the detail for the execution result.  
    /// </summary>  
    protected string? Detail { get; set; }

    /// <summary>
    /// Gets or sets the result object of the execution.
    /// </summary>
    protected object? Result { get; set; }

    /// <summary>
    /// Gets or sets the location URI for the execution result.
    /// </summary>
    protected Uri? Location { get; set; }

    /// <inheritdoc/>
    public IExecutionResult Build() =>
        new ExecutionResult
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
    public TBuilder Merge(IExecutionResult execution)
    {
        ArgumentNullException.ThrowIfNull(execution);

        if (execution.IsSuccessStatusCode
            || (int)StatusCode is >= 200 and <= 299)
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
        ElementEntry? entry = Errors[ExecutionResultExtensions.ExceptionKey];
        if (entry.HasValue)
        {
            _ = Errors.Remove(entry.Value.Key);
            entry = entry.Value with
            {
                Values = [.. new string[] { BuildErrorMessage(exception) }.Union(entry.Value.Values)]
            };
        }
        else
        {
            entry = new ElementEntry(
                ExecutionResultExtensions.ExceptionKey,
                BuildErrorMessage(exception));
        }

        Errors.Add(entry.Value);

        return (this as TBuilder)!;

        static string BuildErrorMessage(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

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
    public new IExecutionResult<TResult> Build() =>
        new ExecutionResult<TResult>
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