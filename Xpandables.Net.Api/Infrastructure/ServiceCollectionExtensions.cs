/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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

using Microsoft.EntityFrameworkCore;
using Xpandables.Net.Api.Features.BankAccounts.Domain;
using Xpandables.Net.Api.Infrastructure.Data;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Events;

namespace Xpandables.Net.Api.Infrastructure;

/// <summary>
/// Extension methods for service registration demonstrating all Xpandables.Net features.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Xpandables.Net services for the demo application.
    /// This demonstrates the complete setup required for a production application.
    /// </summary>
    public static IServiceCollection AddXpandablesNetDemo(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ========== 1. MEDIATOR PATTERN ==========
        // Registers the mediator for CQRS command/query handling
        services.AddXMediator();

        // ========== 2. PIPELINE DECORATORS ==========
        // These execute in order: Pre ? Validation ? Exception ? UnitOfWork ? Handler ? Post
        
        // Base pipeline handler - REQUIRED
        services.AddXPipelineRequestHandler();
        
        // Pre-processing decorator - runs before main handler
        // Use for: logging, authentication checks, tenant resolution
        services.AddXPipelinePreDecorator();
        
        // Post-processing decorator - runs after main handler
        // Use for: response transformation, caching, notifications
        services.AddXPipelinePostDecorator();
        
        // Exception handling decorator - catches and handles exceptions
        // Converts exceptions to ExecutionResult with proper status codes
        services.AddXPipelineExceptionDecorator();
        
        // Validation decorator - validates requests using registered validators
        // Returns validation errors before handler execution
        services.AddXPipelineValidationDecorator();
        
        // UnitOfWork decorator - manages database transactions
        // Commits on success, rolls back on failure
        services.AddXPipelineUnitOfWorkDecorator();

        // ========== 3. VALIDATION ==========
        // Scans assembly for IValidator<T> implementations
        services.AddXValidators(typeof(ServiceCollectionExtensions).Assembly);

        // ========== 4. DATABASE CONTEXT ==========
        // Registers DbContext for event sourcing
        services.AddDbContext<BankingDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("BankingDb") ?? "Data Source=banking.db",
                sqliteOptions =>
                {
                    sqliteOptions.MigrationsAssembly(typeof(BankingDbContext).Assembly.FullName);
                    sqliteOptions.CommandTimeout(30);
                }));

        // ========== 5. EVENT SOURCING ==========
        // Registers event store using Entity Framework
        // Provides: IEventStore, event persistence, event replay
        services.AddXEventStore<BankingDbContext>();
        
        // Registers aggregate store for specific aggregate type
        // Handles: loading aggregates, saving events, replay
        services.AddScoped<IAggregateStore<BankAccountAggregate>, 
            AggregateStore<BankAccountAggregate>>();

        // ========== 6. UNIT OF WORK ==========
        // Registers UnitOfWork for Entity Framework
        // Used by UnitOfWorkDecorator to manage transactions
        services.AddXEntityFrameworkUnitOfWork<BankingDbContext>();

        // ========== 7. DOMAIN EVENTS ==========
        // Registers buffer for domain events
        // Collects events during request processing for later dispatch
        services.AddXPendingDomainEventsBuffer();

        // ========== 8. EVENT SCHEDULER (Optional) ==========
        // Background service for processing integration events from outbox
        services.AddXScheduler(options =>
        {
            // Enable/disable the scheduler
            options.IsEventSchedulerEnabled = true;
            
            // How often to check for events (milliseconds)
            options.SchedulerFrequency = 5000; // 5 seconds
            
            // Number of events to process per batch
            options.BatchSize = 10;
            
            // Circuit breaker settings
            options.CircuitBreakerFailureThreshold = 5;
            options.CircuitBreakerTimeout = TimeSpan.FromMinutes(1);
            
            // Retry settings
            options.BackoffBaseDelayMs = 1000;
            options.BackoffMaxDelayMs = 60000;
            
            // Processing timeout per event
            options.EventProcessingTimeout = 30000; // 30 seconds
            
            // Concurrent processors
            options.MaxConcurrentProcessors = Environment.ProcessorCount;
        });

        // ========== 9. OUTBOX PATTERN (Optional) ==========
        // Registers outbox store for reliable event publishing
        // Ensures events are not lost even if external systems are down
        services.AddXOutboxStore<BankingDbContext>();

        // ========== 10. SNAPSHOT SUPPORT (Optional) ==========
        // Uncomment to enable snapshots for performance
        // Snapshots reduce the number of events to replay for large aggregates
        /*
        services.AddXSnapshotStore(options =>
        {
            options.IsSnapshotEnabled = true;
            options.SnapshotFrequency = 100; // Create snapshot every 100 events
        });
        */

        // ========== 11. COMMAND/QUERY HANDLERS ==========
        // Auto-register all IRequestHandler<> implementations
        services.Scan(scan => scan
            .FromAssemblyOf<ServiceCollectionExtensions>()
            .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // Auto-register all IRequestHandler<,> implementations (with response)
        services.Scan(scan => scan
            .FromAssemblyOf<ServiceCollectionExtensions>()
            .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // Auto-register all IStreamRequestHandler<,> implementations
        services.Scan(scan => scan
            .FromAssemblyOf<ServiceCollectionExtensions>()
            .AddClasses(classes => classes.AssignableTo(typeof(IStreamRequestHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // ========== 12. ENDPOINTS ==========
        // Auto-register all IEndpointRoute implementations
        services.AddXEndpointRoutes(typeof(ServiceCollectionExtensions).Assembly);

        // ========== 13. ADDITIONAL FEATURES (Optional) ==========
        
        // Publisher/Subscriber for in-memory events
        // services.AddXPublisher();
        // services.AddXSubscriber();
        
        // Repository pattern (if needed for read models)
        // services.AddXRepository<TEntity, TRepository>();
        
        // Specification pattern for complex queries
        // services.AddXSpecification();

        return services;
    }

    /// <summary>
    /// Configures the application middleware pipeline.
    /// </summary>
    public static WebApplication UseXpandablesNetDemo(this WebApplication app)
    {
        // ========== 1. ENSURE DATABASE CREATED ==========
        // Create database and apply migrations
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
            dbContext.Database.EnsureCreated();
            
            // For production, use migrations instead:
            // dbContext.Database.Migrate();
        }

        // ========== 2. DEVELOPMENT MIDDLEWARE ==========
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Bank Account API v1");
                options.RoutePrefix = "swagger";
            });
        }

        // ========== 3. STANDARD MIDDLEWARE ==========
        app.UseHttpsRedirection();
        app.UseCors();

        // ========== 4. XPANDABLES.NET ENDPOINTS ==========
        // Maps all registered IEndpointRoute implementations
        app.MapXEndpointRoutes();

        return app;
    }
}
