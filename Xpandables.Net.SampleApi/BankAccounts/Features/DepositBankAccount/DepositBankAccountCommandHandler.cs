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

namespace Xpandables.Net.SampleApi.BankAccounts.Features.DepositBankAccount;

public sealed class DepositBankAccountCommandHandler(IAggregateStore<BankAccount> aggregateStore) :
    IRequestHandler<DepositBankAccountCommand, DepositBankAccountResult>
{
    public async Task<Result<DepositBankAccountResult>> HandleAsync(
        DepositBankAccountCommand request,
        CancellationToken cancellationToken = default)
    {
        BankAccount account = await aggregateStore.LoadAsync(request.AccountId, cancellationToken);

        account.CurrentState.Deposit(request.Amount, request.Currency, request.Description);

        await aggregateStore.SaveAsync(account, cancellationToken);

        var result = new DepositBankAccountResult()
        {
            AccountId = account.StreamId.ToString(),
            NewBalance = account.CurrentState.Balance,
            AccountNumber = account.CurrentState.AccountNumber,
            DepositedOn = DateTime.UtcNow,
            Description = request.Description
        };

        return Result.Success(result);
    }
}
