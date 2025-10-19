# Xpandables.Net Bank Account Demo

This is a comprehensive demonstration of the **Xpandables.Net** library featuring:

## Architecture Overview

### API (Xpandables.Net.Api)
- **Event Sourcing** with `BankAccountAggregate`
- **CQRS Pattern** with Commands and Queries
- **Vertical Slice Architecture** organizing features by use case
- **Mediator Pattern** with full pipeline support
- **Entity Framework Core** with SQLite for event store
- **Minimal API** endpoints using `IEndpointRoute`

### Client (Xpandables.Net.BlazorClient)
- **Blazor WebAssembly** for rich client experience
- **REST Client** using `IRestClient` with attributes
- **Responsive UI** with Bootstrap 5

## Features Demonstrated

### 1. Event Sourcing & Aggregates
- `BankAccountAggregate` - Domain aggregate with event handlers
- Domain events: `BankAccountCreatedEvent`, `MoneyDepositedEvent`, `MoneyWithdrawnEvent`
- Event replay and state reconstruction
- Event versioning and stream management

### 2. CQRS Implementation
**Commands:**
- `CreateBankAccountCommand` - Creates new accounts
- `DepositMoneyCommand` - Deposits money
- `WithdrawMoneyCommand` - Withdraws money

**Queries:**
- `GetBankAccountQuery` - Retrieves account details
- `GetAccountTransactionsQuery` - Streams transactions with paging
- `GetAccountSummaryQuery` - Gets account statistics

### 3. Pipeline Decorators
The API uses the following pipeline decorators:
- **ValidationDecorator** - Validates requests using FluentValidation-style rules
- **ExceptionDecorator** - Handles exceptions gracefully
- **UnitOfWorkDecorator** - Manages database transactions
- **PreDecorator** - Pre-processing pipeline
- **PostDecorator** - Post-processing pipeline

### 4. IAsyncPagedEnumerable
- Efficient streaming of large transaction datasets
- Pagination support built-in
- Memory-efficient processing

### 5. Event Store & Unit of Work
- SQLite-based event store using Entity Framework
- Atomic transaction handling
- Event persistence and retrieval

### 6. Scheduler
- Background service for processing integration events
- Configurable batch processing
- Retry logic and error handling

### 7. REST Client (Blazor)
- Type-safe REST client using attributes
- Automatic serialization/deserialization
- `ExecutionResult` pattern for error handling

## Project Structure

### API Project Structure
```
Xpandables.Net.Api/
??? Features/
?   ??? BankAccounts/
?       ??? Domain/
?       ?   ??? BankAccountAggregate.cs
?       ?   ??? BankAccountEvents.cs
?       ??? Commands/
?       ?   ??? CreateBankAccount.cs
?       ?   ??? TransactionCommands.cs
?       ??? Queries/
?       ?   ??? BankAccountQueries.cs
?       ??? Contracts/
?       ?   ??? BankAccountResponses.cs
?       ??? Endpoints/
?           ??? BankAccountEndpoints.cs
??? Infrastructure/
?   ??? Data/
?       ??? BankingDbContext.cs
??? Program.cs
```

### Client Project Structure
```
Xpandables.Net.BlazorClient/
??? Pages/
?   ??? Accounts/
?       ??? AccountList.razor
?       ??? CreateAccount.razor
?       ??? AccountDetails.razor
??? Services/
?   ??? IBankAccountClient.cs
??? Program.cs
```

## Running the Application

### Prerequisites
- .NET 10 RC2 SDK
- Visual Studio 2022 (17.13+) or VS Code

### Step 1: Build the Solution
```bash
cd C:\Users\fewan\source\repos\Francescolis\Xpandables.Net
dotnet build
```

### Step 2: Run the API
```bash
cd Xpandables.Net.Api
dotnet run
```

The API will start on:
- HTTPS: https://localhost:7001
- HTTP: http://localhost:5001

Swagger UI available at: https://localhost:7001/swagger

### Step 3: Run the Blazor Client
```bash
cd Xpandables.Net.BlazorClient
dotnet run
```

The client will start on:
- HTTPS: https://localhost:5002
- HTTP: http://localhost:5003

## Using the Application

### 1. Create a Bank Account
- Navigate to the Blazor app
- Click "Create New Account"
- Fill in:
  - Owner name (min 3 characters)
  - Email address
  - Initial balance (optional)
- Submit the form

### 2. Make Transactions
- View your account details
- Click "Deposit Money" or "Withdraw Money"
- Enter amount and description
- Submit

