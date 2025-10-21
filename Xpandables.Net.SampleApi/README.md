# ?? Xpandables.Net.SampleApi

[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-ready-blue.svg)](https://www.docker.com/)

> **Event Sourcing Banking Sample API** - A complete, production-ready example demonstrating Event Sourcing, CQRS, and Clean Architecture using Xpandables.Net

---

## ?? Overview

This is a fully functional banking API that demonstrates how to build an event-sourced application using **Xpandables.Net**. It showcases best practices for:

- ? **Event Sourcing** - Complete event-driven architecture
- ? **CQRS** - Command Query Responsibility Segregation
- ? **Domain-Driven Design** - Aggregates, events, and commands
- ? **Minimal API** - Clean endpoint definitions
- ? **EF Core** - Event store and outbox pattern
- ? **Swagger/OpenAPI** - Comprehensive API documentation
- ? **Docker Support** - Production-ready containerization

---

## ?? Quick Start

### Prerequisites

- **.NET 10 SDK** or later
- **SQL Server** (LocalDB, Express, or full version)
- **Docker** (optional, for containerized deployment)

### Step 1: Clone the Repository

```bash
git clone https://github.com/Francescolis/Xpandables.Net.git
cd Xpandables.Net/Xpandables.Net.SampleApi
```

### Step 2: Configure Database Connection

Update `appsettings.json` with your SQL Server connection string:

```json
{
  "ConnectionStrings": {
    "EventStoreDb": "Server=(localdb)\\mssqllocaldb;Database=EventStoreDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "SwaggerOptions": {
    "Name": "Xpandables.Net Sample API",
    "Version": "v1",
    "Description": "Event Sourcing Banking Sample API",
    "RoutePrefix": "",
    "Prefix": "",
    "TermsOfService": "https://example.com/terms",
    "Contact": {
      "Name": "Kamersoft",
      "Email": "support@kamersoft.com",
      "Url": "https://github.com/Francescolis/Xpandables.Net"
    },
    "License": {
      "Name": "Apache 2.0",
      "Url": "https://www.apache.org/licenses/LICENSE-2.0.html"
    }
  }
}
```

### Step 3: Run the Application

```bash
dotnet run
```

The application will:
1. Start on `https://localhost:5001` (HTTPS) and `http://localhost:5000` (HTTP)
2. Automatically run EF Core migrations
3. Create the Event Store and Outbox databases
4. Open Swagger UI at `https://localhost:5001/swagger`

---

## ?? Project Structure

```
Xpandables.Net.SampleApi/
??? BankAccounts/
?   ??? Domain/
?   ?   ??? BankAccountAggregate.cs          # Aggregate root
?   ?   ??? Events/
?   ?       ??? BankAccountCreatedEvent.cs
?   ?       ??? MoneyDepositedEvent.cs
?   ?       ??? MoneyWithdrawnEvent.cs
?   ??? Features/
?       ??? CreateBankAccount/
?       ?   ??? CreateBankAccountCommand.cs
?       ?   ??? CreateBankAccountHandler.cs
?       ?   ??? CreateBankAccountValidator.cs
?       ?   ??? CreateBankAccountEndpoint.cs
?       ??? DepositMoney/
?       ??? WithdrawMoney/
??? EventStorage/
?   ??? EventStoreDataContext.cs              # EF Core DbContext for events
?   ??? EventStoreModelCustomizer.cs          # Event store configuration
?   ??? OutboxStoreDataContext.cs             # Outbox pattern DbContext
?   ??? OutboxStoreModelCustomizer.cs         # Outbox configuration
??? Program.cs                                 # Application entry point
??? appsettings.json                          # Configuration
??? Dockerfile                                # Docker configuration
??? README.md                                  # This file
```

---

## ??? Architecture

### Domain Layer: Aggregates and Events

**Bank Account Aggregate:**

```csharp
using Xpandables.Net.Aggregates;
using Xpandables.Net.Events;

public sealed class BankAccountAggregate : Aggregate
{
    public string AccountNumber { get; private set; } = default!;
    public string AccountHolder { get; private set; } = default!;
    public decimal Balance { get; private set; }

    // Factory method
    public static BankAccountAggregate Create(
        string accountNumber,
        string accountHolder,
        decimal initialBalance)
    {
        if (initialBalance < 0)
            throw new InvalidOperationException("Initial balance cannot be negative");

        var aggregate = new BankAccountAggregate();
        
        aggregate.AppendEvent(new BankAccountCreatedEvent(
            Guid.NewGuid(),
            accountNumber,
            accountHolder,
            initialBalance));

        return aggregate;
    }

    // Apply events to rebuild state
    private void On(BankAccountCreatedEvent @event)
    {
        Id = @event.AggregateId;
        AccountNumber = @event.AccountNumber;
        AccountHolder = @event.AccountHolder;
        Balance = @event.InitialBalance;
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Deposit amount must be positive");

        AppendEvent(new MoneyDepositedEvent(Id, amount));
    }

    private void On(MoneyDepositedEvent @event)
    {
        Balance += @event.Amount;
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Withdrawal amount must be positive");
            
        if (Balance < amount)
            throw new InvalidOperationException("Insufficient funds");

        AppendEvent(new MoneyWithdrawnEvent(Id, amount));
    }

    private void On(MoneyWithdrawnEvent @event)
    {
        Balance -= @event.Amount;
    }
}
```

**Domain Events:**

```csharp
using Xpandables.Net.Events;

public sealed record BankAccountCreatedEvent(
    Guid AggregateId,
    string AccountNumber,
    string AccountHolder,
    decimal InitialBalance) : Event(AggregateId)
{
    public override string AggregateName => nameof(BankAccountAggregate);
}
```

### Application Layer: Commands and Handlers

**Command:**

```csharp
using Xpandables.Net.Cqrs;

public sealed record CreateBankAccountCommand(
    string AccountNumber,
    string AccountHolder,
    decimal InitialBalance) : IRequest<CreateBankAccountResult>;

public sealed record CreateBankAccountResult(
    Guid AccountId,
    string AccountNumber);
```

**Handler:**

```csharp
using Xpandables.Net.Cqrs;
using Xpandables.Net.Events;
using Xpandables.Net.ExecutionResults;

public sealed class CreateBankAccountHandler 
    : IRequestHandler<CreateBankAccountCommand, CreateBankAccountResult>
{
    private readonly IAggregateStore _aggregateStore;

    public CreateBankAccountHandler(IAggregateStore aggregateStore) 
        => _aggregateStore = aggregateStore;

    public async Task<ExecutionResult<CreateBankAccountResult>> HandleAsync(
        CreateBankAccountCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Create aggregate
            var account = BankAccountAggregate.Create(
                request.AccountNumber,
                request.AccountHolder,
                request.InitialBalance);

            // Persist aggregate using event sourcing
            await _aggregateStore.AppendAsync(account, cancellationToken);

            var result = new CreateBankAccountResult(
                account.Id,
                request.AccountNumber);

            return ExecutionResult.Created(result);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult
                .BadRequest()
                .WithError("request", ex.Message)
                .Build<CreateBankAccountResult>();
        }
    }
}
```

**Validator:**

```csharp
using Xpandables.Net.Validators;

public sealed class CreateBankAccountValidator 
    : Validator<CreateBankAccountCommand>
{
    public override IReadOnlyCollection<ValidationResult> Validate(
        CreateBankAccountCommand instance)
    {
        var errors = new List<ValidationResult>();

        if (string.IsNullOrWhiteSpace(instance.AccountNumber))
        {
            errors.Add(new ValidationResult(
                "Account number is required",
                [nameof(instance.AccountNumber)]));
        }

        if (string.IsNullOrWhiteSpace(instance.AccountHolder))
        {
            errors.Add(new ValidationResult(
                "Account holder is required",
                [nameof(instance.AccountHolder)]));
        }

        if (instance.InitialBalance < 0)
        {
            errors.Add(new ValidationResult(
                "Initial balance cannot be negative",
                [nameof(instance.InitialBalance)]));
        }

        return errors;
    }
}
```

### Presentation Layer: Endpoints

**Endpoint Definition:**

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Routing;
using Xpandables.Net.Tasks;

public sealed class CreateBankAccountEndpoint : IEndpointRoute
{
    public void AddServices(IServiceCollection services)
    {
        // Register endpoint-specific services if needed
    }

    public void UseServices(WebApplication application)
    {
        // Configure middleware if needed
    }

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/bank-accounts", 
            async (CreateBankAccountCommand command, IMediator mediator) =>
                await mediator.SendAsync(command).ConfigureAwait(false))
            .WithXMinimalApi()
            .AllowAnonymous()
            .WithTags("BankAccounts")
            .WithName("CreateBankAccount")
            .WithSummary("Creates a new bank account.")
            .WithDescription("Creates a new bank account with the provided details.")
            .Accepts<CreateBankAccountCommand>()
            .Produces201Created<CreateBankAccountResult>()
            .Produces400BadRequest()
            .Produces401Unauthorized()
            .Produces500InternalServerError();
    }
}
```

---

## ?? Configuration

### Program.cs - Application Setup

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Events;
using Xpandables.Net.ExecutionResults;
using Xpandables.Net.SampleApi.EventStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

// Add CORS
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("https://localhost:5001", "http://localhost:5000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()));

// Configure SqlServer database for event sourcing
builder.Services.AddXEventStoreDataContext(options =>
    options
        .UseSqlServer(
            builder.Configuration.GetConnectionString("EventStoreDb"),
            options => options
                .EnableRetryOnFailure()
                .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                .MigrationsHistoryTable("__EventStoreMigrations")
                .MigrationsAssembly("Xpandables.Net.SampleApi"))
        .EnableDetailedErrors()
        .EnableSensitiveDataLogging()
        .ReplaceService<IModelCustomizer, EventStoreModelCustomizer>());

builder.Services.AddXOutboxStoreDataContext(options =>
    options
        .UseSqlServer(
            builder.Configuration.GetConnectionString("EventStoreDb"),
            options => options
                .EnableRetryOnFailure()
                .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                .MigrationsHistoryTable("__OutboxStoreMigrations")
                .MigrationsAssembly("Xpandables.Net.SampleApi"))
        .EnableDetailedErrors()
        .EnableSensitiveDataLogging()
        .ReplaceService<IModelCustomizer, OutboxStoreModelCustomizer>());

// Register Xpandables.Net services
builder.Services
    .AddXEndpointRoutes()              // Scan for IEndpointRoute implementations
    .AddXMinimalApi()                   // Add Minimal API support
    .AddXMediatorWithEventSourcing()   // Add CQRS with Event Sourcing
    .AddXRequestHandlers()              // Register all IRequestHandler implementations
    .AddXEventUnitOfWork()             // Add event sourcing unit of work
    .AddXPublisher()                    // Add event publisher
    .AddXAggregateStore()              // Add aggregate store
    .AddXEventStore()                   // Add event store
    .AddXOutboxStore();                 // Add outbox pattern store

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        builder.Configuration["SwaggerOptions:Version"], 
        new OpenApiInfo()
    {
        Title = builder.Configuration["SwaggerOptions:Name"],
        Version = builder.Configuration["SwaggerOptions:Version"],
        Description = builder.Configuration["SwaggerOptions:Description"],
        Contact = builder.Configuration
            .GetSection("SwaggerOptions:Contact")
            .Get<OpenApiContact>(),
        License = builder.Configuration
            .GetSection("SwaggerOptions:License")
            .Get<OpenApiLicense>(),
        TermsOfService = new(
            builder.Configuration["SwaggerOptions:TermsOfService"]!)
    });

    options.EnableAnnotations();
});

var app = builder.Build();

// Use ExecutionResult middleware
app.UseXExecutionResultMinimalMiddleware();

app.UseHttpsRedirection();
app.UseSwagger()
    .UseSwaggerUI(options =>
    {
        string? routePrefix = builder.Configuration["SwaggerOptions:RoutePrefix"];
        string? prefix = builder.Configuration["SwaggerOptions:Prefix"];

        options.SwaggerEndpoint(
            $"{prefix}/swagger/{builder.Configuration["SwaggerOptions:Version"]}/swagger.json",
            builder.Configuration["SwaggerOptions:Description"]);
        options.DocExpansion(DocExpansion.None);
        options.RoutePrefix = routePrefix;
    });

// Run migrations
using (var scope = app.Services.CreateScope())
{
    var eventDb = scope.ServiceProvider
        .GetRequiredService<EventStoreDataContext>();
    var outboxDb = scope.ServiceProvider
        .GetRequiredService<OutboxStoreDataContext>();
    
    await eventDb.Database.MigrateAsync().ConfigureAwait(false);
    await outboxDb.Database.MigrateAsync().ConfigureAwait(false);
}

// Map all IEndpointRoute implementations
app.UseXEndpointRoutes();

await app.RunAsync();
```

