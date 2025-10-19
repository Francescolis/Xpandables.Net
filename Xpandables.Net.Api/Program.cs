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
using Xpandables.Net.AspNetCore;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Events;
using Xpandables.Net.Events.EntityFramework;
using Xpandables.Net.Repositories.EntityFramework;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:5001", "http://localhost:5000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Configure SQLite database for event sourcing
builder.Services.AddDbContext<BankingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("BankingDb") 
        ?? "Data Source=banking.db"));

// Register Xpandables.Net services
builder.Services.AddXMediator();
builder.Services.AddXPipelineRequestHandler();
builder.Services.AddXPipelinePreDecorator();
builder.Services.AddXPipelinePostDecorator();
builder.Services.AddXPipelineExceptionDecorator();
builder.Services.AddXPipelineValidationDecorator();
builder.Services.AddXPipelineUnitOfWorkDecorator();

// Register validators from assembly
builder.Services.AddXValidators(typeof(Program).Assembly);

// Register event store and aggregate store
builder.Services.AddXEventStore<BankingDbContext>();
builder.Services.AddScoped<IAggregateStore<BankAccountAggregate>, AggregateStore<BankAccountAggregate>>();

// Register unit of work for Entity Framework
builder.Services.AddXEntityFrameworkUnitOfWork<BankingDbContext>();

// Register pending domain events buffer
builder.Services.AddXPendingDomainEventsBuffer();

// Register event scheduler (for integration events)
builder.Services.AddXScheduler(options =>
{
    options.IsEventSchedulerEnabled = true;
    options.SchedulerFrequency = 5000; // 5 seconds
    options.BatchSize = 10;
});

// Register outbox store
builder.Services.AddXOutboxStore<BankingDbContext>();

// Scan and register command/query handlers
builder.Services.Scan(scan => scan
    .FromAssemblyOf<Program>()
    .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<>)))
    .AsImplementedInterfaces()
    .WithScopedLifetime());

builder.Services.Scan(scan => scan
    .FromAssemblyOf<Program>()
    .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)))
    .AsImplementedInterfaces()
    .WithScopedLifetime());

builder.Services.Scan(scan => scan
    .FromAssemblyOf<Program>()
    .AddClasses(classes => classes.AssignableTo(typeof(IStreamRequestHandler<,>)))
    .AsImplementedInterfaces()
    .WithScopedLifetime());

// Register endpoints
builder.Services.AddXEndpointRoutes(typeof(Program).Assembly);

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

// Map endpoints
app.MapXEndpointRoutes();

await app.RunAsync();
