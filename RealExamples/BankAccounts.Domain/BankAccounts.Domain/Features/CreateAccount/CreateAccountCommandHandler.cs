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
using System.Security.Cryptography;

namespace BankAccounts.Domain.Features.CreateAccount;

public sealed class CreateAccountCommandHandler(IAggregateStore<Account> aggregateStore) : IRequestHandler<CreateAccountCommand>
{
	public async Task<Result> HandleAsync(CreateAccountCommand request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		string accountNumber = GenerateAccountNumber();
		var accountId = Guid.NewGuid();

		var bankAccount = Account.Create(
			accountId,
			accountNumber,
			request.AccountType,
			request.Owner,
			request.Email,
			request.InitialBalance);

		await aggregateStore
			.SaveAsync(bankAccount, cancellationToken)
			.ConfigureAwait(false);

		return Result
			.Success()
			.WithHeader("AccountId", accountId.ToString())
			.WithHeader("AccountNumber", accountNumber)
			.Build();
	}

	private static string GenerateAccountNumber() =>
		$"ACC{DateTime.UtcNow:yyyyMMddHHmmss}{RandomNumberGenerator.GetInt32(1000, 9999)}";
}
