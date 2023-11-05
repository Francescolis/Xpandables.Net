
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
using System.Text.Json;

using FluentAssertions;

using Xpandables.Net.Primitives;

namespace Xpandables.Net.UnitTests;
public sealed class PrimitiveUnitTest
{
    [Theory]
    [InlineData("\"email@email.com\"", "email@email.com")]
    public void AssertThatPrimitiveJsonStringValueIsConvertedToPrimitiveType(string jsonEmail, string expectedResult)
    {
        Email instance = JsonSerializer.Deserialize<Email>(jsonEmail);
        instance.Value.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("email@email.com")]
    public void AssertThatPrimitivePropertyIsConverted(string expectedResult)
    {
        Person person = new() { Email = new(expectedResult) };
        string jsonPerson = JsonSerializer.Serialize(person);

        Person instance = JsonSerializer.Deserialize<Person>(jsonPerson)!;
        instance.Email.Value.Should().Be(expectedResult);
    }

    [PrimitiveJsonConverter]
    public readonly record struct Email(string Value) : IPrimitive<Email, string>
    {
        public static string DefaultValue => "NOEMAIL@EMAIL.COM";

        public static Email CreateInstance(string value) => new Email(value);

        public static Email DefaultInstance() => new(DefaultValue);

        public static implicit operator string(Email self)
        {
            throw new NotImplementedException();
        }
    }

    public class Person
    {
        public Email Email { get; set; }
    }
}
