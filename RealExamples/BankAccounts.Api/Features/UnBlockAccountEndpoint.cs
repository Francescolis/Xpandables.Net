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
using System.ComponentModel.DataAnnotations;
using System.Results.Tasks;

using BankAccounts.Domain.Features.UnBlockAccount;

using Microsoft.AspNetCore.Mvc;

namespace BankAccounts.Api.Features;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>")]
public sealed class UnBlockAccountEndpoint : IMinimalEndpointRoute
{
	public void AddRoutes(MinimalRouteBuilder app)
	{
		ArgumentNullException.ThrowIfNull(app);

		app.MapPost("/bank-accounts/{accountId:guid:required}/unblock",
			async (
				[FromRoute] Guid accountId,
				[FromBody, Required] string reason,
				IMediator mediator) =>
			await mediator.SendAsync(new UnBlockAccountCommand
			{
				AccountId = accountId,
				Reason = reason
			})
			.ConfigureAwait(false))
			.AllowAnonymous()
			.WithTags("BankAccounts")
			.WithName("UnBlockBankAccount")
			.WithSummary("UnBlock bank account.")
			.WithDescription("UnBlocks the specify bank account.")
			.Produces200OK()
			.Produces400BadRequest()
			.Produces401Unauthorized()
			.Produces500InternalServerError();
	}
}
