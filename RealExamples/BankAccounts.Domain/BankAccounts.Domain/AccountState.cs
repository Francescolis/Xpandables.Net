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
using System.ComponentModel.DataAnnotations;
using System.States;

namespace BankAccounts.Domain;

public abstract class AccountState : State<Account>
{
	public string AccountNumber { get; internal set; } = string.Empty;
	public string Owner { get; internal set; } = string.Empty;
	public string Email { get; internal set; } = string.Empty;
	public decimal Balance { get; internal set; } = 0m;
	public virtual void Deposit(decimal amount, string curreny, string description) => throw new UnauthorizedAccessException();
	public virtual void Withdraw(decimal amount, string curreny, string description) => throw new UnauthorizedAccessException();
	public virtual void Block(string reason) => throw new UnauthorizedAccessException();
	public virtual void UnBlock(string reason) => throw new UnauthorizedAccessException();
	public virtual void Close(string reason) => throw new UnauthorizedAccessException();
}

public sealed class AccountStatePending : AccountState;

public sealed class AccountStateBlocked : AccountState
{
	public override void UnBlock(string reason)
	{
		if (string.IsNullOrEmpty(reason))
		{
			throw new ValidationException(
				new ValidationResult(
					"The unblock reason can not be null.", [nameof(reason)]), null, reason);
		}

		AccountUnblockedEvent @event = new()
		{
			Reason = reason,
			StreamId = Context.StreamId,
			StreamName = Context.StreamName,
			UnBlockedOn = DateTime.UtcNow
		};

		Context.AppendEvent(@event);
	}
}
public sealed class AccountStateActive : AccountState
{
	public override void Block(string reason)
	{
		if (string.IsNullOrEmpty(reason))
		{
			throw new ValidationException(
				new ValidationResult(
					"The block reason can not be null.", [nameof(reason)]), null, reason);
		}

		AccountBlockedEvent @event = new()
		{
			BlockedOn = DateTime.UtcNow,
			Reason = reason,
			StreamId = Context.StreamId,
			StreamName = Context.StreamName
		};

		Context.AppendEvent(@event);
	}


	public override void Close(string reason)
	{
		if (string.IsNullOrEmpty(reason))
		{
			throw new ValidationException(
				new ValidationResult(
					"The close reason can not be null.", [nameof(reason)]), null, reason);
		}

		AccountClosedEvent @event = new()
		{
			Reason = reason,
			ClosedOn = DateTime.UtcNow,
			StreamId = Context.StreamId,
			StreamName = Context.StreamName
		};

		Context.AppendEvent(@event);
	}

	public override void Deposit(decimal amount, string curreny, string description)
	{
		if (amount <= 0m)
		{
			throw new ValidationException(
				new ValidationResult(
					"Deposit amount must be greater than zero.", [nameof(amount)]), null, amount);
		}

		if (!curreny.Equals("EUR", StringComparison.OrdinalIgnoreCase))
		{
			throw new ValidationException(
				new ValidationResult(
					"Only EUR currency is accepted for deposit.", [nameof(curreny)]), null, curreny);
		}

		MoneyDepositEvent @event = new()
		{
			StreamId = Context.StreamId,
			StreamName = Context.StreamName,
			Amount = amount,
			Currency = curreny,
			Description = description
		};

		Context.AppendEvent(@event);
	}

	public override void Withdraw(decimal amount, string curreny, string description)
	{
		if (amount <= 0m)
		{
			throw new ValidationException(
				new ValidationResult(
					"Withdraw amount must be greater than zero.", [nameof(amount)]), null, amount);
		}

		if (!curreny.Equals("EUR", StringComparison.OrdinalIgnoreCase))
		{
			throw new ValidationException(
				new ValidationResult(
					"Only EUR currency is accepted for withdraw.", [nameof(curreny)]), null, curreny);
		}

		if (Balance - amount < 0m)
		{
			throw new ValidationException(
				new ValidationResult(
					"Insufficient funds for this withdraw.", [nameof(amount)]), null, amount);
		}

		MoneyWithdrawEvent @event = new()
		{
			StreamId = Context.StreamId,
			StreamName = Context.StreamName,
			Amount = amount,
			Currency = curreny,
			Description = description
		};

		Context.AppendEvent(@event);
	}
}
