using FluentAssertions;

using Xpandables.Net.DataAnnotations;

namespace Xpandables.Net.Test.UnitTests;

public sealed class SpecificationUnitTest
{
    [Fact]
    public static void Test()
    {
        Specification<string> spec = new() { Expression = x => x.Length > 0 };
        Specification<string> spec1 = new() { Expression = x => x.Length > 5 };
        spec &= spec1;
        bool result = spec.IsSatisfiedBy("Hello, World!");
        Console.WriteLine(result);
        result.Should().BeTrue();
    }
}
