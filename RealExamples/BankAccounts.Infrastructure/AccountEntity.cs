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
using System.ComponentModel.DataAnnotations.Schema;
using System.Entities;

namespace BankAccounts.Infrastructure;

[Table("Accounts", Schema = "Bank")]
public sealed class AccountEntity : Entity<Guid>
{
	public required string AccountNumber { get; set; }
	public required string AccountType { get; set; }
	public required string Owner { get; set; }
	public required string Email { get; set; }
	public required decimal Balance { get; set; } = 0m;
}
