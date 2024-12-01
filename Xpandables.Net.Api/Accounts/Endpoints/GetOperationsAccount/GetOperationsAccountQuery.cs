using Xpandables.Net.Commands;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetOperationsAccount;

public sealed record GetOperationsAccountQuery : IQueryAsync<OperationAccount>
{
    public required Guid KeyId { get; init; }
}


public readonly record struct OperationAccount
{
    public required Guid Id { get; init; }
    public required DateTime Date { get; init; }
    public required decimal Amount { get; init; }
    public required string Type { get; init; }
}