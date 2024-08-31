
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
using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Decorators;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Distribution;
using Xpandables.Net.Interceptions;
using Xpandables.Net.Operations;

namespace Xpandables.Net.UnitTests;

public sealed class InterceptorTests
{
    private const int ExpectedValue = 40;

    [Theory]
    [InlineData(10, ExpectedValue)]
    public async Task InterceptionClassicMethod(int value, int expected)
    {
        ICalculator calculator = InterceptorFactory
            .CreateProxy<ICalculator>(
            new CalculatorInterceptor(),
            new Calculator());

        int result = await calculator.CalculateAsync(value);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(10, ExpectedValue)]
    public async Task InterceptorDependencyMethod(int value, int expected)
    {
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddTransient<ICalculator, Calculator>()
            .AddXInterceptor<ICalculator, CalculatorInterceptor>()
            .BuildServiceProvider();

        ICalculator calculator = serviceProvider
            .GetRequiredService<ICalculator>();
        int result = await calculator.CalculateAsync(value);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(10, ExpectedValue)]
    public async Task InterceptorDependencyAttributeMethod(
        int value, int expected)
    {
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddTransient<ICalculator, Calculator>()
            .AddXInterceptorAttributes()
            .BuildServiceProvider();

        ICalculator calculator = serviceProvider
            .GetRequiredService<ICalculator>();
        int result = await calculator.CalculateAsync(value);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(10, ExpectedValue)]
    public async Task InterceptorDependencyHandlerMethod(
        int value, int expected)
    {
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddXRequestResponseHandlers(typeof(Calculator).Assembly)
            .AddXInterceptorHandlers<CalculatorInterceptor>(
            typeof(Calculator).Assembly)
            .BuildServiceProvider();

        IRequestHandler<Args, int> handler = serviceProvider
            .GetRequiredService<IRequestHandler<Args, int>>();
        IOperationResult<int> result = await handler.HandleAsync(new(value));

        Assert.Equal(expected, result.Result);
    }

    public sealed class CalculatorInterceptor : Interceptor
    {
        public override void Intercept(IInvocation invocation)
        {
            if (invocation.Arguments.First().Value is Args)
            {
                invocation.Arguments.First().ChangeValueTo(
                    new Args(ExpectedValue));
            }
            else
            {
                invocation.Arguments.First().ChangeValueTo(ExpectedValue);
            }

            invocation.Proceed();
        }
    }

    public sealed class CalculatorInterceptorAttribute : InterceptorAttribute
    {
        public override IInterceptor Create(
            IServiceProvider serviceProvider)
            => new CalculatorInterceptor();
    }

    [CalculatorInterceptor]
    public interface ICalculator
    {
        Task<int> CalculateAsync(int args);
    }

    public sealed class Calculator : ICalculator
    {
        public async Task<int> CalculateAsync(int args)
        {
            await Task.Yield();
            return args;
        }
    }

    public sealed record Args : IRequest<int>, IInterceptorDecorator
    {
        public Args(int value) => Value = value;

        [Range(5, 25)]
        public int Value { get; set; }
    }

    public sealed record Args1(int Value) : IRequest<int>, IInterceptorDecorator;
    public sealed record Args2(int Value) : IRequest<int>, IInterceptorDecorator;

    public sealed class HandleArgs : IRequestHandler<Args, int>
    {
        public async Task<IOperationResult<int>> HandleAsync(
            Args query, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return OperationResults.Ok(query.Value).Build();
        }
    }

    public sealed class HandleExceptionArgs : IRequestHandler<Args1, int>
    {
        public async Task<IOperationResult<int>> HandleAsync(
            Args1 query, CancellationToken cancellationToken = default)
        {
            await Task.Yield();

            return OperationResults.Ok(query.Value).Build();
        }
    }
}
