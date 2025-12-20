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
using Microsoft.AspNetCore.Mvc;

namespace Xpandables.Net.SampleApi.BankAccounts.Features.GetBankAccount;

public sealed record GetBankAccountQuery : IStreamPagedRequest<GetBankAccountResult>
{
    [FromQuery]
    public Guid? AccountId { get; init; }
    [FromQuery]
    public int Count { get; init; } = 10;
}

public readonly record struct GetBankAccountResult
{
    public readonly required string AccountId { get; init; }
    public readonly required string AccountNumber { get; init; }
}