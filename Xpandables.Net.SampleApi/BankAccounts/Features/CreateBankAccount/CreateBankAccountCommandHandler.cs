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
using Xpandables.Net.SampleApi.BankAccounts.Accounts;

namespace Xpandables.Net.SampleApi.BankAccounts.Features.CreateBankAccount;

public sealed class CreateBankAccountCommandHandler(
    IAggregateStore<BankAccount> aggregateStore) : IRequestHandler<CreateBankAccountCommand>
{
    public async Task<Result> HandleAsync(
        CreateBankAccountCommand request,
        CancellationToken cancellationToken = default)
    {
        var accountNumber = GenerateAccountNumber();
        var accountId = Guid.NewGuid();

        var bankAccount = BankAccount.Create(
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
        $"ACC{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
}
