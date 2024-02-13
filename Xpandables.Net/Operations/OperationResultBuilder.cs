
/************************************************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
************************************************************************************************************/
using System.Net;

using Xpandables.Net.Optionals;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Operations;

internal abstract class Builder<TBuilder>(HttpStatusCode statusCode) :
    IOperationResult.IHeaderBuilder<TBuilder>,
    IOperationResult.IUrlBuilder<TBuilder>,
    IOperationResult.IErrorBuilder<TBuilder>,
    IOperationResult.IStatusBuilder<TBuilder>,
    IOperationResult.IDescriptionBuilder<TBuilder>,
    IOperationResult.IClearBuilder<TBuilder>,
    IOperationResult.IBuilder
    where TBuilder : class, IOperationResult.IBuilder
{
    private protected readonly ElementCollection _headers = [];
    private protected readonly ElementCollection _errors = [];
    private protected HttpStatusCode _statusCode = statusCode;
    private protected Optional<string> _uri = Optional.Empty<string>();
    private protected Optional<object> _result = Optional.Empty<object>();
    private protected Optional<string> _title = Optional.Empty<string>();
    private protected Optional<string> _detail = Optional.Empty<string>();

    TBuilder IOperationResult.IStatusBuilder<TBuilder>.WithStatusCode(HttpStatusCode statusCode)
    {
        if (_statusCode.IsSuccessStatusCode())
            _ = statusCode.EnsureSuccessStatusCode();
        else
            _ = statusCode.EnsureFailureStatusCode();

        _statusCode = statusCode;
        return (this as TBuilder)!;
    }

    TBuilder IOperationResult.IDescriptionBuilder<TBuilder>.WithTitle(string title)
    {
        _title = title;
        return (this as TBuilder)!;
    }

    TBuilder IOperationResult.IDescriptionBuilder<TBuilder>.WithDetail(string detail)
    {
        _detail = detail;
        return (this as TBuilder)!;
    }

    TBuilder IOperationResult.IClearBuilder<TBuilder>.Clear()
    {
        _headers.Clear();
        _errors.Clear();
        _statusCode = _statusCode.IsSuccessStatusCode() ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
        _uri = Optional.Empty<string>();
        _result = Optional.Empty<object>();
        _detail = Optional.Empty<string>();
        _title = Optional.Empty<string>();

        return (this as TBuilder)!;
    }

    IOperationResult IOperationResult.IBuilder.Build()
        => new OperationResult(
            _statusCode,
            _result,
            _uri,
            _errors,
            _headers,
            _title,
            _detail);

    TBuilder IOperationResult.IErrorBuilder<TBuilder>.WithError(string key, params string[] errorMessages)
    {
        _errors.Add(key, [.. errorMessages]);
        return (this as TBuilder)!;
    }

    TBuilder IOperationResult.IErrorBuilder<TBuilder>.WithError(string key, Exception exception)
    {
        _errors.Add(key, exception.ToString());
        return (this as TBuilder)!;
    }

    TBuilder IOperationResult.IErrorBuilder<TBuilder>.WithError(ElementEntry error)
    {
        _errors.Add(error);
        return (this as TBuilder)!;
    }

    TBuilder IOperationResult.IErrorBuilder<TBuilder>.WithErrors(ElementCollection errors)
    {
        _errors.Merge(errors);
        return (this as TBuilder)!;
    }

    TBuilder IOperationResult.IErrorBuilder<TBuilder>.WithErrors(IReadOnlyCollection<ElementEntry> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        _errors.Merge(ElementCollection.With(errors.ToList()));

        return (this as TBuilder)!;
    }

    TBuilder IOperationResult.IHeaderBuilder<TBuilder>.WithHeader(string key, string value)
    {
        _headers.Add(key, value);
        return (this as TBuilder)!;
    }

    TBuilder IOperationResult.IHeaderBuilder<TBuilder>.WithHeader(string key, params string[] values)
    {
        _headers.Add(key, values);
        return (this as TBuilder)!;
    }

    TBuilder IOperationResult.IHeaderBuilder<TBuilder>.WithHeaders(IDictionary<string, string> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        _headers.Merge(ElementCollection.With(headers.Select(x => new ElementEntry(x.Key, x.Value)).ToList()));

        return (this as TBuilder)!;
    }

    TBuilder IOperationResult.IHeaderBuilder<TBuilder>.WithHeaders(ElementCollection headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        _headers.Merge(headers);
        return (this as TBuilder)!;
    }

    TBuilder IOperationResult.IUrlBuilder<TBuilder>.WithUrl(string url)
    {
        _uri = url;
        return (this as TBuilder)!;
    }

    TBuilder IOperationResult.IUrlBuilder<TBuilder>.WithUrl(Uri uri)
    {
        _ = uri ?? throw new ArgumentNullException(nameof(uri));
        _uri = uri.AbsolutePath;
        return (this as TBuilder)!;
    }
}

internal abstract class Builder<TBuilder, TResult>(HttpStatusCode statusCode) :
    Builder<TBuilder>(statusCode),
    IOperationResult.IResultBuilder<TBuilder, TResult>,
    IOperationResult.IBuilder<TResult>
    where TBuilder : class, IOperationResult.IBuilder<TResult>
{
    IOperationResult<TResult> IOperationResult.IBuilder<TResult>.Build()
        => new OperationResult<TResult>(
            _statusCode,
             _result.IsEmpty
                ? Optional.Empty<TResult>()
                : _result.Value is TResult value
                    ? Optional.Some<TResult>(value)
                    : Optional.Empty<TResult>(),
            _uri,
            _errors,
            _headers,
            _title,
            _detail);

    TBuilder IOperationResult.IResultBuilder<TBuilder, TResult>.WithResult(TResult result)
    {
        _ = result ?? throw new ArgumentNullException(nameof(result));
        _result = Optional.Some<object>(result);
        return (this as TBuilder)!;
    }
}

internal sealed class SuccessBuilder : Builder<IOperationResult.ISuccessBuilder>, IOperationResult.ISuccessBuilder
{
    internal SuccessBuilder(HttpStatusCode statusCode) : base(statusCode)
    {
        _ = statusCode.EnsureSuccessStatusCode();
    }
}

internal sealed class SuccessBuilder<TResult> :
    Builder<IOperationResult.ISuccessBuilder<TResult>, TResult>, IOperationResult.ISuccessBuilder<TResult>
{
    internal SuccessBuilder(HttpStatusCode statusCode) : base(statusCode)
    {
        _ = statusCode.EnsureSuccessStatusCode();
    }
}

internal sealed class FailureBuilder : Builder<IOperationResult.IFailureBuilder>, IOperationResult.IFailureBuilder
{
    internal FailureBuilder(HttpStatusCode statusCode) : base(statusCode)
    {
        _ = statusCode.EnsureFailureStatusCode();
    }
}

internal sealed class FailureBuilder<TResult> :
    Builder<IOperationResult.IFailureBuilder<TResult>, TResult>, IOperationResult.IFailureBuilder<TResult>
{
    internal FailureBuilder(HttpStatusCode statusCode) : base(statusCode)
    {
        _ = statusCode.EnsureFailureStatusCode();
    }
}
