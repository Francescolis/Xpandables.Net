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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

public sealed class AccountTypeEntityConfiguration : IEntityTypeConfiguration<AccountEntity>
{
	public void Configure(EntityTypeBuilder<AccountEntity> builder)
	{
		ArgumentNullException.ThrowIfNull(builder);

		builder.HasKey(a => a.KeyId);

		builder.Property(a => a.AccountNumber)
			.IsRequired()
			.HasMaxLength(50);

		builder.Property(a => a.AccountType)
			.IsRequired()
			.HasMaxLength(50);

		builder.Property(a => a.Owner)
			.IsRequired()
			.HasMaxLength(100);

		builder.Property(a => a.Email)
			.IsRequired()
			.HasMaxLength(100);

		builder.Property(a => a.Balance)
			.IsRequired()
			.HasColumnType("decimal(18,2)");

		builder.Property(a => a.CreatedOn)
			.HasDefaultValueSql("GETUTCDATE()");

		builder.Property(a => a.UpdatedOn);

		builder.Property(a => a.DeletedOn);

		builder.Property(a => a.Status)
			.IsRequired()
			.HasMaxLength(20);
	}
}
