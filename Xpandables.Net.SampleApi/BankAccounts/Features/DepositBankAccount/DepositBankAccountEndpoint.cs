using Microsoft.AspNetCore.Mvc;

namespace Xpandables.Net.SampleApi.BankAccounts.Features.DepositBankAccount;

public sealed class DepositBankAccountEndpoint : IMinimalEndpointRoute
{
    public void AddRoutes(MinimalRouteBuilder app)
    {
        app.MapPost("/bank-accounts/{accountId}/deposit",
            async (
                [FromRoute] Guid accountId,
                DepositBankAccountCommand command,
                IMediator mediator) =>
        await mediator.SendAsync(command with { AccountId = accountId }).ConfigureAwait(false))
            .AllowAnonymous()
            .WithTags("BankAccounts")
            .WithName("DepositBankAccount")
            .WithSummary("Deposits money into a bank account.")
            .WithDescription("Deposits money into a bank account with the provided details.")
            .Accepts<DepositBankAccountCommand>()
            .Produces201Created<DepositBankAccountResult>()
            .Produces400BadRequest()
            .Produces401Unauthorized()
            .Produces500InternalServerError();
    }
}
