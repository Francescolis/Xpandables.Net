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

namespace Xpandables.Net.SampleApi.BankAccounts.Features.WithdrawBankAccount;

public sealed class WithdrawBankAccountCommandHandler(IAggregateStore<BankAccount> aggregateStore) :
    IRequestHandler<WithdrawBankAccountCommand, WithdrawBankAccountResult>
{
    public async Task<Result<WithdrawBankAccountResult>> HandleAsync(
        WithdrawBankAccountCommand request,
        CancellationToken cancellationToken = default)
    {
        var bankAccount = await aggregateStore
            .LoadAsync(request.AccountId, cancellationToken)
            .ConfigureAwait(false);

        bankAccount.CurrentState.Withdraw(request.Amount, request.Currency, request.Description);

        await aggregateStore
            .SaveAsync(bankAccount, cancellationToken)
            .ConfigureAwait(false);

        var result = new WithdrawBankAccountResult
        {
            AccountId = bankAccount.StreamId.ToString(),
            AccountNumber = bankAccount.CurrentState.AccountNumber,
            NewBalance = bankAccount.CurrentState.Balance,
            WithdrawnOn = DateTime.UtcNow,
            Description = request.Description
        };

        return Result.Success(result);
    }
}
