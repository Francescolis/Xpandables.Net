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

using BankAccounts.Domain;
using BankAccounts.Infrastructure;

using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Worker.Persistence;

public sealed class MoneyWithdrawIntegrationEventHandler(AccountDataContext context) :
	IEventHandler<MoneyWithdrawIntegrationEvent>, IInboxConsumer
{
	public string Consumer => typeof(MoneyWithdrawIntegrationEventHandler).FullName ??
		nameof(MoneyWithdrawIntegrationEventHandler);
	public async Task HandleAsync(MoneyWithdrawIntegrationEvent @event, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(@event);

		await context.Accounts
			.Where(b => b.KeyId == @event.BankAccountId)
			.ExecuteUpdateAsync(
				b => b
					.SetProperty(b => b.Balance, b => b.Balance + @event.Amount)
					.SetProperty(b => b.UpdatedOn, DateTime.UtcNow),
				cancellationToken)
			.ConfigureAwait(false);
	}
}
