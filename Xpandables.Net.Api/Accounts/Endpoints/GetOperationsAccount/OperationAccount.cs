namespace Xpandables.Net.Api.Accounts.Endpoints.GetOperationsAccount;

public record OperationAccount
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty; // Initialize to prevent null warnings
}
