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
namespace Xpandables.Net.SampleApi.BankAccounts.Features.GetBankAccount;

public sealed class GetBankAccountEndpoint : IMinimalEndpointRoute
{
    public void AddRoutes(MinimalRouteBuilder app)
    {
        app.MapGet("/bank-accounts", async ([AsParameters] GetBankAccountQuery query, IMediator mediator) =>
            await mediator.SendAsync(query).ConfigureAwait(false))
            .AllowAnonymous()
            .WithTags("BankAccounts")
            .WithName("GetBankAccount")
            .WithSummary("Gets a bank account or accounts.")
            .WithDescription("Retrieves the specified bank account or accounts.")
            .Produces200OK<IAsyncPagedEnumerable<GetBankAccountResult>>()
            .Produces400BadRequest()
            .Produces401Unauthorized()
            .Produces500InternalServerError();
    }
}
