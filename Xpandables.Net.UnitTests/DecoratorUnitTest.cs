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

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Operations;
using Xpandables.Net.Operations.Messaging;
using Xpandables.Net.Validators;

namespace Xpandables.Net.UnitTests;

public sealed record QueryDecorated(Guid Id) : IQuery<string>, IValidateDecorator;
public sealed class QueryDecoratedHandler : IQueryHandler<QueryDecorated, string>
{
    public async ValueTask<OperationResult<string>> HandleAsync(
        QueryDecorated query,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return OperationResults
            .Ok("Product")
            .Build();
    }
}
public sealed class CustomValidationQueryDecorator<TQuery, TResult>(
    IQueryHandler<TQuery, TResult> handler) : IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>, IValidateDecorator
{
    private readonly IQueryHandler<TQuery, TResult> _handler = handler
        ?? throw new ArgumentNullException(nameof(handler));

    public ValueTask<OperationResult<TResult>> HandleAsync(
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
        var handler = _serviceProvider
            .GetService<IQueryHandler<QueryDecorated, string>>();

        handler.Should().NotBeNull();
        handler.Should().BeOfType<CustomValidationQueryDecorator<QueryDecorated, string>>();
    }

}