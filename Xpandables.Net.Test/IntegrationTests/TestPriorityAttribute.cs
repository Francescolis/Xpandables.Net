using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xpandables.Net.Test.IntegrationTests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class TestPriorityAttribute(int priority) : Attribute
{
    public int Priority { get; } = priority;
}

internal sealed class PriorityOrderer : ITestCaseOrderer
{
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
        where TTestCase : ITestCase
    {
        var sortedMethods = testCases
            .OrderBy(testCase => testCase.TestMethod.Method
                .GetCustomAttributes(typeof(TestPriorityAttribute))
                .OfType<IAttributeInfo>()
                .Select(attr => attr.GetNamedArgument<int>("Priority"))
                .FirstOrDefault());

        return sortedMethods;
    }
}