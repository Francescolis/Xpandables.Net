namespace Xpandables.Net.SampleApi.BankAccounts.Features.CreateBankAccount;

public sealed class CreateBankAccountEndpoint : IEndpointRoute
{
    public void AddRoutes(EndpointRouteBuilder app)
    {
        app.MapPost("/bank-accounts", async (CreateBankAccountCommand command, IMediator mediator) =>
            await mediator.SendAsync(command).ConfigureAwait(false))
            .AllowAnonymous()
            .WithTags("BankAccounts")
            .WithName("CreateBankAccount")
            .WithSummary("Creates a new bank account.")
            .WithDescription("Creates a new bank account with the provided details.")
            .Accepts<CreateBankAccountCommand>()
            .Produces201Created<CreateBankAccountResult>()
            .Produces400BadRequest()
            .Produces401Unauthorized()
            .Produces500InternalServerError();
    }
}