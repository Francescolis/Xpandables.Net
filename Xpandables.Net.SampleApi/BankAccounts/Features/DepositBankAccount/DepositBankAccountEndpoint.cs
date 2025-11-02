
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Tasks;

namespace Xpandables.Net.SampleApi.BankAccounts.Features.DepositBankAccount;

public sealed class DepositBankAccountEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/bank-accounts/{accountId}/deposit", async (DepositBankAccountCommand command, IMediator mediator) =>
        await mediator.SendAsync(command).ConfigureAwait(false))
            .WithXMinimalApi()
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
