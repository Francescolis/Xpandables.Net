
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

using Xpandables.Net.Decorators;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Operations;

namespace Xpandables.Net.UnitTests;

public sealed record QueryDecorated(Guid Id) : IRequest<string>, IValidateDecorator;
public sealed class QueryDecoratedHandlerA : IRequestHandler<QueryDecorated, string>
{
    public async Task<IOperationResult<string>> HandleAsync(
        QueryDecorated query,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return OperationResults
            .Ok("Product A")
            .Build();
    }
}

public sealed class QueryDecoratedHandlerB : IRequestHandler<QueryDecorated, string>
{
    public async Task<IOperationResult<string>> HandleAsync(
        QueryDecorated query,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return OperationResults
            .Ok("Product B")
            .Build();
    }
}

public sealed class QueryDecoratedHandlerC :
    IRequestHandler<QueryDecorated, string>, IValidateDecorator
{
    public async Task<IOperationResult<string>> HandleAsync(
        QueryDecorated query,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return OperationResults
            .Ok("Product C")
            .Build();
    }
}

public sealed class CustomValidationQueryDecorator<TQuery, TResult>(
    IRequestHandler<TQuery, TResult> handler) :
    IRequestHandler<TQuery, TResult>, IDecorator
    where TQuery : IRequest<TResult>, IValidateDecorator
{
    private readonly IRequestHandler<TQuery, TResult> _handler = handler
        ?? throw new ArgumentNullException(nameof(handler));

    public Task<IOperationResult<TResult>> HandleAsync(
        TQuery query,
        CancellationToken cancellationToken = default)
        => _handler.HandleAsync(query, cancellationToken);
}

public sealed class DecoratorUnitTest
{
    private readonly IServiceProvider _serviceProvider;
    public DecoratorUnitTest() =>
        _serviceProvider = new ServiceCollection()
            .AddXRequestResponseHandlers()
            .AddXValidatorGenerics()
            .AddXValidators()
            .XTryDecorate(
                typeof(IRequestHandler<,>),
                typeof(CustomValidationQueryDecorator<,>),
                typeof(IValidateDecorator))
            .BuildServiceProvider();

    [Fact]
    public void DecoratorRegistration_Should_Return_DecoratorHandler()
    {
        IRequestHandler<QueryDecorated, string>? handler = _serviceProvider
            .GetService<IRequestHandler<QueryDecorated, string>>();

        handler.Should().NotBeNull();
        handler.Should().BeOfType<CustomValidationQueryDecorator
            <QueryDecorated, string>>();
    }

    [Fact]
    public void Decorator_Should_Match_Number_Registered()
    {
        IEnumerable<IRequestHandler<QueryDecorated, string>> handlers =
            _serviceProvider
            .GetServices<IRequestHandler<QueryDecorated, string>>();

        handlers.Should().Contain(
            handler => handler.GetType() ==
            typeof(CustomValidationQueryDecorator<QueryDecorated, string>));
        handlers.Should().HaveCount(3);
    }

}
