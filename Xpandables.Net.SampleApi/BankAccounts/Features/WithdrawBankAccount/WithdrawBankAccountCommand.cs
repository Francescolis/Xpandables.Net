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

namespace Xpandables.Net.SampleApi.BankAccounts.Features.WithdrawBankAccount;

public sealed record WithdrawBankAccountCommand :
    IRequest<WithdrawBankAccountResult>, IRequiresValidation, IRequiresEventStorage
{
    internal Guid AccountId { get; init; }
    [Required]
    [Range(0.01, double.MaxValue)]
    public required decimal Amount { get; init; }
    [Required]
    [StringLength(3, MinimumLength = 3)]
    public required string Currency { get; init; }
    [Required]
    [StringLength(250, MinimumLength = 3)]
    public required string Description { get; init; }
}

public readonly record struct WithdrawBankAccountResult
{
    public required string AccountId { get; init; }
    public required string AccountNumber { get; init; }
    public required decimal NewBalance { get; init; }
    public required DateTime WithdrawnOn { get; init; }
    public required string Description { get; init; }
}
