
/************************************************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
************************************************************************************************************/
using System.Net;
using System.Text.Json;

using FluentAssertions;

using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.UnitTests;
public sealed class OperationResultUnitTest
{
    [Theory]
    [InlineData("key", "Header")]
    public void OperationResult_Should_Return_Headers(string hKey, string hValue)
    {
        IOperationResult operationResult = OperationResults
            .Ok()
            .WithHeader(hKey, hValue)
            .Build();

        operationResult.Headers
            .First()
            .Key
            .Should().Be(hKey);
    }

    [Theory]
    [InlineData("key", "Header", "errorKey", "errorMessage")]
    public void OperationResult_Should_Return_Errors(string hKey, string hValue, string eKey, string eMessage)
    {
        IOperationResult operationResult = OperationResults
            .BadRequest()
            .WithError(eKey, eMessage)
            .WithHeader(hKey, hValue)
            .Build();

        operationResult.Headers
            .First()
            .Key
            .Should()
            .Be(hKey);

        operationResult.Errors
            .First()
            .Values
            .First()
            .Should()
            .Be(eMessage);
    }

    [Theory]
    [InlineData("key", "Header", "errorKey", "errorMessage", "result", "http://localhost7152/")]
    public void JsonConverter_Should_Serialize_And_Deserialize_OperationResult(
        string hKey, string hValue, string eKey, string eMessage, string result, string url)
    {
        IOperationResult<string> badResult = OperationResults
            .BadRequest<string>()
            .WithError(eKey, eMessage)
            .WithHeader(hKey, hValue)
            .Build();

        IOperationResult<string> okResult = OperationResults
            .Ok(result)
            .WithHeader(hKey, hValue)
            .WithUrl(url)
            .Build();

        string barResultJson = JsonSerializer.Serialize(badResult);
        string okResultJson = JsonSerializer.Serialize(okResult);

        IOperationResult<string> expectedOkResult = JsonSerializer.Deserialize<IOperationResult<string>>(okResultJson)!;
        IOperationResult<string> expectedBadResult = JsonSerializer.Deserialize<IOperationResult<string>>(barResultJson)!;

        expectedBadResult.Headers.First()
            .Key
            .Should()
            .Be(hKey);
        expectedBadResult.Headers.First()
            .Values.First()
            .Should()
            .Be(hValue);
        expectedBadResult.Errors.First()
            .Values.First()
            .Should()
            .Be(eMessage);
        
        expectedOkResult.Headers.First()
            .Key
            .Should()
            .Be(hKey);
        expectedOkResult.Headers.First()
            .Values.First()
            .Should()
            .Be(hValue);
        expectedOkResult.LocationUrl.Value
            .Should()
            .Be(url);
    }

    [Theory]
    [InlineData(HttpStatusCode.Accepted)]
    public void OperationResult_Should_Be_Success(HttpStatusCode statusCode)
    {
        Func<HttpStatusCode> action = () => statusCode.EnsureSuccessStatusCode();

        action().Should().Be(statusCode);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    public void OperationResult_Should_Be_Failure(HttpStatusCode statusCode)
    {
        Func<HttpStatusCode> action = () => statusCode.EnsureSuccessStatusCode();

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void OperationResult_Should_Return_Empty_Optional()
    {
        IOperationResult<StructType> optional = OperationResults
            .NotFound<StructType>()
            .WithError("Structype", "erromessage")
            .Build();

        optional.Result.Should().BeEmpty();
        optional.IsFailure.Should().BeTrue();
        optional.Result.IsEmpty.Should().BeTrue();
    }

    readonly record struct StructType(string Value);
}
