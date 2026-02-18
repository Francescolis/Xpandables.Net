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
using System.Results.Requests;

namespace BankAccounts.Domain.Features.OperationAccount;

public sealed record AccountOperationQuery : IStreamPagedRequest<AccountOperationResult>
{
	[Required]
	public required Guid AccountId { get; init; }
}

public readonly record struct AccountOperationResult
{
	public required readonly Guid AccountId { get; init; }
	public required readonly decimal Amount { get; init; }
	public required readonly string Description { get; init; }
	public required readonly DateTime OperationDate { get; init; }
	public required readonly string OperationType { get; init; }
}
