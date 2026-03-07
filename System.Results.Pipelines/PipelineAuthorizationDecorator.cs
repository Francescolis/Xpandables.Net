/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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
using System.Results.Requests;
using System.Security.Claims;

namespace System.Results.Pipelines;

/// <summary>
/// Marker interface for requests that require authorization before execution.
/// </summary>
/// <remarks>
/// Implement this interface on request types that should be subject to authorization checks.
/// The <see cref="PolicyName"/> determines which authorization policy is evaluated,
/// and <see cref="RequiredRoles"/> specifies any additional role-based requirements.
/// </remarks>
public interface IRequiresAuthorization : IRequest
{
	/// <summary>
	/// Gets the authorization policy name to evaluate.
	/// Return <see langword="null"/> for default (authenticated-user-only) checks.
	/// </summary>
	string? PolicyName => null;

	/// <summary>
	/// Gets the roles required to execute this request.
	/// Return an empty array when no specific roles are needed.
	/// </summary>
	string[] RequiredRoles => [];
}

/// <summary>
/// Provides the <see cref="ClaimsPrincipal"/> for the current pipeline execution.
/// </summary>
/// <remarks>
/// Register an implementation that returns the current user (e.g., from <c>IHttpContextAccessor</c>).
/// </remarks>
public interface IPipelinePrincipalAccessor
{
	/// <summary>
	/// Gets the claims principal for the current operation.
	/// </summary>
	ClaimsPrincipal? Principal { get; }
}

/// <summary>
/// A pipeline decorator that enforces authorization before the request handler executes.
/// </summary>
/// <typeparam name="TRequest">The type of the request. Must implement <see cref="IRequiresAuthorization"/>.</typeparam>
/// <param name="principalAccessor">Provides the current user's <see cref="ClaimsPrincipal"/>.</param>
/// <remarks>
/// <para>When the principal is unauthenticated, the decorator short-circuits with
/// <see cref="HttpStatusCode.Unauthorized"/>. When role requirements are not met,
/// it short-circuits with <see cref="HttpStatusCode.Forbidden"/>.</para>
/// <para>Register this decorator early in the pipeline, after the exception decorator
/// but before validation and the handler.</para>
/// </remarks>
public sealed class PipelineAuthorizationDecorator<TRequest>(
	IPipelinePrincipalAccessor principalAccessor) :
	IPipelineDecorator<TRequest>
	where TRequest : class, IRequiresAuthorization
{
	/// <inheritdoc/>
	public Task<Result> HandleAsync(
		RequestContext<TRequest> context,
		RequestHandler nextHandler,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(context);
		ArgumentNullException.ThrowIfNull(nextHandler);

		ClaimsPrincipal? principal = principalAccessor.Principal;

		if (principal?.Identity is not { IsAuthenticated: true })
		{
			Result unauthorized = Result
				.Failure()
				.WithStatusCode(HttpStatusCode.Unauthorized)
				.WithTitle("Unauthorized")
				.WithDetail("Authentication is required to execute this request.")
				.Build();

			return Task.FromResult(unauthorized);
		}

		string[] requiredRoles = context.Request.RequiredRoles;

		if (requiredRoles is { Length: > 0 }
			&& !Array.Exists(requiredRoles, principal.IsInRole))
		{
			Result forbidden = Result
				.Failure()
				.WithStatusCode(HttpStatusCode.Forbidden)
				.WithTitle("Forbidden")
				.WithDetail("You do not have the required role to execute this request.")
				.Build();

			return Task.FromResult(forbidden);
		}

		return nextHandler(cancellationToken);
	}
}
