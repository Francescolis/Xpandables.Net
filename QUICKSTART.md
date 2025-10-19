# Xpandables.Net Demo Application - Quick Start

## ?? What's Been Created

A complete **Event-Sourced Bank Account Management System** demonstrating all major features of Xpandables.Net:

### API (Xpandables.Net.Api)
? **Event Sourcing** with BankAccountAggregate
? **CQRS** with Commands and Queries  
? **Mediator Pattern** with pipeline decorators
? **Vertical Slice Architecture**
? **Entity Framework Core** event store (SQLite)
? **Minimal API** endpoints with `IEndpointRoute`
? **Validation** with FluentValidation-style rules
? **UnitOfWork** pattern
? **IAsyncPagedEnumerable** for streaming
? **Scheduler** for background processing

### Client (Xpandables.Net.BlazorClient)
? **Blazor WebAssembly** SPA
? **REST Client** with `IRestClient` attributes
? **ExecutionResult** pattern for error handling
? **Bootstrap 5** responsive UI

## ?? Running the Demo

### 1. Start the API
```bash
cd Xpandables.Net.Api
dotnet run
```
Access Swagger UI: https://localhost:7001/swagger

### 2. Start the Blazor Client
```bash
cd Xpandables.Net.BlazorClient
dotnet run
```
Access app: https://localhost:5002

## ?? Key Files Created

### API Structure
```
Xpandables.Net.Api/
??? Features/BankAccounts/
?   ??? Domain/
?   ?   ??? BankAccountAggregate.cs      ? Event-sourced aggregate
?   ?   ??? BankAccountEvents.cs         ? Domain events
?   ??? Commands/
?   ?   ??? CreateBankAccount.cs         ? Create account command
?   ?   ??? TransactionCommands.cs       ? Deposit/Withdraw commands
?   ??? Queries/
?   ?   ??? BankAccountQueries.cs        ? CQRS queries with streaming
?   ??? Contracts/
?   ?   ??? BankAccountResponses.cs      ? DTOs
?   ??? Endpoints/
?       ??? BankAccountEndpoints.cs      ? Minimal API endpoints
??? Infrastructure/Data/
?   ??? BankingDbContext.cs              ? Event store context
??? Program.cs                            ? DI configuration
```

### Client Structure
```
Xpandables.Net.BlazorClient/
??? Pages/Accounts/
?   ??? AccountList.razor                ? List view
?   ??? CreateAccount.razor              ? Create form
?   ??? AccountDetails.razor             ? Details with transactions
??? Services/
?   ??? IBankAccountClient.cs            ? REST client interface
??? Program.cs                            ? Client configuration
```

## ?? Features Demonstrated

### 1. Event Sourcing
- Aggregate root with event handlers
- Event replay for state reconstruction
- Event persistence in SQL
- Stream versioning

### 2. Pipeline Decorators
All commands/queries go through:
- ? **Validation** decorator
- ? **Exception** handling decorator
- ? **UnitOfWork** decorator  
- ? **Pre/Post** processing decorators

### 3. IAsyncPagedEnumerable
```csharp
// Streaming large transaction datasets
await foreach (var tx in transactionsResult.Value)
{
    transactions.Add(tx);
}
```

### 4. REST Client with Attributes
```csharp
[RestClient(BaseUrl = "https://localhost:7001")]
public interface IBankAccountClient
{
    [RestPost("/api/bank-accounts")]
    Task<ExecutionResult<BankAccountResponse>> CreateAccountAsync(...);
    
    [RestGet("/api/bank-accounts/{accountId}/transactions")]
    Task<ExecutionResult<IAsyncEnumerable<TransactionResponse>>> GetTransactionsAsync(...);
}
```

### 5. ExecutionResult Pattern
```csharp
var result = await mediator.SendAsync(command);
if (result.IsSuccess)
{
    // Success path
}
else
{
    // Handle errors with result.Errors, result.Detail
}
```

## ?? Test Scenarios

### Scenario 1: Create Account
1. Open Blazor app
2. Click "Create New Account"
3. Enter: Owner="John Doe", Email="john@test.com", InitialBalance=1000
4. Account created with unique account number

### Scenario 2: Make Transactions
1. View account details
2. Click "Deposit Money" ? Enter $500
3. Click "Withdraw Money" ? Enter $200
4. Balance updates: $1000 ? $1500 ? $1300
5. All transactions appear in history

### Scenario 3: View Event Stream
1. Make several transactions
2. View transaction history (streamed via IAsyncPagedEnumerable)
3. Each event shows: Type, Amount, Description, Running Balance
4. Summary shows: Total Deposits, Total Withdrawals

### Scenario 4: Business Rules
1. Try to withdraw more than balance ? ? Error: "Insufficient funds"
2. Try negative deposit ? ? Error: "Amount must be positive"
3. Try deposit to closed account ? ? Error: "Cannot deposit to closed account"

## ?? Event Sourcing Flow

```
1. User Action (Blazor)
   ?
2. REST Client (IBankAccountClient)
   ?
3. API Endpoint (Minimal API)
   ?
4. Mediator Pipeline
   ?? ValidationDecorator
   ?? ExceptionDecorator  
   ?? UnitOfWorkDecorator
      ?
5. Command Handler
   ?? Load Aggregate (from event stream)
   ?? Execute Business Logic
   ?? Generate Domain Events
      ?
6. Event Store
   ?? Persist Events (SQLite)
      ?
7. Return ExecutionResult
   ?
8. Client Updates UI
```

## ?? Database Inspection

The SQLite database (`banking.db`) contains:

### Events Table
| Column | Description |
|--------|-------------|
| EventId | Unique event identifier |
| StreamId | Aggregate ID (AccountId) |
| StreamVersion | Event sequence number |
| EventType | BankAccountCreatedEvent, etc. |
| EventData | JSON serialized event |
| OccurredOn | Timestamp |

View events:
```bash
sqlite3 banking.db
SELECT EventId, StreamId, StreamVersion, EventType FROM Events ORDER BY StreamVersion;
```

## ?? Key Takeaways

1. **Event Sourcing** = Complete audit trail of all changes
2. **CQRS** = Separate read and write models
3. **Mediator** = Centralized request handling with pipelines
4. **Vertical Slices** = Feature-based organization
5. **ExecutionResult** = Unified error handling
6. **IAsyncPagedEnumerable** = Efficient data streaming
7. **REST Client** = Type-safe API communication

## ?? Next Steps

- Add **Snapshot** support for performance
- Implement **Integration Events** for cross-service communication
- Add **Authentication** and **Authorization**
- Create **Read Models** (projections) for complex queries
- Add **SignalR** for real-time updates
- Implement **Saga** pattern for distributed transactions

## ?? Common Issues

**Issue:** Port already in use
**Solution:** Change ports in launchSettings.json

**Issue:** CORS errors in browser
**Solution:** Verify CORS configuration in appsettings.json

**Issue:** Database locked
**Solution:** Close all connections, restart API

## ?? Documentation

See **README-DEMO.md** for complete documentation including:
- Detailed architecture explanation
- All API endpoints
- Testing instructions
- Troubleshooting guide
- Extension ideas

---

**Built with .NET 10 RC2, C# 14, Xpandables.Net v3.0**
