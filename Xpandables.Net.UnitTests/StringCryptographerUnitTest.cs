
/*******************************************************************************
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
********************************************************************************/
using FluentAssertions;

using Xpandables.Net.Primitives;
using Xpandables.Net.Primitives.Text;

namespace Xpandables.Net.UnitTests;

public sealed class StringCryptographerUnitTest
{
    [Theory]
    [InlineData("ValueToBeEncrypted", 12)]
    public void Assert_Value_KeySize_CanBeEncrypted(
        string value, ushort keySize)
    {
        string key = TextGenerator.Generate(keySize);
        EncryptedValue encrypted = TextCryptography.Encrypt(value, key);

        encrypted.Should().NotBeNull();
        encrypted.Key.Length.Should().Be(keySize);
    }

    [Theory]
    [InlineData("ValueToBeEncrypted")]
    public void Assert_Value_WithDefaultKeyAndSalt_Encrypted_Decrypted(
        string value)
    {
        EncryptedValue encrypted = TextCryptography.Encrypt(value);

        encrypted.Should().NotBeNull();
        encrypted.Key.Should().NotBeNull();

        string result = TextCryptography.Decrypt(encrypted);

        result.Should().NotBeNull();
        result.Should().Be(value);
    }

    [Theory]
    [InlineData("ValueToBeEncrypted")]
    public void Assert_Value_WithDefaultKeyAndSalt_Encrypted_IsEqualTo_Value(
        string value)
    {
        EncryptedValue encrypted = TextCryptography.Encrypt(value);

        encrypted.Should().NotBeNull();
        encrypted.Key.Should().NotBeNull();

        TextCryptography.AreEqual(encrypted, value).Should().Be(true);
    }
}