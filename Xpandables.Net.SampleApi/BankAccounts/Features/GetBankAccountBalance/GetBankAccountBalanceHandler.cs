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
using Microsoft.EntityFrameworkCore;

using Xpandables.Net.SampleApi.ReadStorage;

namespace Xpandables.Net.SampleApi.BankAccounts.Features.GetBankAccountBalance;

public sealed class GetBankAccountBalanceHandler(BankAccountDataContext context) :
    IRequestHandler<GetBankAccountBalanceQuery, GetBankAccountBalanceResult>
{
    public async Task<Result<GetBankAccountBalanceResult>> HandleAsync(
        GetBankAccountBalanceQuery request,
        CancellationToken cancellationToken = default)
    {
        GetBankAccountBalanceResult? account = await context.BankAccounts
            .AsNoTracking()
            .Where(a => a.KeyId == request.AccountId)
            .Select(a => new GetBankAccountBalanceResult
            {
                AccountId = a.KeyId.ToString(),
                AccountNumber = a.AccountNumber,
                Balance = a.Balance
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return account is { AccountId: not null }
            ? Result.Success(account.Value)
            : Result.NotFound<GetBankAccountBalanceResult>(nameof(request.AccountId), "Account not found");
    }
}
