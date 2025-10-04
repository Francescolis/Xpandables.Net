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
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.ExecutionResults;

using FluentAssertions;

namespace Xpandables.Net.UnitTests.ExecutionResults;

/// <summary>
/// Tests for ExecutionResult delegate extension methods.
/// </summary>
public class ExecutionResultDelegateExtensionsTests
{
    [Fact]
    public void ToExecutionResult_Action_WhenSuccessful_ShouldReturnSuccessResult()
    {
        // Arrange
        var executed = false;
        Action action = () => executed = true;

        // Act
        var result = action.ToExecutionResult();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.IsSuccess.Should().BeTrue();
        executed.Should().BeTrue();
    }

    [Fact]
    public void ToExecutionResult_Action_WhenThrows_ShouldReturnFailureResult()
    {
        // Arrange
        Action action = () => throw new InvalidOperationException("Test error");

        // Act
        var result = action.ToExecutionResult();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.IsSuccess.Should().BeFalse();
        result.Errors.ContainsKey("Exception").Should().BeTrue();
    }

    [Fact]
    public void ToExecutionResult_Action_WhenThrowsExecutionResultException_ShouldNotWrap()
    {
        // Arrange
        var originalResult = ExecutionResultExtensions.BadRequest().Build();
        var executionException = new ExecutionResultException(originalResult);
        Action action = () => throw executionException;

        // Act & Assert
        action.Invoking(a => a.ToExecutionResult())
            .Should().Throw<ExecutionResultException>();
    }

    [Fact]
    public void ToExecutionResult_ActionWithParameter_WhenSuccessful_ShouldReturnSuccessResult()
    {
        // Arrange
        var receivedValue = string.Empty;
        const string testValue = "test input";
        Action<string> action = value => receivedValue = value;

        // Act
        var result = action.ToExecutionResult(testValue);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.IsSuccess.Should().BeTrue();
        receivedValue.Should().Be(testValue);
    }

    [Fact]
    public void ToExecutionResult_ActionWithParameter_WhenThrows_ShouldReturnFailureResult()
    {
        // Arrange
        Action<string> action = _ => throw new ArgumentException("Invalid argument");

        // Act
        var result = action.ToExecutionResult("test");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ToExecutionResult_ActionWithParameter_WhenThrowsValidationException_ShouldHandleSpecially()
    {
        // Arrange
        var validationException = new ValidationException("Validation error");
        Action<string> action = _ => throw validationException;

        // Act
        var result = action.ToExecutionResult("test");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ToExecutionResult_ActionWithParameter_WhenThrowsExecutionResultException_ShouldReturnOriginalResult()
    {
        // Arrange
        var originalResult = ExecutionResultExtensions.NotFound().Build();
        var executionException = new ExecutionResultException(originalResult);
        Action<string> action = _ => throw executionException;

        // Act
        var result = action.ToExecutionResult("test");

        // Assert
        result.Should().Be(originalResult);
    }

    [Fact]
    public async Task ToExecutionResultAsync_Task_WhenSuccessful_ShouldReturnSuccessResult()
    {
        // Arrange
        var executed = false;
        Task task = Task.Run(() => executed = true);

        // Act
        var result = await task.ToExecutionResultAsync();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.IsSuccess.Should().BeTrue();
        executed.Should().BeTrue();
    }

    [Fact]
    public async Task ToExecutionResultAsync_Task_WhenThrows_ShouldReturnFailureResult()
    {
        // Arrange
        Task task = Task.Run(() => throw new InvalidOperationException("Async error"));

        // Act
        var result = await task.ToExecutionResultAsync();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.IsSuccess.Should().BeFalse();
        result.Errors.ContainsKey("Exception").Should().BeTrue();
    }

    [Fact]
    public async Task ToExecutionResultAsync_Task_WhenThrowsExecutionResultException_ShouldNotWrap()
    {
        // Arrange
        var originalResult = ExecutionResultExtensions.BadRequest().Build();
        var executionException = new ExecutionResultException(originalResult);
        Task task = Task.Run(() => throw executionException);

        // Act & Assert
        await task.Invoking(t => t.ToExecutionResultAsync())
            .Should().ThrowAsync<ExecutionResultException>();
    }


    [Fact]
    public async Task ToExecutionResultAsync_TaskWithResult_WhenSuccessful_ShouldReturnSuccessResultWithValue()
    {
        // Arrange
        const string expectedValue = "task result";
        Task<string> task = Task.FromResult(expectedValue);

        // Act
        var result = await task.ToExecutionResultAsync();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedValue);
    }

    [Fact]
    public async Task ToExecutionResultAsync_TaskWithResult_WhenThrows_ShouldReturnFailureResult()
    {
        // Arrange
        Task<string> task = Task.Run<string>((Func<string>)(() => throw new InvalidOperationException("Async error with result")));

        // Act
        var result = await task.ToExecutionResultAsync();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task ToExecutionResultAsync_TaskWithResult_WhenThrowsExecutionResultException_ShouldNotWrap()
    {
        // Arrange
        var originalResult = ExecutionResultExtensions.BadRequest().Build();
        var executionException = new ExecutionResultException(originalResult);
        Task<string> task = Task.Run<string>((Func<string>)(() => throw executionException));

        // Act & Assert
        await task.Invoking(t => t.ToExecutionResultAsync())
            .Should().ThrowAsync<ExecutionResultException>();
    }

    [Fact]
    public void ToExecutionResult_Func_WhenSuccessful_ShouldReturnSuccessResultWithValue()
    {
        // Arrange
        const int expectedValue = 42;
        Func<int> func = () => expectedValue;

        // Act
        var result = func.ToExecutionResult();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedValue);
    }

    [Fact]
    public void ToExecutionResult_Func_WhenThrows_ShouldReturnFailureResult()
    {
        // Arrange
        Func<string> func = () => throw new InvalidOperationException("Function error");

        // Act
        var result = func.ToExecutionResult();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
    }

    [Fact]
    public void ToExecutionResult_Func_WhenThrowsExecutionResultException_ShouldNotWrap()
    {
        // Arrange
        var originalResult = ExecutionResultExtensions.BadRequest().Build();
        var executionException = new ExecutionResultException(originalResult);
        Func<string> func = () => throw executionException;

        // Act & Assert
        func.Invoking(f => f.ToExecutionResult())
            .Should().Throw<ExecutionResultException>();
    }

    [Fact]
    public void ToExecutionResult_Func_WithComplexReturnType_ShouldWork()
    {
        // Arrange
        var expectedValue = new TestModel { Id = 1, Name = "Test" };
        Func<TestModel> func = () => expectedValue;

        // Act
        var result = func.ToExecutionResult();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedValue);
        result.Value!.Id.Should().Be(1);
        result.Value.Name.Should().Be("Test");
    }

