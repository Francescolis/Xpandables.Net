
/*******************************************************************************
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
********************************************************************************/
using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Distribution;
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives.Collections;

namespace Xpandables.Net.UnitTests;

public readonly record struct AddProductCommand(
    Guid ProductId, int Quantity) : IRequest;
public readonly record struct GetProductQuery(
    Guid ProductId) : IRequest<string>;
public readonly record struct GetProductAsyncQuery(
    Guid ProductId) : IAsyncRequest<string>;
public sealed record class ProductAddedEvent(
    Guid ProductId, int Qty) : EventDomain;
public sealed record class ProductAddedIntegrationEvent : EventIntegration;
public sealed class AddProductCommandHandler :
    IRequestHandler<AddProductCommand>
{
    public async Task<IOperationResult> HandleAsync(
        AddProductCommand command,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return OperationResults
            .Ok()
            .Build();
    }
}

public sealed class GetProductQueryHandler :
    IRequestHandler<GetProductQuery, string>
{
    public async Task<IOperationResult<string>> HandleAsync(
        GetProductQuery query,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return OperationResults
            .Ok("Product")
            .Build();
    }
}

public sealed class GetProductAsyncQueryHandler :
    IAsyncRequestHandler<GetProductAsyncQuery, string>
{
    public IAsyncEnumerable<string> HandleAsync(
        GetProductAsyncQuery query,
        CancellationToken cancellationToken = default) => Enumerable.Empty<string>()
            .ToAsyncEnumerable();
}

public sealed class ProductAddedEventHandler : IEventHandler<ProductAddedEvent>
{
    public Task<IOperationResult> HandleAsync(
        ProductAddedEvent @event,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}

public sealed class ProductAddedIntegrationEventHandler :
    IEventHandler<ProductAddedIntegrationEvent>
{
    public Task<IOperationResult> HandleAsync(
        ProductAddedIntegrationEvent @event,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
public sealed class MessagingRegistrationUnitTest
{
    private readonly IServiceProvider _serviceProvider;
    public MessagingRegistrationUnitTest() =>
        _serviceProvider = new ServiceCollection()
            .AddXRequestHandlers()
            .AddXRequestResponseHandlers()
            .AddXAsyncRequestResponseHandlers()
            .AddXEventHandlers()
            .BuildServiceProvider();

    [Fact]
    public void MessagingRegistration_Should_Return_CommandHandler()
    {
        IRequestHandler<AddProductCommand>? handler = _serviceProvider
            .GetService<IRequestHandler<AddProductCommand>>();

        handler.Should().NotBeNull();
        handler.Should().BeOfType<AddProductCommandHandler>();
    }

    [Fact]
    public void MessagingRegistration_Should_Return_QueryHandler()
    {
        IRequestHandler<GetProductQuery, string>? handler = _serviceProvider
            .GetService<IRequestHandler<GetProductQuery, string>>();

        handler.Should().NotBeNull();
        handler.Should().BeOfType<GetProductQueryHandler>();
    }

    [Fact]
    public void MessagingRegistration_Should_Return_AsyncQueryHandler()
    {
        IAsyncRequestHandler<GetProductAsyncQuery, string>? handler =
            _serviceProvider
            .GetService<IAsyncRequestHandler<GetProductAsyncQuery, string>>();

        handler.Should().NotBeNull();
        handler.Should().BeOfType<GetProductAsyncQueryHandler>();
    }

    [Fact]
    public void MessagingRegistration_Should_Return_DomainEventHandler()
    {
        IEventHandler<ProductAddedEvent>? handler =
            _serviceProvider
            .GetService<IEventHandler<ProductAddedEvent>>();

        handler.Should().NotBeNull();
        handler.Should().BeOfType<ProductAddedEventHandler>();
    }

    [Fact]
    public void MessagingRegistration_Should_Return_IntegrationEventHandler()
    {
        IEventHandler<ProductAddedIntegrationEvent>? handler =
            _serviceProvider
            .GetService<IEventHandler<ProductAddedIntegrationEvent>>();

        handler.Should().NotBeNull();
        handler.Should().BeOfType<ProductAddedIntegrationEventHandler>();
    }
}
