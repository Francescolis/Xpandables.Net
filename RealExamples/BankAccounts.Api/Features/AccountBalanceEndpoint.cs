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
using System.Entities;
using System.Results;
using System.Results.Requests;
using System.Results.Tasks;

using BankAccounts.Infrastructure;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Features;

public sealed class AccountBalanceQuery : IRequest<AccountBalanceResult>
{
	[Required, FromRoute]
	public required Guid AccountId { get; init; }

}

public readonly record struct AccountBalanceResult
{
	public readonly required string AccountId { get; init; }
	public readonly required string AccountNumber { get; init; }
	public readonly required decimal Balance { get; init; }
}

public sealed class AccountBalanceEndpoint : IMinimalEndpointRoute
{
	public void AddRoutes(MinimalRouteBuilder app)
	{
		ArgumentNullException.ThrowIfNull(app);

		app.MapGet("/bank-accounts/{accountId:guid:required}/balance",
			async (Guid accountId, IMediator mediator) =>
				await mediator.SendAsync(new AccountBalanceQuery { AccountId = accountId }).ConfigureAwait(false))
				.AllowAnonymous()
				.WithTags("BankAccounts")
				.WithName("GetBankAccountBalance")
				.WithSummary("Gets the balance of a bank account.")
				.WithDescription("Retrieves the current balance of the specified bank account.")
				.Produces200OK<AccountBalanceResult>()
				.Produces400BadRequest()
				.Produces401Unauthorized()
				.Produces404NotFound()
				.Produces500InternalServerError();
	}

}

public sealed class AccountBalanceQueryHandler(AccountDataContext context) : IRequestHandler<AccountBalanceQuery, AccountBalanceResult>
{
	public async Task<Result<AccountBalanceResult>> HandleAsync(
		AccountBalanceQuery request,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		AccountBalanceResult? account = await context.Accounts
			.AsNoTracking()
			.Where(a => a.KeyId == request.AccountId && a.Status == EntityStatus.ACTIVE.Value)
			.Select(a => new AccountBalanceResult
			{
				AccountId = a.KeyId.ToString(),
				AccountNumber = a.AccountNumber,
				Balance = a.Balance
			})
			.FirstOrDefaultAsync(cancellationToken)
			.ConfigureAwait(false);

		return account is { AccountId: not null }
			? Result.Success(account.Value)
			: Result.NotFound<AccountBalanceResult>(nameof(request.AccountId), "Account not found");
	}
}
