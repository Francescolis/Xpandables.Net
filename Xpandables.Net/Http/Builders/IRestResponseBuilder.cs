using System.Net;

namespace Xpandables.Net.Http.Builders;

/// <summary>
/// Defines method to build <see cref="RestResponse"/> and <see cref="RestResponse{TResult}"/>.
/// </summary>
public interface IRestResponseBuilder
{
    /// <summary>
    /// Gets the response content type result being built by the current 
    /// builder instance.
    /// </summary>
    Type Type { get; }

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
    /// Asynchronously builds a response based on the provided context.
    /// </summary>
    /// <param name="context">This parameter provides the necessary context for building the response.</param>
    /// <param name="cancellationToken">This parameter allows the operation to be canceled if needed.</param>
    /// <returns>The method returns a task that resolves to the generated response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="context"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the operation fails.</exception>
    Task<TRestResponse> BuildAsync<TRestResponse>(RestResponseContext context, CancellationToken cancellationToken = default)
        where TRestResponse : RestResponseAbstract;
}