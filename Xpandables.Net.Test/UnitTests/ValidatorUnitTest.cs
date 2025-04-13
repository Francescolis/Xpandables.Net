using System.ComponentModel.DataAnnotations;

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

        Assert.True(result.IsSuccessStatusCode);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ReturnsFailure_WhenInstanceIsInvalid()
    {
        var instance = new ToBeValidated { Name = "Name", Age = 120 };
        var validator = new Validator<ToBeValidated>();

        var result = validator.Validate(instance);

        Assert.False(result.IsSuccessStatusCode);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsSuccess_WhenInstanceIsValid()
    {
        var instance = new ToBeValidated { Name = "Name", Age = 50 };
        var validator = new Validator<ToBeValidated>();

        var result = await validator.ValidateAsync(instance);

        Assert.True(result.IsSuccessStatusCode);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsFailure_WhenInstanceIsInvalid()
    {
        var instance = new ToBeValidated { Name = "Name", Age = 120 };
        var validator = new Validator<ToBeValidated>();

        var result = await validator.ValidateAsync(instance);

        Assert.False(result.IsSuccessStatusCode);
        Assert.NotEmpty(result.Errors);
    }
}

public record ToBeValidated : IValidationEnabled
{
    [Required] public string? Name { get; set; }

    [Range(0, 100)] public int Age { get; set; }
}