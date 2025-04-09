﻿using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;

namespace Xpandables.Net.Api.Accounts.Endpoints.UnBlockAccount;

[RestPost("/accounts/unblock")]
public sealed record UnBlockAccountRequest : IValidationEnabled
{
    [Required]
    public required Guid KeyId { get; init; }
}
