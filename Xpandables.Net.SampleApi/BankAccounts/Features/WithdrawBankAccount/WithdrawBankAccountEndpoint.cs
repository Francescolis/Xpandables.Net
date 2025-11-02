
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Tasks;

namespace Xpandables.Net.SampleApi.BankAccounts.Features.WithdrawBankAccount;

public sealed class WithdrawBankAccountEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/bank-accounts/{accountId}/withdraw", async (WithdrawBankAccountCommand command, IMediator mediator) =>
            await mediator.SendAsync(command).ConfigureAwait(false))
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
