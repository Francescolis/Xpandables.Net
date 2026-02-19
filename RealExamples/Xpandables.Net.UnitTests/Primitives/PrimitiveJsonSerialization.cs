/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
********************************************************************************/
using System.ComponentModel;

namespace Xpandables.Net.UnitTests.Primitives;

public sealed class PrimitiveJsonSerialization
{
    [Fact]
    public void SerializeAndDeserialize_EmailAddress_Should_BeEqual()
    {
        var email = EmailAddress.Create("test@example.com");
        string json = System.Text.Json.JsonSerializer.Serialize(email);
		EmailAddress deserializedEmail = System.Text.Json.JsonSerializer.Deserialize<EmailAddress>(json);
        Assert.Equal(email, deserializedEmail);
    }

    [PrimitiveJsonConverter<EmailAddress, string>]
    [TypeConverter(typeof(PrimitiveTypeConverter<EmailAddress, string>))]
    public readonly record struct EmailAddress : IPrimitive<EmailAddress, string>
    {
        public string Value { get; }
        private EmailAddress(string value) => Value = value;
        public static bool TryParse(string? s, IFormatProvider? provider, out EmailAddress result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(s) || !s.Contains("@"))
            {
                return false;
            }
            result = Create(s);
            return true;
        }
        public static EmailAddress Create(string value) => new(value);
        public static string DefaultValue => string.Empty;

        public static implicit operator string(EmailAddress primitive) => primitive.Value;
        public static implicit operator EmailAddress(string value) => new(value);
    }
}