---

## ?? Database Schema

### Event Store

The `EventStoreDataContext` manages two main tables:

**EventEntity** - Stores all domain events
```sql
CREATE TABLE EventEntity (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    AggregateId UNIQUEIDENTIFIER NOT NULL,
    AggregateName NVARCHAR(256) NOT NULL,
    EventTypeName NVARCHAR(512) NOT NULL,
    EventData NVARCHAR(MAX) NOT NULL,     -- JSON serialized event
    Version INT NOT NULL,
    OccurredOn DATETIME2 NOT NULL,
    INDEX IX_AggregateId (AggregateId),
    INDEX IX_AggregateName (AggregateName)
);
```

**SnapshotEntity** - Stores aggregate snapshots for performance
```sql
CREATE TABLE SnapshotEntity (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    AggregateId UNIQUEIDENTIFIER NOT NULL,
    AggregateName NVARCHAR(256) NOT NULL,
    SnapshotData NVARCHAR(MAX) NOT NULL,  -- JSON serialized state
    Version INT NOT NULL,
    CreatedOn DATETIME2 NOT NULL
);
```

### Outbox Store

The `OutboxStoreDataContext` implements the Outbox Pattern for reliable event publishing:

**OutboxEntity** - Stores unpublished events
```sql
CREATE TABLE OutboxEntity (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    AggregateId UNIQUEIDENTIFIER NOT NULL,
    EventTypeName NVARCHAR(512) NOT NULL,
    EventData NVARCHAR(MAX) NOT NULL,
    CreatedOn DATETIME2 NOT NULL,
    ProcessedOn DATETIME2 NULL,
    IsProcessed BIT NOT NULL DEFAULT 0,
    INDEX IX_IsProcessed (IsProcessed),
    INDEX IX_CreatedOn (CreatedOn)
);
```

