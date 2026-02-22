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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace System;

/// <summary>
/// Represents an encrypted value along with its associated key and salt used for encryption.
/// </summary>
/// <remarks>This record struct is designed to encapsulate the necessary components for handling encrypted data
/// securely. The Key, Value, and Salt properties are required for proper encryption and decryption processes.</remarks>
[DebuggerDisplay("Key = {Key}, Value = {Value}, Salt = {Salt}")]
[StructLayout(LayoutKind.Auto)]
public readonly record struct EncryptedValue
{
	/// <summary>
	/// Gets the key used for encryption.
	/// </summary>
	public required string Key { get; init; }
	/// <summary>
	/// Gets the encrypted value.
	/// </summary>
	public required string Value { get; init; }
	/// <summary>
	/// Gets the salt used for encryption.
	/// </summary>
	public required string Salt { get; init; }
}

/// <summary>
///	Provides methods for encrypting, decrypting, and securely comparing text values using cryptographic techniques.
/// </summary>
/// <remarks>The TextCryptography class offers functionality for generating cryptographic salts, encrypting and
/// decrypting text with optional keys and salts, and comparing encrypted values to plain text. It is designed to
/// facilitate secure handling of sensitive information, such as passwords or confidential data, by leveraging
/// industry-standard algorithms. All methods are static and thread-safe, making the class suitable for use in
/// multi-threaded applications.</remarks>
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
	public static EncryptedValue Encrypt(string value, string? key = default, string? salt = default)
		=> EncryptInternal(value, key, salt);

	/// <summary>
	/// Decrypts the specified encrypted value.
	/// </summary>
	/// <param name="encrypted">The encrypted value to decrypt.</param>
	/// <returns>The decrypted string.</returns>
	public static string Decrypt(EncryptedValue encrypted)
		=> DecryptInternal(encrypted.Value, encrypted.Key, encrypted.Salt);

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
			RandomNumberGenerator.Fill(salt);
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
	/// Encrypts the specified value using the provided key and salt.
	/// </summary>
	/// <param name="value">The plain-text value to encrypt.</param>
	/// <param name="key">The key to use for encryption. If null, a key will be generated.</param>
	/// <param name="salt">The salt to use for encryption. If null, a salt will be generated.</param>
	/// <returns>An <see cref="EncryptedValue"/> containing the encrypted value, key, and salt.</returns>
	internal static EncryptedValue EncryptInternal(string value, string? key, string? salt)
	{
		key ??= TextGenerator.Generate(12);
		salt ??= GenerateSalt();

		try
		{
			byte[] valueBytes = Encoding.UTF8.GetBytes(value);
			byte[] keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
			byte[] saltBytes = Convert.FromBase64String(salt);

			using MemoryStream memoryStream = new(valueBytes.Length + 16);
			using var aes = Aes.Create();

			aes.KeySize = 256;
			aes.BlockSize = 128;
			aes.Padding = PaddingMode.PKCS7;
			aes.Mode = CipherMode.CBC;

			int keyLength = aes.KeySize / 8;
			int ivLength = aes.BlockSize / 8;

			byte[] derivedBytes = Rfc2898DeriveBytes.Pbkdf2(
				keyBytes,
				saltBytes,
				100000,
				HashAlgorithmName.SHA256,
				keyLength + ivLength);

			aes.Key = derivedBytes[..keyLength];
			aes.IV = derivedBytes[keyLength..];

			using (CryptoStream cryptoStream = new(
				memoryStream,
#pragma warning disable CA5401
				aes.CreateEncryptor(),
#pragma warning restore CA5401
				CryptoStreamMode.Write))
			{
				cryptoStream.Write(valueBytes);
			}

			return new EncryptedValue
			{
				Key = key,
				Value = Convert.ToBase64String(memoryStream.ToArray()),
				Salt = salt
			};
		}
		catch (Exception exception)
			when (exception is EncoderFallbackException
							or FormatException
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

	/// <summary>
	/// Decrypts the specified cipher-text value using the provided key and salt.
	/// </summary>
	/// <param name="value">The Base64-encoded cipher-text to decrypt.</param>
	/// <param name="key">The key used during encryption.</param>
	/// <param name="salt">The salt used during encryption.</param>
	/// <returns>The decrypted plain-text string.</returns>
	internal static string DecryptInternal(string value, string key, string salt)
	{
		try
		{
			byte[] valueBytes = Convert.FromBase64String(value);
			byte[] keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
			byte[] saltBytes = Convert.FromBase64String(salt);

			using MemoryStream memoryStream = new(valueBytes.Length);
			using var aes = Aes.Create();

			aes.KeySize = 256;
			aes.BlockSize = 128;
			aes.Padding = PaddingMode.PKCS7;
			aes.Mode = CipherMode.CBC;

			int keyLength = aes.KeySize / 8;
			int ivLength = aes.BlockSize / 8;

			byte[] derivedBytes = Rfc2898DeriveBytes.Pbkdf2(
				keyBytes,
				saltBytes,
				100000,
				HashAlgorithmName.SHA256,
				keyLength + ivLength);

			aes.Key = derivedBytes[..keyLength];
			aes.IV = derivedBytes[keyLength..];

			using (CryptoStream cryptoStream = new(
				memoryStream,
				aes.CreateDecryptor(),
				CryptoStreamMode.Write))
			{
				cryptoStream.Write(valueBytes);
			}

			return Encoding.UTF8.GetString(memoryStream.ToArray());
		}
		catch (Exception exception)
			when (exception is EncoderFallbackException
							or FormatException
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