    [Fact]
    public void ToExecutionResult_Func_WithNullReturnValue_ShouldReturnSuccessWithNull()
    {
        // Arrange
        Func<string?> func = () => null;

        // Act
        var result = func.ToExecutionResult();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task ToExecutionResultAsync_CompletedTask_ShouldReturnImmediately()
    {
        // Arrange
        Task task = Task.CompletedTask;

        // Act
        var result = await task.ToExecutionResultAsync();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ToExecutionResultAsync_DelayedTask_ShouldWaitForCompletion()
    {
        // Arrange
        var executed = false;
        Task task = Task.Delay(50).ContinueWith(_ => executed = true);

        // Act
        var result = await task.ToExecutionResultAsync();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.IsSuccess.Should().BeTrue();
        executed.Should().BeTrue();
    }

    [Fact]
    public async Task ToExecutionResultAsync_CancelledTask_ShouldReturnFailureResult()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        Task task = Task.Delay(1000, cts.Token);

        // Act
        var result = await task.ToExecutionResultAsync();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.GatewayTimeout); // OperationCanceledException maps to this
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ToExecutionResultAsync_TaskWithResult_CancelledTask_ShouldReturnFailureResult()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        Task<string> task = Task.Delay(1000, cts.Token).ContinueWith<string>(_ => "result", cts.Token);

        // Act
        var result = await task.ToExecutionResultAsync();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.GatewayTimeout);
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
    }

    [Theory]
    [InlineData(typeof(ArgumentException), HttpStatusCode.BadRequest)]
    [InlineData(typeof(InvalidOperationException), HttpStatusCode.InternalServerError)]
    [InlineData(typeof(NotSupportedException), HttpStatusCode.MethodNotAllowed)]
    [InlineData(typeof(TimeoutException), HttpStatusCode.RequestTimeout)]
    public void ToExecutionResult_Action_WithSpecificExceptions_ShouldMapToCorrectStatusCodes(Type exceptionType, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test error")!;
        Action action = () => throw exception;

        // Act
        var result = action.ToExecutionResult();

        // Assert
        result.StatusCode.Should().Be(expectedStatusCode);
        result.IsSuccess.Should().BeFalse();
    }

    [Theory]
    [InlineData(typeof(ArgumentException), HttpStatusCode.BadRequest)]
    [InlineData(typeof(InvalidOperationException), HttpStatusCode.InternalServerError)]
    public void ToExecutionResult_Func_WithSpecificExceptions_ShouldMapToCorrectStatusCodes(Type exceptionType, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test error")!;
        Func<string> func = () => throw exception;

        // Act
        var result = func.ToExecutionResult();

        // Assert
        result.StatusCode.Should().Be(expectedStatusCode);
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
    }

    private record TestModel
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}