---

## ?? Docker Support

### Build and Run with Docker

```bash
# Build the Docker image
docker build -t xpandables-sample-api .

# Run the container
docker run -d \
  -p 5000:8080 \
  -p 5001:8081 \
  -e ConnectionStrings__EventStoreDb="Server=host.docker.internal;Database=EventStoreDb;User Id=sa;Password=YourPassword;" \
  --name xpandables-api \
  xpandables-sample-api
```

### Docker Compose (Recommended)

```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Password
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql

  api:
    build: .
    ports:
      - "5000:8080"
      - "5001:8081"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080;https://+:8081
      - ConnectionStrings__EventStoreDb=Server=sqlserver;Database=EventStoreDb;User Id=sa;Password=YourStrong@Password;TrustServerCertificate=True
    depends_on:
      - sqlserver

volumes:
  sqlserver-data:
```

**Run with Docker Compose:**

```bash
docker-compose up -d
```

---

## ?? Testing the API

### Using Swagger UI

1. Navigate to `https://localhost:5001/swagger`
2. Try the **POST /bank-accounts** endpoint
3. Use the following sample request:

```json
{
  "accountNumber": "ACC-001",
  "accountHolder": "John Doe",
  "initialBalance": 1000.00
}
```

### Using cURL

```bash
# Create a bank account
curl -X POST https://localhost:5001/bank-accounts \
  -H "Content-Type: application/json" \
  -d '{
    "accountNumber": "ACC-001",
    "accountHolder": "John Doe",
    "initialBalance": 1000.00
  }'

# Response:
# {
#   "accountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
#   "accountNumber": "ACC-001"
# }
```

