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

using BankAccounts.Domain.Features.DepositAccount;

using Microsoft.AspNetCore.Mvc;

namespace BankAccounts.Api.Features;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>")]
public sealed class DepositAccountEndpoint : IMinimalEndpointRoute
{
	public void AddRoutes(MinimalRouteBuilder app)
	{
		ArgumentNullException.ThrowIfNull(app);

		app.MapPost("/bank-accounts/{accountId}/deposit",
			async (
				[FromRoute] Guid accountId,
				DepositAccountCommand command,
				IMediator mediator) =>
		await mediator.SendAsync(command with { AccountId = accountId }).ConfigureAwait(false))
			.AllowAnonymous()
			.WithTags("BankAccounts")
			.WithName("DepositBankAccount")
			.WithSummary("Deposits money into a bank account.")
			.WithDescription("Deposits money into a bank account with the provided details.")
			.Accepts<DepositAccountCommand>()
			.Produces201Created<DepositAccountResult>()
			.Produces400BadRequest()
			.Produces401Unauthorized()
			.Produces500InternalServerError();
	}
}
