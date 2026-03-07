/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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

using BankAccounts.Domain.Features.WithdrawAccount;

using Microsoft.AspNetCore.Mvc;

namespace BankAccounts.Api.Features;

public sealed record WithdrawAccountRequest : IRequiresValidation
{
	[Required]
	[Range(0.01, double.MaxValue)]
	public required decimal Amount { get; init; }
	[Required]
	[StringLength(3, MinimumLength = 3)]
	public required string Currency { get; init; }
	[Required]
	[StringLength(250, MinimumLength = 3)]
	public required string Description { get; init; }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>")]
public sealed class WithdrawAccountEndpoint : IMinimalEndpointRoute
{
	public void AddRoutes(MinimalRouteBuilder app)
	{
		ArgumentNullException.ThrowIfNull(app);

		app.MapPost("/bank-accounts/{accountId:guid:required}/withdraw",
			async (
				[FromRoute] Guid accountId,
				WithdrawAccountRequest request,
				IMediator mediator) =>
				await mediator.SendAsync(new WithdrawAccountCommand
				{
					AccountId = accountId,
					Amount = request.Amount,
					Currency = request.Currency,
					Description = request.Description
				})
			.ConfigureAwait(false))
			.AllowAnonymous()
			.WithTags("BankAccounts")
			.WithName("WithdrawBankAccount")
			.WithSummary("Withdraws money from a bank account.")
			.WithDescription("Withdraws money from a bank account with the provided details.")
			.Accepts<WithdrawAccountRequest>()
			.Produces201Created<WithdrawAccountResult>()
			.Produces400BadRequest()
			.Produces401Unauthorized()
			.Produces500InternalServerError();
	}
}