---

## ?? Key Concepts Demonstrated

### 1. Event Sourcing

All state changes are captured as events:

```
BankAccountCreatedEvent ? Balance: 1000
MoneyDepositedEvent     ? Balance: 1500
MoneyWithdrawnEvent     ? Balance: 1200
```

The current state is reconstructed by replaying events.

### 2. CQRS (Command Query Responsibility Segregation)

- **Commands**: Modify state (CreateBankAccount, DepositMoney, WithdrawMoney)
- **Queries**: Read state (GetBankAccount, GetTransactionHistory)

### 3. Aggregate Pattern

`BankAccountAggregate` encapsulates business logic and ensures consistency.

### 4. Outbox Pattern

Events are stored in the outbox table before publishing, ensuring reliable delivery.

### 5. Validation Pipeline

Commands are validated before execution using `Validator<T>`.

---

## ?? Troubleshooting

### Database Connection Issues

If you encounter connection errors:

1. Ensure SQL Server is running
2. Check the connection string in `appsettings.json`
3. Verify SQL Server is accepting connections

```bash
# Test connection with sqlcmd
sqlcmd -S "(localdb)\mssqllocaldb" -Q "SELECT @@VERSION"
```

### Migration Issues

If migrations fail:

```bash
# Drop and recreate database
dotnet ef database drop -f
dotnet run
```

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2025

---

## ?? Author

**Francescolis**
- GitHub: [@Francescolis](https://github.com/Francescolis)
- Company: Kamersoft

---

## ?? Support

For questions or issues:
- Open an issue on [GitHub](https://github.com/Francescolis/Xpandables.Net/issues)
- Check the [main documentation](../README.md)

---

<div align="center">

**Built with ?? using Xpandables.Net and .NET 10**

[? back to top](#-xpandablessampleapi)

</div>
