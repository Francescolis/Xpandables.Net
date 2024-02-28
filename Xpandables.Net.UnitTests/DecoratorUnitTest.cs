/************************************************************************************************************
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

using Xpandables.Net.Commands;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Operations;
using Xpandables.Net.Validators;

namespace Xpandables.Net.UnitTests;

public sealed record QueryDecorated(Guid Id) : IQuery<string>, IValidateDecorator;
public sealed class QueryDecoratedHandlerA : IQueryHandler<QueryDecorated, string>
{
    public async ValueTask<IOperationResult<string>> HandleAsync(
        QueryDecorated query,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return OperationResults
            .Ok("Product A")
            .Build();
    }
}

public sealed class QueryDecoratedHandlerB : IQueryHandler<QueryDecorated, string>
{
    public async ValueTask<IOperationResult<string>> HandleAsync(
        QueryDecorated query,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return OperationResults
            .Ok("Product B")
            .Build();
    }
}

public sealed class QueryDecoratedHandlerC : IQueryHandler<QueryDecorated, string>
{
    public async ValueTask<IOperationResult<string>> HandleAsync(
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
    IQueryHandler<TQuery, TResult> handler) : IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>, IValidateDecorator
{
    private readonly IQueryHandler<TQuery, TResult> _handler = handler
        ?? throw new ArgumentNullException(nameof(handler));

    public ValueTask<IOperationResult<TResult>> HandleAsync(
        TQuery query,
        CancellationToken cancellationToken = default)
    {
        return _handler.HandleAsync(query, cancellationToken);
    }
}

public sealed class DecoratorUnitTest
{
    private readonly IServiceProvider _serviceProvider;
    public DecoratorUnitTest()
    {
        _serviceProvider = new ServiceCollection()
            .AddXQueryHandlers()
            .AddXValidatorGenerics()
            .AddXValidators()
            .XTryDecorate(typeof(IQueryHandler<,>), typeof(CustomValidationQueryDecorator<,>))
            .BuildServiceProvider();
    }

    [Fact]
    public void DecoratorRegistration_Should_Return_DecoratorHandler()
    {
        IQueryHandler<QueryDecorated, string>? handler = _serviceProvider
            .GetService<IQueryHandler<QueryDecorated, string>>();

        handler.Should().NotBeNull();
        handler.Should().BeOfType<CustomValidationQueryDecorator<QueryDecorated, string>>();
    }

    [Fact]
    public void Decorator_Should_Match_Number_Registered()
    {
        IEnumerable<IQueryHandler<QueryDecorated, string>> handlers = _serviceProvider
            .GetServices<IQueryHandler<QueryDecorated, string>>();

        handlers.Should().Contain(
            handler => handler.GetType() == typeof(CustomValidationQueryDecorator<QueryDecorated, string>));
        handlers.Should().HaveCount(3);
    }

}
