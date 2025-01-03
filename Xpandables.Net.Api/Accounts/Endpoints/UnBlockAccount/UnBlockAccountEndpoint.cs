﻿
using Microsoft.AspNetCore.Mvc;

using Xpandables.Net.Commands;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Http.Interfaces;

namespace Xpandables.Net.Api.Accounts.Endpoints.UnBlockAccount;

public sealed class UnBlockAccountEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app) =>
        app.MapPost("/accounts/unblock",
            async (
                [FromBody] UnBlockAccountRequest request,
                IDispatcher dispatcher,
                CancellationToken cancellationToken) =>
            {
                UnBlockAccountCommand command = new()
                {
                    KeyId = request.KeyId
                };

                return await dispatcher
                    .SendAsync(command, cancellationToken)
                    .ConfigureAwait(false);
            })
        .WithTags("Accounts")
        .WithName("UnBlockAccount")
        .WithXExecutionResultMinimalApi()
        .AllowAnonymous()
        .Accepts<UnBlockAccountRequest>(HttpClientParameters.ContentType.Json)
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
}