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

namespace BankAccounts.Domain.Features.CreateAccount;

public sealed class AccountCreatedEventHandler(IPendingIntegrationEventsBuffer pendingIntegration) : IEventHandler<AccountCreatedEvent>
{
	public async Task HandleAsync(AccountCreatedEvent @event, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(@event);

		await Task.Yield();

		AccountCreatedIntegrationEvent createdEvent = new()
		{
			AccountId = @event.StreamId,
			AccountNumber = @event.AccountNumber,
			AccountType = @event.AccountType,
			Owner = @event.Owner,
			Email = @event.Email,
			Balance = @event.InitialBalance
		};

		pendingIntegration.Add(createdEvent);
	}
}
