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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Domain;

[PrimitiveJsonConverter<AccountType, string>]
[TypeConverter(typeof(PrimitiveTypeConverter<AccountType, string>))]
public readonly record struct AccountType : IPrimitive<AccountType, string>
{
	private const string s_savings = "SAVINGS";
	private const string s_checking = "CHECKING";
	private const string s_business = "BUSINESS";

	public static readonly AccountType Savings = new(s_savings);
	public static readonly AccountType Checking = new(s_checking);
	public static readonly AccountType Business = new(s_business);
	public string Value { get; }
	private AccountType(string value) => Value = value;
	public override string ToString() => Value;
	public static implicit operator string(AccountType self) => self.Value;
	public static string DefaultValue => Savings;
	public static implicit operator AccountType(string value) => Create(value);
	public static bool TryParse(string? s, IFormatProvider? provider, out AccountType result)
	{
		result = default;
		if (string.IsNullOrWhiteSpace(s))
		{
			return false;
		}
		try
		{
			result = Create(s);
			return true;
		}
		catch (ValidationException)
		{
		}

		return false;
	}
	public bool Equals(AccountType other) => Value.Equals(other.Value, StringComparison.Ordinal);
	public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
	public static AccountType Create(string value)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(value);
		return value.ToUpperInvariant() switch
		{
			s_savings => Savings,
			s_checking => Checking,
			s_business => Business,
			_ => throw new ValidationException(
				new ValidationResult($"'{value}' is not a valid account type.", [nameof(AccountType)]), null, value)
		};
	}
}

public sealed class AccountTypeValidationAttribute : ValidationAttribute
{
	public override bool IsValid(object? value)
	{
		return value switch
		{
			AccountType _ => true,
			string accountType => AccountType.TryParse(accountType, null, out _),
			_ => false
		};
	}

	public override string FormatErrorMessage(string name)
		=> $"The account type '{name}' does not match an expected value.";
}
