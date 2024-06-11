
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
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Xpandables.Net.Primitives.Text;

/// <summary>
/// Provides with methods to encrypt and decrypt string values.
/// </summary>
public static class TextCryptography
{
    /// <summary>
    /// Returns an object of <see cref="EncryptedValue"/> type that contains 
    /// the encrypted value, the salt and its key from the value.
    /// If <paramref name="key"/> or <paramref name="salt"/> is not provided, 
    /// a default value will be used.
    /// The process uses the <see cref="RijndaelManaged"/> algorithm with the 
    /// <see cref="SHA256"/>.
    /// </summary>
    /// <param name="value">The value to be encrypted.</param>
    /// <param name="key">The optional key value to be used for 
    /// encryption.</param>
    /// <param name="salt">The optional salt base64 string value to be used 
    /// for encryption.</param>
    /// <returns>An object that contains the encrypted value and 
    /// its key.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="value"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The encryption failed. 
    /// See inner exception.</exception>
    public static EncryptedValue Encrypt(
        string value,
        string? key = default,
        string? salt = default)
        => (EncryptedValue)EncryptDecrypt(value, key, salt, true);

    /// <summary>
    /// Returns a decrypted string from the encrypted object.
    /// The process uses the <see cref="RijndaelManaged"/> algorithm with 
    /// the <see cref="SHA256"/>.
    /// </summary>
    /// <param name="encrypted">The object that contains encrypted 
    /// information.</param>
    /// <returns>A decrypted string from the encrypted object.</returns>
    /// <exception cref="InvalidOperationException">The decryption failed. 
    /// See inner exception.</exception>
    public static string Decrypt(EncryptedValue encrypted)
        => (string)EncryptDecrypt(
            encrypted.Value,
            encrypted.Key,
            encrypted.Salt, false);

    /// <summary>
    /// Compares the encrypted object with the plain text one.
    /// Returns <see langword="true"/> if equality otherwise 
    /// <see langword="false"/>.
    /// </summary>
    /// <param name="encrypted">The encrypted object.</param>
    /// <param name="value">The value to compare with.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="value"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The comparison failed. 
    /// See inner exception.</exception>
    /// <returns><see langword="true"/> if equality otherwise 
    /// <see langword="false"/>.</returns>
    public static bool AreEqual(EncryptedValue encrypted, string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        EncryptedValue comp = Encrypt(value, encrypted.Key, encrypted.Salt);
        return comp == encrypted;
    }

    /// <summary>
    /// Generates a salt base64 string of the specified byte length.
    /// </summary>
    /// <param name="length">The length of the expected string value.</param>
    /// <returns>A new base64 string from the salt bytes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The 
    /// <paramref name="length"/> must be greater than zero
    /// and lower or equal to <see cref="ushort.MaxValue"/>.</exception>
    /// <exception cref="InvalidOperationException">Generating the salt failed. 
    /// See inner exception.</exception>
    public static string GenerateSalt(ushort length = 32)
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

#pragma warning disable CA5401
                using (CryptoStream cryptoStream = new(
                    memoryStream, isEncryption
                        ? aes.CreateEncryptor()
                        : aes.CreateDecryptor(),
                    CryptoStreamMode.Write))
                {
                    cryptoStream.Write(valueBytes, 0, valueBytes.Length);
                }
#pragma warning restore CA5401
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
