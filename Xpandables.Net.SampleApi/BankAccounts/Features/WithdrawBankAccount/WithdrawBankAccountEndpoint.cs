
using System.OperationResults.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace Xpandables.Net.SampleApi.BankAccounts.Features.WithdrawBankAccount;

public sealed class WithdrawBankAccountEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/bank-accounts/{accountId}/withdraw",
            async (
                [FromRoute] Guid accountId,
                WithdrawBankAccountCommand command,
                IMediator mediator) =>
            await mediator.SendAsync(command with { AccountId = accountId }).ConfigureAwait(false))
            .WithXMinimalApi()
            .AllowAnonymous()
            .WithTags("BankAccounts")
            .WithName("WithdrawBankAccount")
            .WithSummary("Withdraws money from a bank account.")
            .WithDescription("Withdraws money from a bank account with the provided details.")
            .Accepts<WithdrawBankAccountCommand>()
            .Produces201Created<WithdrawBankAccountResult>()
            .Produces400BadRequest()
            .Produces401Unauthorized()
            .Produces500InternalServerError();
    }
}
