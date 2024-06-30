
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

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Commands;
using Xpandables.Net.Decorators;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;
using Xpandables.Net.Validators;

namespace Xpandables.Net.UnitTests;

public sealed class ValidatorUnitTest
{
    private const string UserName = "MyName";
    private const string Password = "MyPassword";

    private readonly IServiceProvider _serviceProvider;
    public ValidatorUnitTest() =>
        _serviceProvider = new ServiceCollection()
            .AddXValidatorGenerics()
            .AddXValidators()
            .BuildServiceProvider();

    [Theory]
    [InlineData("MyName", "password")]
    public void Validator_Throws_OperationResultException(
        string userName, string password)
    {
        ICompositeValidator<Login> validators = new CompositeValidator<Login>(
            [new ValidatorThrowsValidationException()]);

        ICommandHandler<Login> commandHandler = new HandleLogin();
        ValidatorCommandDecorator<Login> validatorDecorator =
            new(commandHandler, validators);
        Login login = new(userName, password);
        Func<Task<IOperationResult>> result = async ()
            => await validatorDecorator.HandleAsync(login);

        result.Should().ThrowExactlyAsync<ValidationException>();
    }

    [Theory]
    [InlineData(UserName, Password)]
    public async Task Validator_Returns_OperationResult(
        string userName, string password)
    {
        ICompositeValidator<Login> validators = new CompositeValidator<Login>(
            [new ValidatorReturnsOperationResult()]);

        ICommandHandler<Login> commandHandler = new HandleLogin();
        ValidatorCommandDecorator<Login> validatorDecorator =
            new(commandHandler, validators);
        Login login = new(userName, password);
        IOperationResult result = await validatorDecorator.HandleAsync(login);

        result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public void ValidatorRegistration_Should_Return_Validator()
    {
        ICompositeValidator<Login>? validators = _serviceProvider
            .GetService<ICompositeValidator<Login>>();

        validators.Should().NotBeNull();
    }

    public readonly record struct Login(string UserLogin, string UserPassword)
        : ICommand, IValidateDecorator;

    public sealed class HandleLogin : ICommandHandler<Login>
    {
        public Task<IOperationResult> HandleAsync(
            Login command,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                OperationResults
                .Ok()
                .WithHeader(nameof(Login.UserLogin), command.UserLogin)
                .Build());
    }

    public sealed class ValidatorReturnsOperationResult : IValidator<Login>
    {
        public IOperationResult Validate(Login argument)
        {
            bool areEqual = argument.UserLogin.Equals(UserName)
                && argument.UserPassword.Equals(Password);

            return areEqual switch
            {
                true => OperationResults.Ok().Build(),
                _ => OperationResults
                    .BadRequest()
                    .WithError(nameof(argument.UserLogin), "Failed")
                    .Build()
            };
        }
    }

    public sealed class ValidatorThrowsValidationException : IValidator<Login>
    {
        public IOperationResult Validate(Login argument)
        {
            bool areEqual = argument.UserLogin.Equals(UserName)
                && argument.UserPassword.Equals(Password);

            return areEqual switch
            {
                true => OperationResults.Ok().Build(),
                _ => throw new ValidationException(
                    new ValidationResult("Failed", [nameof(argument.UserLogin)]),
                    default,
                    default)
            };
        }
    }
}
