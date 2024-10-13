using System.Net;

namespace Xpandables.Net.Http;

/// <summary>
/// Represents a delegate that builds an HTTP client response.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <param name="context">The context of the HTTP client response.</param>
/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
/// <returns>A task that represents the asynchronous operation. 
/// The task result contains the built response object.</returns>
public delegate Task<TResponse> ResponseBuilderDelegate<TResponse>(
    HttpClientResponseContext context,
    CancellationToken cancellationToken)
    where TResponse : HttpClientResponse;

/// <summary>
/// Defines methods to build HTTP client responses.
/// </summary>
public interface IHttpClientResponseBuilder
{
    /// <summary>
    /// Gets the response content type result being built by the current 
    /// builder instance.
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <returns></returns>
    ResponseBuilderDelegate<TResponse> Builder<TResponse>()
        where TResponse : HttpClientResponse;

    /// <summary>
    /// When overridden in a derived class, determines whether the builder
    /// instance can build the response for the specified status code.
    /// </summary>
    /// <param name="targetType">The type of the response.</param>
    /// <param name="statusCode">The status code of the response.</param>
    /// <returns><see langword="true"/> if the instance can build the
    /// specified request; otherwise, <see langword="false"/>.</returns>
    bool CanBuild(Type targetType, HttpStatusCode statusCode);

    /// <summary>
    /// Asynchronously builds the response for the specified context.
    /// </summary>
    /// <param name="context">The context of the HTTP client response.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the built response object.</returns>
    /// <exception cref="InvalidOperationException">The response cannot be built.</exception>
    public Task<TResponse> BuildAsync<TResponse>(
        HttpClientResponseContext context,
        CancellationToken cancellationToken)
        where TResponse : HttpClientResponse =>
        Builder<TResponse>()(context, cancellationToken);
}