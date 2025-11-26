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
using System.Events.Integration;

using Xpandables.Net.SampleApi.BankAccounts.Accounts;
using Xpandables.Net.SampleApi.CrossEvents;

namespace Xpandables.Net.SampleApi.BankAccounts.Features.DepositBankAccount;

public sealed class MoneyDepositEventHandler(IPendingIntegrationEventsBuffer pendingIntegration) : IEventHandler<MoneyDepositEvent>
{
    public Task HandleAsync(MoneyDepositEvent eventInstance, CancellationToken cancellationToken = default)
    {
        var integrationEvent = new MoneyDepositWithdrawIntegrationEvent
        {
            BankAccountId = eventInstance.StreamId,
            Amount = eventInstance.Amount,
        };

        pendingIntegration.Add(integrationEvent);

        return Task.CompletedTask;
    }
}
