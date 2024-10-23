
/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Xpandables.Net.Text;
/// <summary>
/// Provides methods for encrypting and decrypting text values.
/// </summary>
public static class TextCryptography
{
    /// <summary>
    /// Encrypts the specified value using the provided key and salt.
    /// </summary>
    /// <param name="value">The value to encrypt.</param>
    /// <param name="key">The key to use for encryption. If null, a key 
    /// will be generated.</param>
    /// <param name="salt">The salt to use for encryption. If null, a salt 
    /// will be generated.</param>
    /// <returns>An <see cref="EncryptedValue"/> containing the encrypted 
    /// value, key, and salt.</returns>
    public static EncryptedValue Encrypt(
        string value,
        string? key = default,
        string? salt = default)
        => (EncryptedValue)EncryptDecrypt(value, key, salt, true);

    /// <summary>
    /// Decrypts the specified encrypted value.
    /// </summary>
    /// <param name="encrypted">The encrypted value to decrypt.</param>
    /// <returns>The decrypted string.</returns>
    public static string Decrypt(EncryptedValue encrypted)
        => (string)EncryptDecrypt(
            encrypted.Value,
            encrypted.Key,
            encrypted.Salt, false);

    /// <summary>
    /// Compares an encrypted value with a plain text value to determine if 
    /// they are equal.
    /// </summary>
    /// <param name="encrypted">The encrypted value to compare.</param>
    /// <param name="value">The plain text value to compare.</param>
    /// <returns>True if the encrypted value matches the plain text value; 
    /// otherwise, false.</returns>
    public static bool AreEqual(EncryptedValue encrypted, string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        EncryptedValue comp = Encrypt(value, encrypted.Key, encrypted.Salt);
        return comp == encrypted;
    }

    /// <summary>
    /// Generates a cryptographic salt of the specified length.
    /// </summary>
    /// <param name="length">The length of the salt to generate. 
    /// Must be between 1 and 65535.</param>
    /// <returns>A base64 encoded string representing the generated salt.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the length is 
    /// less than 1 or greater than 65535.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the salt 
    /// generation fails due to a cryptographic error.</exception>
    public static string GenerateSalt(int length = 32)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(length, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, ushort.MaxValue);

        try
        {
            byte[] salt = new byte[length];
            using RandomNumberGenerator random = RandomNumberGenerator.Create();

            random.GetNonZeroBytes(salt);

            return Convert.ToBase64String(salt);
        }
        catch (Exception exception)
            when (exception is CryptographicException)
        {
            throw new InvalidOperationException(
                $"{nameof(GenerateSalt)} : Generating salt for '{length}' " +
                $"characters failed. See inner exception.",
                exception);
        }
    }

    /// <summary>
    /// Encrypts or decrypts the specified value using the provided key and salt.
    /// </summary>
    /// <param name="value">The value to encrypt or decrypt.</param>
    /// <param name="key">The key to use for encryption or decryption. If null, a key 
    /// will be generated.</param>
    /// <param name="salt">The salt to use for encryption or decryption. If null, a salt 
    /// will be generated.</param>
    /// <param name="isEncryption">True to encrypt the value; false to decrypt.</param>
    /// <returns>An encrypted or decrypted value.</returns>
    internal static object EncryptDecrypt(
        string value,
        string? key = default,
        string? salt = default,
        bool isEncryption = true)
    {
        key ??= TextGenerator.Generate(12);
        salt ??= GenerateSalt();

        try
        {
            byte[] valueBytes = isEncryption
                ? Encoding.UTF8.GetBytes(value)
                : Convert.FromBase64String(value);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] saltBytes = Convert.FromBase64String(salt);

            keyBytes = SHA256.HashData(keyBytes);
            string encryptedDecryptedString;

            using (MemoryStream memoryStream = new())
            {
                using Aes aes = Aes.Create();

                using Rfc2898DeriveBytes rfcKey = new(
                    keyBytes,
                    saltBytes,
                    1000,
                    HashAlgorithmName.SHA256);

                rfcKey.IterationCount = 100000;

                if (isEncryption)
                {
                    aes.Padding = PaddingMode.PKCS7;
                }

                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Key = rfcKey.GetBytes(aes.KeySize / 8);
                aes.IV = rfcKey.GetBytes(aes.BlockSize / 8);
                aes.Mode = CipherMode.CBC;

                using (CryptoStream cryptoStream = new(
                    memoryStream, isEncryption
                        ? aes.CreateEncryptor()
                        : aes.CreateDecryptor(),
                    CryptoStreamMode.Write))
                {
                    cryptoStream.Write(valueBytes, 0, valueBytes.Length);
                }

                byte[] encryptedDescripted = memoryStream.ToArray();
                encryptedDecryptedString = isEncryption
                    ? Convert.ToBase64String(encryptedDescripted)
                    : Encoding.UTF8.GetString(encryptedDescripted);
            }

            return isEncryption
                ? new EncryptedValue
                {
                    Key = key,
                    Value = encryptedDecryptedString,
                    Salt = salt
                }
                : encryptedDecryptedString;
        }
        catch (Exception exception)
            when (exception is EncoderFallbackException
                            or ObjectDisposedException
                            or ArgumentException
                            or ArgumentOutOfRangeException
                            or NotSupportedException
                            or TargetInvocationException)
        {
            throw new InvalidOperationException(
                "Cryptography failed. See inner exception.",
                exception);
        }
    }
}
