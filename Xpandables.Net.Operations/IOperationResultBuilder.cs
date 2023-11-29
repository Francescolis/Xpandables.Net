
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
using System.ComponentModel;
using System.Net;

using Xpandables.Net.Primitives;

namespace Xpandables.Net.Operations;

public partial interface IOperationResult
{
    /// <summary>
    /// Provides with commands to build a success <see cref="IOperationResult"/> in a fluent design.
    /// </summary>
    public interface ISuccessBuilder :
        IHeaderBuilder<ISuccessBuilder>,
        IUrlBuilder<ISuccessBuilder>,
        IStatusBuilder<ISuccessBuilder>,
        IClearBuilder<ISuccessBuilder>,
        IBuilder
    { }

    /// <summary>
    /// Provides with commands to build a success <see cref="IOperationResult{TResult}"/> in a fluent design.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface ISuccessBuilder<TResult> :
        IHeaderBuilder<ISuccessBuilder<TResult>>,
        IUrlBuilder<ISuccessBuilder<TResult>>,
        IResultBuilder<ISuccessBuilder<TResult>, TResult>,
        IStatusBuilder<ISuccessBuilder<TResult>>,
        IClearBuilder<ISuccessBuilder<TResult>>,
        IBuilder<TResult>
    { }

    /// <summary>
    /// Provides with commands to build a failure <see cref="IOperationResult"/> in a fluent design.
    /// </summary>
    public interface IFailureBuilder :
        IHeaderBuilder<IFailureBuilder>,
        IErrorBuilder<IFailureBuilder>,
        IStatusBuilder<IFailureBuilder>,
        IDescriptionBuilder<IFailureBuilder>,
        IClearBuilder<IFailureBuilder>,
        IBuilder
    { }

    /// <summary>
    /// Provides with commands to build a success <see cref="IOperationResult{TResult}"/> in a fluent design.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface IFailureBuilder<TResult> :
        IHeaderBuilder<IFailureBuilder<TResult>>,
        IErrorBuilder<IFailureBuilder<TResult>>,
        IStatusBuilder<IFailureBuilder<TResult>>,
        IDescriptionBuilder<IFailureBuilder<TResult>>,
        IClearBuilder<IFailureBuilder<TResult>>,
        IBuilder<TResult>
    { }


    /// <summary>
    /// Provides with the command to build the <see cref="IOperationResult"/>.
    /// </summary>
    public interface IBuilder
    {
        /// <summary>
        /// Returns the instance that matches the builder information.
        /// </summary>
        /// <returns>An implementation of <see cref="IOperationResult"/>.</returns>
        OperationResult Build();
    }

    /// <summary>
    /// Provides with command to create the target instance that implements <see cref="IOperationResult{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface IBuilder<TResult> : IBuilder
    {
        /// <summary>
        /// Returns the instance that matches the builder information.
        /// </summary>
        /// <returns>An implementation of <see cref="IOperationResult{TResult}"/>.</returns>
        new OperationResult<TResult> Build();

        [EditorBrowsable(EditorBrowsableState.Never)]
        OperationResult IBuilder.Build() => Build();
    }

    /// <summary>
    /// Provides with command to add status code to the <see cref="IStatusBuilder{TBuilder}"/> builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the target builder.</typeparam>
    public interface IStatusBuilder<out TBuilder>
    {
        /// <summary>
        /// Adds the specified status code to the builder.
        /// </summary>
        /// <param name="statusCode">The status code result to be used by the builder.</param>
        /// <returns>The current instance.</returns>
        TBuilder WithStatusCode(HttpStatusCode statusCode);
    }

    /// <summary>
    /// Provides with command to add title and/or detail to the <see cref="IDescriptionBuilder{TBuilder}"/> builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the target builder.</typeparam>
    public interface IDescriptionBuilder<out TBuilder>
    {
        /// <summary>
        /// Adds the specified title to the builder.
        /// </summary>
        /// <param name="title">the operation title from the execution operation.</param>
        /// <returns>The current instance.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="title"/> is null or empty.</exception>
        TBuilder WithTitle(string title);

        /// <summary>
        /// Adds the specified detail to the builder.
        /// </summary>
        /// <param name="detail">the operation detail from the execution operation.</param>
        /// <returns>The current instance.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="detail"/> is null or empty.</exception>
        TBuilder WithDetail(string detail);
    }

    /// <summary>
    /// Provides with command to add URL to the <see cref="IBuilder{TResult}"/> builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the target builder.</typeparam>
    public interface IUrlBuilder<out TBuilder>
        where TBuilder : class, IBuilder
    {
        /// <summary>
        /// Adds the URL for location header.Mostly used with <see cref="HttpStatusCode.Created"/> to the builder.
        /// </summary>
        /// <param name="url">The URL to added.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="url"/> is null.</exception>
        /// <exception cref="UriFormatException">The <paramref name="url"/> is a bad format.</exception>
        /// <returns>The current instance.</returns>
        TBuilder WithUrl(string url);

        /// <summary>
        /// Adds the URL for location header.Mostly used with <see cref="HttpStatusCode.Created"/> to the builder.
        /// </summary>
        /// <param name="uri">The URL to added.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is null.</exception>
        /// <returns>The current instance.</returns>
        TBuilder WithUrl(Uri uri);
    }

