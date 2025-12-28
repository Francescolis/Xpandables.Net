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
using System.Events;

using Microsoft.EntityFrameworkCore;

using Xpandables.Net.Worker.CrossEvents;

namespace Xpandables.Net.Worker.ReadStorage;

public sealed class MoneyDepositWithdrawIntegrationEventHandler(BankAccountDataContext context) :
    IEventHandler<MoneyDepositWithdrawIntegrationEvent>
{
    public async Task HandleAsync(MoneyDepositWithdrawIntegrationEvent eventInstance, CancellationToken cancellationToken = default)
    {
        await context.BankAccounts
            .Where(b => b.KeyId == eventInstance.BankAccountId)
            .ExecuteUpdateAsync(
                b => b
                    .SetProperty(b => b.Balance, b => b.Balance + eventInstance.Amount)
                    .SetProperty(b => b.UpdatedOn, DateTime.UtcNow),
                cancellationToken);
    }
}
