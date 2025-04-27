using FluentAssertions;

using Xpandables.Net.Executions.Rests;

namespace Xpandables.Net.Test.UnitTests;

public sealed class RestAttributeGeneratorUnitTest
{
    [Fact]
    public void Test() => true.Should().BeTrue();
}

[Rest(Path = "api/user", Location = Rest.Location.Query)]
public sealed record GetUserRequest : IRestRequest;