/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
namespace Xpandables.Net.SampleApi.BankAccounts.Features.GetBankAccountBalance;

public sealed class GetBankAccountBalanceEndpoint : IMinimalEndpointRoute
{
    public void AddRoutes(MinimalRouteBuilder app)
    {
        app.MapGet("/bank-accounts/{accountId}/balance", async (Guid accountId, IMediator mediator) =>
            await mediator.SendAsync(new GetBankAccountBalanceQuery { AccountId = accountId }).ConfigureAwait(false))
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