### 3. View Transaction History
- All transactions are displayed on the account details page
- Transactions show:
  - Date and time
  - Transaction type (badge-colored)
  - Description
  - Amount (green for deposits, red for withdrawals)
  - Balance after transaction

### 4. View Account Summary
- Summary shows:
  - Current balance
  - Total number of transactions
  - Total deposits
  - Total withdrawals

## API Endpoints

### Bank Accounts
- **POST** `/api/bank-accounts` - Create account
- **GET** `/api/bank-accounts/{id}` - Get account details
- **GET** `/api/bank-accounts/{id}/summary` - Get account summary
- **GET** `/api/bank-accounts/{id}/transactions` - Get transactions (streamed)
- **POST** `/api/bank-accounts/{id}/deposit` - Deposit money
- **POST** `/api/bank-accounts/{id}/withdraw` - Withdraw money

## Key Technologies Used

### Xpandables.Net Libraries
- `Xpandables.Net.Events` - Event sourcing infrastructure
- `Xpandables.Net.Events.EntityFramework` - EF Core event store
- `Xpandables.Net.Tasks` - Mediator and pipeline
- `Xpandables.Net.Validators` - Validation framework
- `Xpandables.Net.Validators.Pipelines` - Validation pipeline decorator
- `Xpandables.Net.ExecutionResults` - Result pattern
- `Xpandables.Net.ExecutionResults.AspNetCore` - ASP.NET Core integration
- `Xpandables.Net.Repositories.EntityFramework` - Repository pattern
- `Xpandables.Net.AspNetCore` - ASP.NET Core utilities
- `Xpandables.Net.Rests` - REST client framework
- `Xpandables.Net.Async` - Async enumerable utilities
- `Xpandables.Net.Optionals` - Optional type

### Additional Technologies
- Entity Framework Core 10
- SQLite
- Blazor WebAssembly
- Bootstrap 5
- Minimal APIs

## Database

The application uses SQLite for simplicity. The database file (`banking.db`) will be created automatically in the API project directory on first run.

### Event Store Schema
- `Events` table - Stores all domain events
- `Snapshots` table - Stores aggregate snapshots (if enabled)
- `Outbox` table - Stores integration events for reliable messaging

## Event Sourcing Flow

1. **Command arrives** ? Validated by ValidationDecorator
2. **Handler loads aggregate** from event store (replays all events)
3. **Aggregate processes command** ? Generates new domain event(s)
4. **Events are persisted** to event store
5. **UnitOfWork commits** transaction
6. **Response returned** with ExecutionResult

## Best Practices Demonstrated

### 1. Vertical Slice Architecture
Each feature is self-contained with:
- Domain logic
- Commands/Queries
- Handlers
- Endpoints
- DTOs

### 2. Separation of Concerns
- Domain logic in aggregates
- Infrastructure in separate layer
- API contracts separate from domain
- Client models separate from API

### 3. Error Handling
- ExecutionResult pattern throughout
- Proper HTTP status codes
- Detailed error messages
- Validation errors

### 4. Type Safety
- Strong typing everywhere
- Required properties
- Nullable reference types
- Generic constraints

### 5. Performance
- Async/await throughout
- Streaming large datasets
- Efficient event replay
- Connection pooling

## Testing the API

### Using Swagger UI
1. Open https://localhost:7001/swagger
2. Test endpoints interactively
3. View request/response schemas

### Using curl

**Create Account:**
```bash
curl -X POST https://localhost:7001/api/bank-accounts \
  -H "Content-Type: application/json" \
  -d '{
    "owner": "John Doe",
    "email": "john@example.com",
    "initialBalance": 1000
  }'
```

**Deposit Money:**
```bash
curl -X POST https://localhost:7001/api/bank-accounts/{id}/deposit \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 500,
    "description": "Salary deposit"
  }'
```

## Troubleshooting

### API won't start
- Ensure port 7001 is available
- Check the database file permissions
- Verify .NET 10 RC2 SDK is installed

### Client can't connect to API
- Verify API is running
- Check CORS configuration in appsettings.json
- Ensure HTTPS certificates are trusted

### Database errors
- Delete `banking.db` and restart (will reset all data)
- Check file permissions
- Verify SQLite provider is installed

## Next Steps

To extend this demo:

1. **Add Authentication** - Integrate with Xpandables.Net.Security
2. **Add Snapshot** support - Configure SnapshotStore
3. **Add Integration Events** - Implement event publishing
4. **Add Read Models** - Create projection handlers
5. **Add SignalR** - Real-time balance updates
6. **Add Tests** - Unit tests for aggregates and handlers

## License

Copyright (C) 2024 Francis-Black EWANE
Licensed under the Apache License, Version 2.0
