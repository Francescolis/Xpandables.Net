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
using System.Events.Aggregates;
using System.Results;
using System.Results.Requests;

namespace BankAccounts.Domain.Features.WithdrawAccount;

public sealed class WithdrawAccountCommandHandler(IAggregateStore<Account> aggregateStore) :
	IRequestHandler<WithdrawAccountCommand, WithdrawAccountResult>
{
	public async Task<Result<WithdrawAccountResult>> HandleAsync(
		WithdrawAccountCommand request,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		Account account = await aggregateStore
			.LoadAsync(request.AccountId, cancellationToken)
			.ConfigureAwait(false);

		account.CurrentState.Withdraw(request.Amount, request.Currency, request.Description);

		await aggregateStore
			.SaveAsync(account, cancellationToken)
			.ConfigureAwait(false);

		var result = new WithdrawAccountResult
		{
			AccountId = account.StreamId.ToString(),
			AccountNumber = account.CurrentState.AccountNumber,
			NewBalance = account.CurrentState.Balance,
			Description = request.Description
		};

		return Result.Success(result);
	}
}
