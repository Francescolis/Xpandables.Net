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
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace System.Results;

/// <summary>
/// Provides a JSON serializer context for AOT-compatible serialization of Result types.
/// </summary>
/// <remarks>
/// This context enables Native AOT compilation by providing compile-time source generation
/// for JSON serialization of all Result-related types. Use this context when serializing
/// or deserializing Result objects in AOT scenarios.
/// </remarks>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false,
    IgnoreReadOnlyProperties = false)]
[JsonSerializable(typeof(Result))]
[JsonSerializable(typeof(Result<object>))]
[JsonSerializable(typeof(SuccessResult))]
[JsonSerializable(typeof(SuccessResult<object>))]
[JsonSerializable(typeof(FailureResult))]
[JsonSerializable(typeof(FailureResult<object>))]
[JsonSerializable(typeof(ElementCollection))]
[JsonSerializable(typeof(ElementEntry))]
[JsonSerializable(typeof(ElementEntry[]))]
[UnconditionalSuppressMessage(
    "Trimming",
    "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
    Justification = "Exception property is marked with JsonIgnore and is not serialized.")]
public partial class ResultJsonContext : JsonSerializerContext;
