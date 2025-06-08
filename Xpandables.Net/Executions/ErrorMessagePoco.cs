using System.Collections.Generic;

namespace Xpandables.Net.Executions;

public class ErrorMessagePoco
{
    public Dictionary<string, IEnumerable<string>> Errors { get; } = new();
}
