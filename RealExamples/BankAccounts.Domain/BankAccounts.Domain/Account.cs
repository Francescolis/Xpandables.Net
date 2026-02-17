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
using System.Events.Aggregates;

namespace BankAccounts.Domain;

public sealed class Account : AggregateStateContext<AccountState>, IAggregateFactory<Account>
{
	public static Account Initialize() => new();
	public static Account Create(
		Guid accountId,
		string accountNumber,
		AccountType accountType,
		string owner,
		string email,
		decimal initialBalance)
	{
		Account account = Initialize();
		var @event = new AccountCreatedEvent
		{
			StreamId = accountId,
			AccountNumber = accountNumber,
			AccountType = accountType,
			Owner = owner,
			Email = email,
			InitialBalance = initialBalance
		};

		account.AppendEvent(@event);
		return account;
	}

	private Account() : base(new AccountStatePending())
	{
		On<AccountCreatedEvent>(@event =>
		{
			StreamId = @event.StreamId;
			CurrentState.AccountNumber = @event.AccountNumber;
			CurrentState.Owner = @event.Owner;
			CurrentState.Email = @event.Email;
			CurrentState.Balance = @event.InitialBalance;
			CurrentState.AccountNumber = @event.AccountNumber;

			TransitionToState(new AccountStateActive());
		});

		On<MoneyDepositEvent>(@event =>
		{
			CurrentState.Balance += @event.Amount;
			CurrentState.AccountNumber = CurrentState.AccountNumber;
		});

		On<MoneyWithdrawEvent>(@event =>
		{
			CurrentState.Balance -= @event.Amount;
			CurrentState.AccountNumber = CurrentState.AccountNumber;
		});

		On<AccountBlockedEvent>(@event => TransitionToState(new AccountStateBlocked()));

		On<AccountUnblockedEvent>(@event => TransitionToState(new AccountStateActive()));
	}

	protected override void OnStateTransitioning(AccountState? currentState, AccountState newState)
	{
		ArgumentNullException.ThrowIfNull(newState);

		if (currentState is not null)
		{
			newState.AccountNumber = currentState.AccountNumber;
			newState.Balance = currentState.Balance;
			newState.Email = currentState.Email;
			newState.Owner = currentState.Owner;
		}
	}
}
