using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.UploadPicture;

public sealed class UploadPictureEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app) =>
        app.MapPut("/accounts/picture",
            async (
                IFormFile formFile,
                IMediator dispatcher,
                CancellationToken cancellationToken) =>
            {
                using MemoryStream memoryStream = new();
                await formFile.OpenReadStream().CopyToAsync(memoryStream, cancellationToken);
                return ExecutionResult.Success();
            })
        .WithTags("Accounts")
        .WithName("PictureAccount")
        .WithXMinimalApi()
        .DisableAntiforgery()
        .AllowAnonymous()
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
}
