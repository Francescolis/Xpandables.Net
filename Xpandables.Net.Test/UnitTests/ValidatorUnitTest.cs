using System.ComponentModel.DataAnnotations;

using FluentAssertions;

using Moq;

using Xpandables.Net.DataAnnotations;

namespace Xpandables.Net.Test.UnitTests;

public sealed class ValidatorUnitTest
{
    [Fact]
    public void Validate_ReturnsSuccess_WhenInstanceIsValid()
    {
        var instance = new ToBeValidated { Name = "Name", Age = 35 };
        var validator = new Validator<ToBeValidated>();

        var result = validator.Validate(instance);

        result.IsSuccessStatusCode.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ReturnsFailure_WhenInstanceIsInvalid()
    {
        var instance = new ToBeValidated { Name = "Name", Age = 120 };
        var validator = new Validator<ToBeValidated>();

        var result = validator.Validate(instance);

        result.IsSuccessStatusCode.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_ReturnsSuccess_WhenInstanceIsValid()
    {
        var instance = new ToBeValidated { Name = "Name", Age = 50 };
        var validator = new Validator<ToBeValidated>();

        var result = await validator.ValidateAsync(instance);

        result.IsSuccessStatusCode.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_ReturnsFailure_WhenInstanceIsInvalid()
    {
        var instance = new ToBeValidated { Name = "Name", Age = 120 };
        var validator = new Validator<ToBeValidated>();

        var result = await validator.ValidateAsync(instance);

        result.IsSuccessStatusCode.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
}

public record ToBeValidated : IRequiresValidation
{
    [Required] public string? Name { get; set; }

    [Range(0, 100)] public int Age { get; set; }
}