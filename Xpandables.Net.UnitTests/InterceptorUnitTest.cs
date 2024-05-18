
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

using Xpandables.Net.Aspects;
using Xpandables.Net.Commands;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Interceptions;
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;

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
    [InlineData(10)]
    public async Task AspectValidatorDependencyAttributeMethod(int value)
    {
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddXQueryHandlers(typeof(Calculator).Assembly)
            .AddScoped(typeof(OnAspectValidator<>))
            .AddScoped(typeof(IAspectValidator<>), typeof(AspectValidator<>))
            .AddXAspectBehaviors()
            .BuildServiceProvider();

        IQueryHandler<Args, int> handler = serviceProvider
                 .GetRequiredService<IQueryHandler<Args, int>>();
        IOperationResult result = await handler.HandleAsync(new(value));

        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData(10, ExpectedValue)]
    public async Task AspectVisitorDependencyAttributeMethod(
        int value, int expected)
    {
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddXQueryHandlers(typeof(Calculator).Assembly)
            .AddXOnAspects(typeof(AspectValidator<>).Assembly)
            .AddScoped(typeof(IAspectVisitor<Args>), typeof(AspectVisitor))
            .AddXAspectBehaviors()
            .BuildServiceProvider();

        IQueryHandler<Args, int> handler = serviceProvider
                 .GetRequiredService<IQueryHandler<Args, int>>();
        IOperationResult<int> result = await handler.HandleAsync(new(value));

        Assert.Equal(expected, result.Result);
    }

    [Theory]
    [InlineData(10, ExpectedValue)]
    public async Task InterceptorDependencyHandlerMethod(
        int value, int expected)
    {
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddXQueryHandlers(typeof(Calculator).Assembly)
            .AddXInterceptorHandlers<CalculatorInterceptor>(
            typeof(Calculator).Assembly)
            .BuildServiceProvider();

        IQueryHandler<Args, int> handler = serviceProvider
            .GetRequiredService<IQueryHandler<Args, int>>();
        IOperationResult<int> result = await handler.HandleAsync(new(value));

        Assert.Equal(expected, result.Result);
    }

    public sealed class CalculatorInterceptor : Interceptor
    {
        public override void Intercept(IInvocation invocation)
        {
            if (invocation.Arguments.First().Value is Args)
                invocation.Arguments.First().ChangeValueTo(
                    new Args(ExpectedValue));
            else
                invocation.Arguments.First().ChangeValueTo(ExpectedValue);

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
        ValueTask<int> CalculateAsync(int args);
    }

    public class Calculator : ICalculator
    {
        ValueTask<int> ICalculator.CalculateAsync(int args) => new(args);
    }

    public sealed record Args : IQuery<int>, IInterceptorDecorator, IAspectVisitable
    {
        public Args(int value)
        {
            Value = value;
        }

        [Range(5, 25)]
        public int Value { get; set; }
    }

    [AspectValidator<IQueryHandler<Args, int>>(ThrowException = false)]
    [AspectVisitor<IQueryHandler<Args, int>>(Order = 1)]
    public sealed class HandleArgs : IQueryHandler<Args, int>
    {
        public ValueTask<IOperationResult<int>> HandleAsync(
            Args query, CancellationToken cancellationToken = default)
        {
            return new ValueTask<IOperationResult<int>>(
                OperationResults.Ok(query.Value).Build());
        }
    }

    public sealed class AspectVisitor : IAspectVisitor<Args>
    {
        public void Visit(Args element)
        {
            element.Value = ExpectedValue;// element with { Value = ExpectedValue };
        }
    }
}
