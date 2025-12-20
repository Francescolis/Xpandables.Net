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

using Xpandables.Net.SampleApi.ReadStorage;

namespace Xpandables.Net.SampleApi.BankAccounts.Features.GetBankAccount;

public sealed class GetBankAccountQueryHandler(BankAccountDataContext context) :
    IStreamPagedRequestHandler<GetBankAccountQuery, GetBankAccountResult>
{
    public async Task<Result<IAsyncPagedEnumerable<GetBankAccountResult>>> HandleAsync(
        GetBankAccountQuery request, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        IAsyncPagedEnumerable<GetBankAccountResult> accounts = context.BankAccounts
            .Where(a => request.AccountId == null || a.KeyId == request.AccountId)
            .Take(request.Count)
            .Select(a => new GetBankAccountResult
            {
                AccountId = a.KeyId.ToString(),
                AccountNumber = a.AccountNumber
            })
            .ToAsyncPagedEnumerable();

        return Result.Success(accounts);
    }
}
