using System.Data.Common;
using System.Net;

using FluentAssertions;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Dependencies;
using Xpandables.Net.Executions.Pipelines;
using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Test.UnitTests;
public sealed class PipelineUnitTest
{
    [Fact]
    public async Task Pipeline_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var request = new TestDependencyRequest { DependencyKeyId = "test-key", Name = "Test" };
        var handler = new TestDependencyRequestHandler();
        var decorators = BuildPipelineDecorators();
        var pipeline = new PipelineRequestHandler<TestDependencyRequest>(handler, decorators);

        // Act
        var result = await pipeline.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessStatusCode.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Pipeline_WithInvalidRequest_ShouldFailValidation()
    {
        // Arrange
        var request = new TestDependencyRequest { DependencyKeyId = "test-key" }; // Empty name will fail validation
        var handler = new TestDependencyRequestHandler();
        var decorators = BuildPipelineDecorators();
        var pipeline = new PipelineRequestHandler<TestDependencyRequest>(handler, decorators);

        // Act
        var result = await pipeline.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessStatusCode.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Pipeline_WithException_ShouldHandleError()
    {
        // Arrange
        var request = new TestDependencyRequest
        {
            DependencyKeyId = "test-key",
            Name = "ThrowError"
        };
        var handler = new TestDependencyRequestHandler();
        var decorators = BuildPipelineDecorators();
        var pipeline = new PipelineRequestHandler<TestDependencyRequest>(handler, decorators);

        // Act
        var result = await pipeline.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessStatusCode.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Pipeline_WithUnitOfWork_ShouldSaveChanges()
    {
        // Arrange
        var request = new TestDependencyRequest { DependencyKeyId = "test-key", Name = "Test" };
        var handler = new TestDependencyRequestHandler();
        var unitOfWork = new TestUnitOfWork();
        var decorators = new IPipelineDecorator<TestDependencyRequest>[]
        {
            new PipelineUnitOfWorkDecorator<TestDependencyRequest>(unitOfWork),
            new PipelineDependencyDecorator<TestDependencyRequest>(new TestDependencyManager())
        };
        var pipeline = new PipelineRequestHandler<TestDependencyRequest>(handler, decorators);

        // Act
        var result = await pipeline.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessStatusCode.Should().BeTrue();
        unitOfWork.SaveChangesCalled.Should().BeTrue();
    }

    private static IEnumerable<IPipelineDecorator<TestDependencyRequest>> BuildPipelineDecorators() =>
        [
            new PipelineValidationDecorator<TestDependencyRequest>(new CompositeValidator<TestDependencyRequest>([new TestCompositeValidator()])),
            new PipelineUnitOfWorkDecorator<TestDependencyRequest>(new TestUnitOfWork()),
            new PipelineDependencyDecorator<TestDependencyRequest>(new TestDependencyManager()),
            new PipelineExceptionDecorator<TestDependencyRequest>()
        ];
}

// Request implementation using DependencyRequest base class
public sealed record TestDependencyRequest : DependencyRequest<TestDependency>,
    IValidationEnabled,
    IUnitOfWorkApplied,
    IDependencyProvided
{
    public string Name { get; init; } = string.Empty;
}

public sealed class TestDependencyRequestHandler :
    IDependencyRequestHandler<TestDependencyRequest, TestDependency>
{
    public Task<ExecutionResult> HandleAsync(
        TestDependencyRequest request,
        TestDependency dependency,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dependency);

        if (request.Name == "ThrowError")
            throw new InvalidOperationException("Test error");

        // Use the dependency in the handling logic
        var result = request.Name.Length > 0
            ? ExecutionResults.Success()
            : ExecutionResults.Failure("Name", "Name is required");

        return Task.FromResult(result);
    }
}

// Concrete implementation for validator
public sealed class TestCompositeValidator : Validator<TestDependencyRequest>
{
    public override ValueTask<ExecutionResult> ValidateAsync(TestDependencyRequest instance)
    {
        var result = string.IsNullOrEmpty(instance.Name)
            ? ExecutionResults.Failure("Name", "Name is required")
            : ExecutionResults.Success();

        return ValueTask.FromResult(result);
    }
}

// Concrete implementation for unit of work
public sealed class TestUnitOfWork : IUnitOfWork
{
    public bool IsTransactional { get; set; }
    public bool SaveChangesCalled { get; private set; }

    public Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public TRepository GetRepository<TRepository>() where TRepository : class, IRepository
        => throw new NotSupportedException("Not needed for this test");
    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCalled = true;
        return Task.FromResult(1);
    }

    public Task<IUnitOfWorkTransaction> UseTransactionAsync(
        DbTransaction transaction,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();
}

// Concrete implementation for dependency management
public sealed class TestDependencyManager : IDependencyManager
{
    public IDependencyProvider GetDependencyProvider(Type dependencyType)
        => new TestDependencyProvider();
}

public sealed class TestDependencyProvider : IDependencyProvider
{
    public bool CanProvideDependency(Type dependencyType) => true;
    public Task<object> GetDependencyAsync(
        IDependencyRequest __,
        CancellationToken _ = default)
        => Task.FromResult<object>(new TestDependency());
}

// Test dependency
public sealed class TestDependency
{
    public string Value { get; init; } = "Test Dependency";
}
