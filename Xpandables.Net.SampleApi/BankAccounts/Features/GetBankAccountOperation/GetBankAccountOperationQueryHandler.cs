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

namespace Xpandables.Net.SampleApi.BankAccounts.Features.GetBankAccountOperation;

public sealed class GetBankAccountOperationQueryHandler(IEventStore eventStore) :
    IStreamPagedRequestHandler<GetBankAccountOperationQuery, GetBankAccountOperationResult>
{
    public async Task<Result<IAsyncPagedEnumerable<GetBankAccountOperationResult>>> HandleAsync(
        GetBankAccountOperationQuery request,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        ReadStreamRequest streamRequest = new()
        {
            StreamId = request.AccountId,
            FromVersion = 0,
            MaxCount = int.MaxValue
        };

        IAsyncPagedEnumerable<GetBankAccountOperationResult> operations = eventStore
            .ReadStreamAsync(streamRequest, cancellationToken)
            .Where(e => e.EventName is nameof(MoneyDepositEvent) or nameof(MoneyWithdrawEvent))
            .Select(e => e.Event switch
                {
                    MoneyDepositEvent deposit => new GetBankAccountOperationResult
                    {
                        AccountId = request.AccountId,
                        Amount = deposit.Amount,
                        Description = deposit.Description,
                        OperationDate = e.OccurredOn.DateTime,
                        OperationType = "Deposit"
                    },
                    MoneyWithdrawEvent withdraw => new GetBankAccountOperationResult
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
