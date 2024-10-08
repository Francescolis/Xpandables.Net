using System.Net;
using System.Text.Json;

using FluentAssertions;

using Xpandables.Net.Collections;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Test.UnitTests;

public sealed class OperationResultUnitTest
{
    [Fact]
    public void OperationResult_Should_Initialize()
    {
        SuccessBuilder<string> builder = new(HttpStatusCode.OK);
        IOperationResult result = builder
            .WithLocation(new Uri("http://localhost"))
            .WithResult("Name")
            .WithHeader("Key", "Value")
            .WithExtension("Key", "Value")
            .Build();

        IOperationResult result2 = result;

        string resultJson = JsonSerializer.Serialize(result);
        IOperationResult result3 = JsonSerializer.Deserialize<OperationResult<string>>(resultJson)!;

        result2.Errors.Add(new ElementEntry("Key", "Value"));
        result2.Headers.Add(new ElementEntry("Key", "Value"));
        result2.Extensions.Add(new ElementEntry("Key", "Value"));
        result2.StatusCode.Should().Be(HttpStatusCode.OK);
        result2.Title.Should().Be("Title");
        result2.Detail.Should().Be("Detail");
        result2.Location.Should().Be(new Uri("http://localhost"));
        result2.Result.Should().BeEquivalentTo("Name");
        result2.Errors.Should().ContainSingle();
        result2.Headers.Should().ContainSingle();
        result2.Extensions.Should().ContainSingle();
        result2.IsSuccessStatusCode.Should().Be(true);
    }
}
