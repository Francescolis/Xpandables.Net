namespace Xpandables.Net.SampleApi.BankAccounts.Features.GetBankAccountBalance;

public sealed class GetBankAccountBalanceEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/bank-accounts/{accountId}/balance", async (Guid accountId, IMediator mediator) =>
            await mediator.SendAsync(new GetBankAccountBalanceQuery { AccountId = accountId }).ConfigureAwait(false))
            .WithXMinimalApi()
            .AllowAnonymous()
            .WithTags("BankAccounts")
            .WithName("GetBankAccountBalance")
            .WithSummary("Gets the balance of a bank account.")
            .WithDescription("Retrieves the current balance of the specified bank account.")
            .Produces200OK<GetBankAccountBalanceResult>()
            .Produces400BadRequest()
            .Produces401Unauthorized()
            .Produces404NotFound()
            .Produces500InternalServerError();
    }

}
