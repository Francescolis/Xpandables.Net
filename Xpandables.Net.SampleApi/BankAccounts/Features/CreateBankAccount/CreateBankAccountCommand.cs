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

using Xpandables.Net.SampleApi.EnumerationTypes;

namespace Xpandables.Net.SampleApi.BankAccounts.Features.CreateBankAccount;

public sealed class CreateBankAccountCommand :
    IRequest<CreateBankAccountResult>, IRequiresValidation, IRequiresEventStorage
{
    [Required, StringLength(byte.MaxValue, MinimumLength = 3)]
    public required string Owner { get; init; }

    [Required]
    public required AccountType AccountType { get; init; }

    [Required, EmailAddress]
    public required string Email { get; init; }

    [Range(0, double.MaxValue)]
    public decimal InitialBalance { get; init; }
}

public readonly record struct CreateBankAccountResult
{
    public readonly required string AccountId { get; init; }
    public readonly required string AccountNumber { get; init; }
}