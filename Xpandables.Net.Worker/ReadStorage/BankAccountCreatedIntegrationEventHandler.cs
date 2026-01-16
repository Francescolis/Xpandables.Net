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

using Xpandables.Net.Worker.CrossEvents;

namespace Xpandables.Net.Worker.ReadStorage;

public sealed class BankAccountCreatedIntegrationEventHandler(BankAccountDataContext context) :
    IEventHandler<BankAccountCreateIntegrationEvent>, IInboxConsumer
{
    public string Consumer => typeof(BankAccountCreatedIntegrationEventHandler).FullName ??
        nameof(BankAccountCreatedIntegrationEventHandler);

    public async Task HandleAsync(BankAccountCreateIntegrationEvent eventInstance, CancellationToken cancellationToken = default)
    {
        BankAccountEntity bankAccount = new()
        {
            KeyId = eventInstance.AccountId,
            AccountNumber = eventInstance.AccountNumber,
            AccountType = eventInstance.AccountType,
            Owner = eventInstance.Owner,
            Email = eventInstance.Email,
            Balance = eventInstance.Balance
        };

        await context.BankAccounts.AddAsync(bankAccount, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
