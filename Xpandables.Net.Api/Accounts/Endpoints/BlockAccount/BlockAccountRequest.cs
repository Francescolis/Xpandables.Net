﻿using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Executions.Rests;

namespace Xpandables.Net.Api.Accounts.Endpoints.BlockAccount;

[RestPost("/accounts/block")]
public sealed record BlockAccountRequest : IValidationEnabled
{
    [Required]
    public required Guid KeyId { get; init; }
}
