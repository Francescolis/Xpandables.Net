# Build Fixes Required for Xpandables.Net Demo

## Summary of Issues

The demo application has several compilation issues due to incorrect API usage. Here are the fixes needed:

## 1. Database Context Fix

**Current (WRONG):**
```csharp
public sealed class BankingDbContext : EventStoreContext
{
    public BankingDbContext(DbContextOptions<BankingDbContext> options)
        : base(options)
    {
    }
}
```

**Correct:**
```csharp
public sealed class BankingDbContext : DataContext
{
    public BankingDbContext(DbContextOptions<BankingDbContext> options)
        : base(options)
    {
    }
}
```

## 2. Service Registration Fixes

**Current (WRONG):**
```csharp
// Wrong: Using AddXEventStoreDataContext with generic parameter
builder.Services.AddXEventStoreDataContext<BankingDbContext>(contextBuilder =>
    contextBuilder.UseSqlite(...));

// Wrong: AddXPipelineUnitOfWorkDecorator doesn't exist
builder.Services.AddXPipelineUnitOfWorkDecorator();

// Wrong: AddXEntityFrameworkUnitOfWork doesn't exist  
builder.Services.AddXEntityFrameworkUnitOfWork<BankingDbContext>();

// Wrong: AddXOutboxStore with DbContext
builder.Services.AddXOutboxStore<BankingDbContext>();
```

**Correct:**
```csharp
// Use AddXEventStoreDataContext WITHOUT generic (uses EventStoreDataContext)
builder.Services.AddXEventStoreDataContext(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("BankingDb") 
        ?? "Data Source=banking.db"));

// Use AddXEventUnitOfWork (not pipeline-specific)
builder.Services.AddXEventUnitOfWork();

// Use AddXOutboxStoreDataContext for context registration
builder.Services.AddXOutboxStoreDataContext(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("BankingDb") 
        ?? "Data Source=banking.db"));

// Then register outbox store implementation
builder.Services.AddXOutboxStore();
```

## 3. Validation Fixes

The Xpandables.Net validator doesn't work like FluentValidation. You need to implement `IValidator<T>` directly.

**Current (WRONG):**
```csharp
public sealed class CreateBankAccountValidator : Validator<CreateBankAccountCommand>
{
    protected override void BuildRules()
    {
        RuleFor(x => x.Owner).NotEmpty();
    }
}
```

**Correct - Option 1 (Data Annotations):**
```csharp
public sealed record CreateBankAccountCommand : IRequest<BankAccountResponse>
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public required string Owner { get; init; }
    
    [Required]
    [EmailAddress]
    public required string Email { get; init; }
}
```

**Correct - Option 2 (Custom IValidator):**
```csharp
public sealed class CreateBankAccountValidator : IValidator<CreateBankAccountCommand>
{
    public async Task<ValidationResult> ValidateAsync(
        CreateBankAccountCommand instance,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationResult>();
        
        if (string.IsNullOrWhiteSpace(instance.Owner))
            errors.Add(new ValidationResult("Owner is required", [nameof(instance.Owner)]));
            
        if (string.IsNullOrWhiteSpace(instance.Email))
            errors.Add(new ValidationResult("Email is required", [nameof(instance.Email)]));
            
        return errors.Count == 0 
            ? ValidationResult.Success! 
            : errors.First();
    }
}
```

## 4. ExecutionResultExtensions Ambiguity Fix

Add `using ExecutionResultExtensions = Xpandables.Net.ExecutionResults.ExecutionResultExtensions;` at the top of files.

## 5. REST Client Registration Fix

**Current (WRONG):**
```csharp
builder.Services.AddXRestClient<IBankAccountClient>();
```

**Correct:**
```csharp
builder.Services.AddXRestClient<IBankAccountClient>((sp, httpClient) =>
{
    httpClient.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7001");
});
```

## 6. Endpoint Route Fix

**Current (WRONG):**
```csharp
app.MapXEndpointRoutes();
```

**Correct:**
The extension method uses `AddXEndpointRoutes()` for registration during service configuration, but mapping happens differently. Check if there's a `UseXEndpointRoutes()` or similar.

## 7. DateTimeOffset vs DateTime Fix

In `TransactionResponse`:
```csharp
public required DateTimeOffset OccurredOn { get; init; } // Not DateTime
```

##8. Blazor Required Usings

Add to CreateAccount.razor:
```csharp
@using System.ComponentModel.DataAnnotations
```

## Recommendation

Given the complexity and number of issues, I recommend:

1. **Simplify the demo** - Remove validation decorators, outbox, scheduler temporarily
2. **Focus on core features**: Event sourcing, CQRS, basic mediator
3. **Get it compiling first** with minimal features
4. **Add features incrementally** once core works

Would you like me to create a simplified, working version that focuses on:
- Event sourcing with BankAccountAggregate
- Basic CQRS commands/queries  
- Minimal API endpoints
- Simple Blazor client

This will be much easier to get working and can be extended later.
