/************************************************************************************************************
 * Copyright (C) 2022 Francis-Black EWANE
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

using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

using Xpandables.Net.Extensions;

namespace Xpandables.Net.Text;

/// <summary>
/// Defines a representation of an encrypted value, its key and its salt used with <see cref="StringCryptographer"/>.
/// </summary>
/// <param name="Key">Contains the encryption key.</param>
/// <param name="Value">Contains the base64 encrypted value.</param>
/// <param name="Salt">Contains the base64 salt value.</param>
/// <exception cref="ArgumentException">The <paramref name="Key"/> or <paramref name="Salt"/> or <paramref name="Value"/> is null.</exception>
[Serializable]
[DebuggerDisplay("Key = {Key}, Value = {Value}, Salt = {Salt}")]
public readonly record struct EncryptedValue([Required] string Key, [Required] string Value, [Required] string Salt);

/// <summary>
/// Provides with methods to encrypt and decrypt string values.
/// </summary>
public static class StringCryptographer
{
    /// <summary>
    /// Returns an object of <see cref="EncryptedValue"/> type that contains the encrypted value, the salt and its key from the value using a randomize key.
    /// The process uses the <see cref="RijndaelManaged"/> algorithm with the <see cref="SHA256"/>.
    /// </summary>
    /// <param name="value">The value to be encrypted.</param>
    /// <param name="keySize">The size of the string key to be used to encrypt the string value.</param>
    /// <returns>An object that contains the encrypted value and its key.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">The <paramref name="keySize"/> must be greater than zero
    /// and lower or equal to <see cref="ushort.MaxValue"/>.</exception>
    /// <exception cref="InvalidOperationException">The encryption failed. See inner exception.</exception>
    public static async ValueTask<EncryptedValue> EncryptAsync(string value, ushort keySize)
    {
        string key = StringGenerator.Generate(keySize);
        string salt = GenerateSalt();

        return await EncryptAsync(value, key, salt).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns an object of <see cref="EncryptedValue"/> type that contains the encrypted value, the salt and its key from the value.
    /// If <paramref name="key"/> or <paramref name="salt"/> is not provided, a default value will be used.
    /// The process uses the <see cref="RijndaelManaged"/> algorithm with the <see cref="SHA256"/>.
    /// </summary>
    /// <param name="value">The value to be encrypted.</param>
    /// <param name="key">The optional key value to be used for encryption.</param>
    /// <param name="salt">The optional salt base64 string value to be used for encryption.</param>
    /// <returns>An object that contains the encrypted value and its key.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The encryption failed. See inner exception.</exception>
    public static async ValueTask<EncryptedValue> EncryptAsync(string value, string? key = default, string? salt = default)
        => (EncryptedValue)await EncryptDecryptAsync(value, key, salt, true).ConfigureAwait(false);

    /// <summary>
    /// Returns an object of <see cref="EncryptedValue"/> type that contains the encrypted value, the salt and its key from the value.
    /// If <paramref name="key"/> or <paramref name="salt"/> is not provided, a default value will be used.
    /// The process uses the <see cref="RijndaelManaged"/> algorithm with the <see cref="SHA256"/>.
    /// </summary>
    /// <param name="value">The value to be encrypted.</param>
    /// <param name="key">The optional key value to be used for encryption.</param>
    /// <param name="salt">The optional salt base64 string value to be used for encryption.</param>
    /// <returns>An object that contains the encrypted value and its key.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The encryption failed. See inner exception.</exception>
    public static EncryptedValue Encrypt(string value, string? key = default, string? salt = default)
    {
        Func<Task<EncryptedValue>> task = () => EncryptAsync(value, key, salt).AsTask();
        return task.RunSync();
    }

    /// <summary>
    /// Returns an decrypted string from the encrypted value.
    /// The process uses the <see cref="RijndaelManaged"/> algorithm with the <see cref="SHA256"/>.
    /// </summary>
    /// <param name="key">The key value to be used for decryption.</param>
    /// <param name="value">The base64 encrypted value to be decrypted.</param>
    /// <param name="salt">The salt base64 string value to be used for decryption.</param>
    /// <returns>A decrypted string from the encrypted values.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="salt"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The decryption failed. See inner exception.</exception>
    public static async ValueTask<string> DecryptAsync(string key, string value, string salt)
        => await DecryptAsync(new EncryptedValue(key, value, salt)).ConfigureAwait(false);

    /// <summary>
    /// Returns a decrypted string from the encrypted object.
    /// The process uses the <see cref="RijndaelManaged"/> algorithm with the <see cref="SHA256"/>.
    /// </summary>
    /// <param name="encrypted">The object that contains encrypted information.</param>
    /// <returns>A decrypted string from the encrypted object.</returns>
    /// <exception cref="InvalidOperationException">The decryption failed. See inner exception.</exception>
    public static async ValueTask<string> DecryptAsync(EncryptedValue encrypted)
        => (string)await EncryptDecryptAsync(encrypted.Value, encrypted.Key, encrypted.Salt, false).ConfigureAwait(false);

    /// <summary>
    /// Returns a decrypted string from the encrypted object.
    /// The process uses the <see cref="RijndaelManaged"/> algorithm with the <see cref="SHA256"/>.
    /// </summary>
    /// <param name="encrypted">The object that contains encrypted information.</param>
    /// <returns>A decrypted string from the encrypted object.</returns>
    /// <exception cref="InvalidOperationException">The decryption failed. See inner exception.</exception>
    public static string Decrypt(EncryptedValue encrypted)
    {
        Func<Task<string>> task = () => DecryptAsync(encrypted).AsTask();
        return task.RunSync();
    }

    /// <summary>
    /// Asynchronously compares the encrypted object with the plain text one.
    /// Returns <see langword="true"/> if equality otherwise <see langword="false"/>.
    /// </summary>
    /// <param name="encrypted">The encrypted object.</param>
    /// <param name="value">The value to compare with.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The comparison failed. See inner exception.</exception>
    /// <returns><see langword="true"/> if equality otherwise <see langword="false"/>.</returns>
    public static async Task<bool> AreEqualAsync(EncryptedValue encrypted, string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var comp = await EncryptAsync(value, encrypted.Key, encrypted.Salt).ConfigureAwait(false);
        return comp == encrypted;
    }

    /// <summary>
    /// Compares the encrypted object with the plain text one.
    /// Returns <see langword="true"/> if equality otherwise <see langword="false"/>.
    /// </summary>
    /// <param name="encrypted">The encrypted object.</param>
    /// <param name="value">The value to compare with.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The comparison failed. See inner exception.</exception>
    /// <returns><see langword="true"/> if equality otherwise <see langword="false"/>.</returns>
    public static bool AreEqual(EncryptedValue encrypted, string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var comp = Encrypt(value, encrypted.Key, encrypted.Salt);
        return comp == encrypted;
    }

    /// <summary>
    /// Generates a salt base64 string of the specified byte length.
    /// </summary>
    /// <param name="length">The length of the expected string value.</param>
    /// <returns>A new base64 string from the salt bytes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="length"/> must be greater than zero
    /// and lower or equal to <see cref="ushort.MaxValue"/>.</exception>
    /// <exception cref="InvalidOperationException">Generating the salt failed. See inner exception.</exception>
    public static string GenerateSalt(ushort length = 32)
    {
        if (length == 0)
            throw new ArgumentException(
                $"{nameof(length)} must be greater than zero and lower or equal to {ushort.MaxValue}");

        try
        {
            var salt = new byte[length];
            using RandomNumberGenerator random = RandomNumberGenerator.Create();

            random.GetNonZeroBytes(salt);

            return Convert.ToBase64String(salt);
        }
        catch (Exception exception) when (exception is CryptographicException)
        {
            throw new InvalidOperationException(
                $"{nameof(GenerateSalt)} : Generating salt for '{length}' characters failed. See inner exception.", exception);
        }
    }

    internal static async ValueTask<object> EncryptDecryptAsync(string value, string? key = default, string? salt = default, bool isEncryption = true)
    {
        key ??= StringGenerator.Generate(12);
        salt ??= GenerateSalt();

        try
        {
            byte[] valueBytes = isEncryption ? Encoding.UTF8.GetBytes(value) : Convert.FromBase64String(value);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] saltBytes = Convert.FromBase64String(salt);

            keyBytes = SHA256.HashData(keyBytes);
            string encryptedDecryptedString;

            using (var memoryStream = new MemoryStream())
            {
                using var aes = Aes.Create();
                using var rfcKey = new Rfc2898DeriveBytes(keyBytes, saltBytes, 1000, HashAlgorithmName.SHA256);
                rfcKey.IterationCount = 100000;

                if (isEncryption) aes.Padding = PaddingMode.PKCS7;

                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Key = rfcKey.GetBytes(aes.KeySize / 8);
                aes.IV = rfcKey.GetBytes(aes.BlockSize / 8);
                aes.Mode = CipherMode.CBC;

                using (CryptoStream cryptoStream = new(memoryStream, isEncryption ? aes.CreateEncryptor() : aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    await cryptoStream.WriteAsync(valueBytes.AsMemory(0, valueBytes.Length), CancellationToken.None).ConfigureAwait(false);
                }

                var encryptedDescripted = memoryStream.ToArray();
                encryptedDecryptedString = isEncryption ? Convert.ToBase64String(encryptedDescripted) : Encoding.UTF8.GetString(encryptedDescripted);
            }

            return isEncryption ? new EncryptedValue(key, encryptedDecryptedString, salt) : encryptedDecryptedString;
        }
        catch (Exception exception) when (exception is EncoderFallbackException
                                              || exception is ObjectDisposedException
                                              || exception is ArgumentException
                                              || exception is ArgumentOutOfRangeException
                                              || exception is NotSupportedException
                                              || exception is TargetInvocationException)
        {
            throw new InvalidOperationException($"{nameof(EncryptAsync)} : cryptography failed. See inner exception.", exception);
        }
    }
}
