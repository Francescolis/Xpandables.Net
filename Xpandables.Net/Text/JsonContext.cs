using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Xpandables.Net.Collections;
using Xpandables.Net.Executions;
using Xpandables.Net.Optionals;
using Xpandables.Net.Executions.Rests;

namespace Xpandables.Net.Text;

[JsonSerializable(typeof(ExecutionResult))]
[JsonSerializable(typeof(Xpandables.Net.Collections.ElementEntry))]
[JsonSerializable(typeof(Xpandables.Net.Executions.ErrorMessagePoco))]
[JsonSerializable(typeof(Optional<string>))]
[JsonSerializable(typeof(Optional<object>))]
[JsonSerializable(typeof(TokenValue))]
[JsonSerializable(typeof(RefreshTokenValue))]
[JsonSerializable(typeof(RestResponse))]
[JsonSerializable(typeof(ExecutionResult<string>))]
[JsonSerializable(typeof(ElementCollection))]
[JsonSerializable(typeof(Xpandables.Net.Collections.ElementEntry[]))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Operation))]
[JsonSerializable(typeof(List<Operation>))]
[JsonSerializable(typeof(Dictionary<string, object>))] // For ITokenDecoder
public partial class DefaultJsonSerializerContext : JsonSerializerContext
{
}