    /// <summary>
    /// Provides with command to add a result of <typeparamref name="TResult"/> type to the <see cref="IBuilder{TResult}"/> builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the target builder.</typeparam>
    /// <typeparam name="TResult">The type of the target result.</typeparam>
    public interface IResultBuilder<out TBuilder, in TResult>
        where TBuilder : class, IBuilder
    {
        /// <summary>
        /// Adds the specified result to the builder.
        /// </summary>
        /// <param name="result">The result to be used by the builder.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="result"/> is null.</exception>
        /// <returns>The current instance.</returns>
        TBuilder WithResult(TResult result);
    }

    /// <summary>
    /// Provides with command to add header to the <see cref="IBuilder{TResult}"/> builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the target builder.</typeparam>
    public interface IHeaderBuilder<out TBuilder>
        where TBuilder : class, IBuilder
    {
        /// <summary>
        /// Adds the specified <paramref name="key"/> and <paramref name="value"/> to the header collection.
        /// </summary>
        /// <param name="key">The key header to add.</param>
        /// <param name="value">The associated value.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> or <paramref name="value"/> is null.</exception>
        /// <returns>The current instance.</returns>
        TBuilder WithHeader(string key, string value);

        /// <summary>
        /// Adds the specified <paramref name="key"/> and <paramref name="values"/> to the header collection.
        /// </summary>
        /// <param name="key">The key header to add.</param>
        /// <param name="values">The associated value.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> or <paramref name="values"/> is null.</exception>
        /// <returns>The current instance.</returns>
        TBuilder WithHeader(string key, params string[] values);

        /// <summary>
        /// Adds the specified dictionary of headers to the header collection.
        /// </summary>
        /// <param name="headers">The dictionary to be added.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="headers"/> is null.</exception>
        /// <returns>The current instance.</returns>
        TBuilder WithHeaders(IDictionary<string, string> headers);

        /// <summary>
        /// Adds the specified header collection to the existing one.
        /// </summary>
        /// <param name="headers">the other collection of headers to merge to.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="headers"/> is null.</exception>
        /// <returns>The current instance.</returns>
        TBuilder WithHeaders(ElementCollection headers);
    }

    /// <summary>
    /// Provides with commands to add errors to the <see cref="IBuilder{TResult}"/> builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the target builder.</typeparam>
    public interface IErrorBuilder<out TBuilder>
        where TBuilder : class, IBuilder
    {
        /// <summary>
        /// Adds the <paramref name="key"/> and the <paramref name="errorMessages"/> to the errors collection.
        /// </summary>
        /// <param name="key">The key of <see cref="ElementEntry"/> to add errors to.</param>
        /// <param name="errorMessages">The associated error messages.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="errorMessages"/> is null.</exception>
        /// <returns>The current instance.</returns>
        TBuilder WithError(string key, params string[] errorMessages);

        /// <summary>
        /// Adds the <paramref name="key"/> and the <paramref name="exception"/> to the errors collection.
        /// </summary>
        /// <param name="key">The key of <see cref="ElementEntry"/> to add errors to.</param>
        /// <param name="exception">The associated exception.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="exception"/> is null.</exception>
        /// <returns>The current instance.</returns>
        TBuilder WithError(string key, Exception exception);

        /// <summary>
        /// Adds the <paramref name="error"/> to the errors collection.
        /// </summary>
        /// <param name="error">The error that is wrapped by the new collection.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="error"/> is null.</exception>
        /// <returns>The current instance.</returns>
        TBuilder WithError(ElementEntry error);

        /// <summary>
        /// Adds the <paramref name="errors"/> to the errors collection.
        /// </summary>
        /// <param name="errors">the other collection of errors to merge to.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="errors"/> is null.</exception>
        /// <returns>The current instance.</returns>
        TBuilder WithErrors(ElementCollection errors);

        /// <summary>
        /// Adds the <paramref name="errors"/> to the errors collection.
        /// </summary>
        /// <param name="errors">the other collection of errors to merge to.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="errors"/> is null.</exception>
        /// <returns>The current instance.</returns>
        TBuilder WithErrors(IReadOnlyCollection<ElementEntry> errors);
    }

    /// <summary>
    /// Provides with command to clear the current builder in order to be reused.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the target builder.</typeparam>
    public interface IClearBuilder<out TBuilder>
    {
        /// <summary>
        /// Reset the builder to allow using it to build new <see cref="IOperationResult"/>.
        /// </summary>
        /// <returns>The current instance.</returns>
        TBuilder Clear();
    }
}