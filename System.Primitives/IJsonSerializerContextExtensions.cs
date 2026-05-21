/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace System;

/// <summary>
/// Provides internal extension methods for JSON serialization types.
/// </summary>
public static class JsonTypeInfoExtensions
{
	extension(JsonTypeInfo jsonTypeInfo)
	{
		/// <summary>
		/// Determines whether the type has known polymorphism characteristics.
		/// </summary>
		/// <returns><see langword="true"/> if the type is sealed, a value type, or has polymorphism options configured; otherwise,
		/// <see langword="false"/>.</returns>
		public bool HasKnownPolymorphism()
			=> jsonTypeInfo.Type.IsSealed || jsonTypeInfo.Type.IsValueType || jsonTypeInfo.PolymorphismOptions is not null;

		/// <summary>
		/// Determines whether this type information should be used for serialization with the specified runtime type.
		/// </summary>
		/// <param name="runtimeType">The runtime type to check compatibility with, or <see langword="null"/> to always use this type information.</param>
		/// <returns><see langword="true"/> if this type information should be used; otherwise, <see langword="false"/>.</returns>
		public bool ShouldUseWith([NotNullWhen(false)] Type? runtimeType)
			=> runtimeType is null || jsonTypeInfo.Type == runtimeType || jsonTypeInfo.HasKnownPolymorphism();
	}
}

/// <summary>
/// Provides internal extension methods for JSON serialization types.
/// </summary>
public static class JsonSerializerOptionsExtensions
{
	extension(JsonSerializerOptions options)
	{
		/// <summary>
		/// Gets the JSON type information for the specified type.
		/// </summary>
		/// <param name="type">The type to retrieve type information for.</param>
		/// <returns>The JSON type information for the specified type.</returns>
		public JsonTypeInfo GetReadOnlyTypeInfo(Type type)
		{
			ArgumentNullException.ThrowIfNull(type);
			options.MakeReadOnly();
			return options.GetTypeInfo(type);
		}
	}
}

/// <summary>
/// Provides internal extension methods for JSON serialization types.
/// </summary>
public static class JsonSerializerContextExtensions
{
	extension(JsonSerializerContext context)
	{
		/// <summary>
		/// Gets the <see cref="JsonTypeInfo"/> for the specified type.
		/// </summary>
		/// <param name="type">The type for which to retrieve the <see cref="JsonTypeInfo"/>.</param>
		/// <returns>The <see cref="JsonTypeInfo"/> associated with the specified type.</returns>
		/// <exception cref="InvalidOperationException">The <see cref="JsonTypeInfo"/> for the specified type cannot be obtained from the context.</exception>
		public JsonTypeInfo GetRequiredTypeInfo(Type type)
		{
			ArgumentNullException.ThrowIfNull(type);
			return context.GetTypeInfo(type)
					?? throw new InvalidOperationException($"Unable to obtain the JsonTypeInfo for type '{type.FullName}' from the context '{context.GetType().FullName}'.");
		}
	}
}
