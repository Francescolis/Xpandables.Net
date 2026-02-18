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
using System.Events.Domain;
using System.Results;
using System.Results.Requests;

namespace BankAccounts.Domain.Features.OperationAccount;

public sealed class AccountOperationQueryHandler(IDomainStore eventStore) :
	IStreamPagedRequestHandler<AccountOperationQuery, AccountOperationResult>
{
	public async Task<Result<IAsyncPagedEnumerable<AccountOperationResult>>> HandleAsync(
		AccountOperationQuery request,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		await Task.Yield();

		ReadStreamRequest streamRequest = new()
		{
			StreamId = request.AccountId,
			FromVersion = 0,
			MaxCount = int.MaxValue
		};

		IAsyncPagedEnumerable<AccountOperationResult> operations = eventStore
			.ReadStreamAsync(streamRequest, cancellationToken)
			.Where(e => e.EventName is nameof(MoneyDepositEvent) or nameof(MoneyWithdrawEvent))
			.Select(e => e.Event switch
				{
					MoneyDepositEvent deposit => new AccountOperationResult
					{
						AccountId = request.AccountId,
						Amount = deposit.Amount,
						Description = deposit.Description,
						OperationDate = e.OccurredOn.DateTime,
						OperationType = "Deposit"
					},
					MoneyWithdrawEvent withdraw => new AccountOperationResult
					{
						AccountId = request.AccountId,
						Amount = withdraw.Amount,
						Description = withdraw.Description,
						OperationDate = e.OccurredOn.DateTime,
						OperationType = "Withdraw"
					},
					_ => throw new InvalidOperationException("Unexpected event type.")
				}).ToAsyncPagedEnumerable();

		return Result.Success(operations);
	}
}
