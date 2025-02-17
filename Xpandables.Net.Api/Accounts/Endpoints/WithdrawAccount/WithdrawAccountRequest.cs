﻿using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;
using Xpandables.Net.Http.Interfaces;

namespace Xpandables.Net.Api.Accounts.Endpoints.WithdrawAccount;

[HttpClient(Path = "/accounts/withdraw",
    IsNullable = false,
    IsSecured = false,
    Location = HttpClientParameters.Location.Body,
    Method = HttpClientParameters.Method.POST)]
public sealed record WithdrawAccountRequest : IApplyValidation
{
    [Required]
    public required Guid KeyId { get; init; }

    [Required]
    [Range(10, double.MaxValue)]
    public required decimal Amount { get; init; }
}
