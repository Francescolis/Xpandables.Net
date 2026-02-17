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
using System.Results;
using System.Results.Requests;
using System.Results.Tasks;

using BankAccounts.Infrastructure;

using Microsoft.AspNetCore.Mvc;

namespace BankAccounts.Api.Features;

public sealed record AccountQuery : IStreamPagedRequest<AccountResult>
{
	[FromQuery]
	public Guid? AccountId { get; init; }
	[FromQuery]
	public int Count { get; init; } = 10;
}

public readonly record struct AccountResult
{
	public readonly required string AccountId { get; init; }
	public readonly required string AccountNumber { get; init; }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>")]
public sealed class AccountEndpoint : IMinimalEndpointRoute
{
	public void AddRoutes(MinimalRouteBuilder app)
	{
		ArgumentNullException.ThrowIfNull(app);

		app.MapGet("/bank-accounts", async ([AsParameters] AccountQuery query, IMediator mediator) =>
			await mediator.SendAsync(query).ConfigureAwait(false))
			.AllowAnonymous()
			.WithTags("BankAccounts")
			.WithName("GetBankAccount")
			.WithSummary("Gets a bank account or accounts.")
			.WithDescription("Retrieves the specified bank account or accounts.")
			.Produces200OK<IAsyncPagedEnumerable<AccountResult>>()
			.Produces400BadRequest()
			.Produces401Unauthorized()
			.Produces500InternalServerError();
	}
}

public sealed class AccountQueryHandler(AccountDataContext context) : IStreamPagedRequestHandler<AccountQuery, AccountResult>
{
	public async Task<Result<IAsyncPagedEnumerable<AccountResult>>> HandleAsync(
		AccountQuery request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		await Task.Yield();

		IAsyncPagedEnumerable<AccountResult> accounts = context.Accounts
			.Where(a => request.AccountId == null || a.KeyId == request.AccountId)
			.Take(request.Count)
			.Select(a => new AccountResult
			{
				AccountId = a.KeyId.ToString(),
				AccountNumber = a.AccountNumber
			})
			.ToAsyncPagedEnumerable();

		return Result.Success(accounts);
	}
}
