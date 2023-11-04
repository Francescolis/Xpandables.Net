﻿/************************************************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
************************************************************************************************************/
using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Aggregates;
using Xpandables.Net.Aggregates.DomainEvents;
using Xpandables.Net.Aggregates.IntegrationEvents;
using Xpandables.Net.Collections;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Operations;
using Xpandables.Net.Operations.Messaging;

namespace Xpandables.Net.UnitTests;

public readonly record struct ProductId(Guid Value) : IAggregateId<ProductId>
{
    public static ProductId CreateInstance(Guid value) => throw new NotImplementedException();
    public static ProductId DefaultInstance() => throw new NotImplementedException();
    public static implicit operator Guid(ProductId self) => throw new NotImplementedException();
    public static implicit operator string(ProductId self) => throw new NotImplementedException();
}
public readonly record struct AddProductCommand(Guid ProductId, int Quantity) : ICommand;
public readonly record struct GetProductQuery(Guid ProductId) : IQuery<string>;
public readonly record struct GetProductAsyncQuery(Guid ProductId) : IAsyncQuery<string>;
public sealed record class ProductAddedEvent(Guid ProductId, int Qty) : DomainEvent<ProductId>;
public sealed record class ProductAddedIntegrationEvent : IntegrationEvent;
public sealed class AddProductCommandHandler : ICommandHandler<AddProductCommand>
{
    public async ValueTask<OperationResult> HandleAsync(
        AddProductCommand command,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return OperationResults
            .Ok()
            .Build();
    }
}

public sealed class GetProductQueryHandler : IQueryHandler<GetProductQuery, string>
{
    public async ValueTask<OperationResult<string>> HandleAsync(
        GetProductQuery query,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return OperationResults
            .Ok("Product")
            .Build();
    }
}

public sealed class GetProductAsyncQueryHandler : IAsyncQueryHandler<GetProductAsyncQuery, string>
{
    public IAsyncEnumerable<string> HandleAsync(
        GetProductAsyncQuery query,
        CancellationToken cancellationToken = default) => Enumerable.Empty<string>()
            .ToAsyncEnumerable();
}

public sealed class ProductAddedEventHandler : IDomainEventHandler<ProductAddedEvent, ProductId>
{
    public ValueTask<OperationResult> HandleAsync(
        ProductAddedEvent @event,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

public sealed class ProductAddedIntegrationEventHandler
    : IIntegrationEventHandler<ProductAddedIntegrationEvent>
{
    public ValueTask<OperationResult> HandleAsync(
        ProductAddedIntegrationEvent @event,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
public sealed class MessagingRegistrationUnitTest
{
    private readonly IServiceProvider _serviceProvider;
    public MessagingRegistrationUnitTest()
    {
        _serviceProvider = new ServiceCollection()
            .AddXCommandHandlers()
            .AddXQueryHandlers()
            .AddXAsyncQueryHandlers()
            .AddXDomainEventHandlers()
            .AddXIntegrationEventHandlers()
            .BuildServiceProvider();
    }

    [Fact]
    public void MessagingRegistration_Should_Return_CommandHandler()
    {
        var handler = _serviceProvider
            .GetService<ICommandHandler<AddProductCommand>>();

        handler.Should().NotBeNull();
        handler.Should().BeOfType<AddProductCommandHandler>();
    }

    [Fact]
    public void MessagingRegistration_Should_Return_QueryHandler()
    {
        var handler = _serviceProvider
            .GetService<IQueryHandler<GetProductQuery, string>>();

        handler.Should().NotBeNull();
        handler.Should().BeOfType<GetProductQueryHandler>();
    }

    [Fact]
    public void MessagingRegistration_Should_Return_AsyncQueryHandler()
    {
        var handler = _serviceProvider
            .GetService<IAsyncQueryHandler<GetProductAsyncQuery, string>>();

        handler.Should().NotBeNull();
        handler.Should().BeOfType<GetProductAsyncQueryHandler>();
    }

    [Fact]
    public void MessagingRegistration_Should_Return_DomainEventHandler()
    {
        var handler = _serviceProvider
            .GetService<IDomainEventHandler<ProductAddedEvent, ProductId>>();

        handler.Should().NotBeNull();
        handler.Should().BeOfType<ProductAddedEventHandler>();
    }

    [Fact]
    public void MessagingRegistration_Should_Return_IntegrationEventHandler()
    {
        var handler = _serviceProvider
            .GetService<IIntegrationEventHandler<ProductAddedIntegrationEvent>>();

        handler.Should().NotBeNull();
        handler.Should().BeOfType<ProductAddedIntegrationEventHandler>();
    }
}