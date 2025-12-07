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

using System.Results.Tasks;

namespace Xpandables.Net.SampleApi.BankAccounts.Features.GetBankAccountOperation;

public sealed class GetBankAccountOperationEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/bank-accounts/{accountId}/operations",
            async (Guid accountId, IMediator mediator) =>
                await mediator.SendAsync(new GetBankAccountOperationQuery { AccountId = accountId }).ConfigureAwait(false))
            .WithXMinimalApi()
            .AllowAnonymous()
            .WithTags("BankAccounts")
            .WithName("GetBankAccountOperation")
            .WithSummary("Gets the operations of a bank account.")
            .WithDescription("Retrieves the list of operations for the specified bank account.")
            .Produces400BadRequest()
            .Produces401Unauthorized()
            .Produces404NotFound()
            .Produces500InternalServerError()
            .Produces200OK<IAsyncPagedEnumerable<GetBankAccountOperationResult>>();
    }
}
