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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace AspNetCore.Net;

internal static class InternalJsonSerializerExtensions
{
    extension(JsonTypeInfo jsonTypeInfo)
    {
        public bool HasKnownPolymorphism()
     => jsonTypeInfo.Type.IsSealed || jsonTypeInfo.Type.IsValueType || jsonTypeInfo.PolymorphismOptions is not null;

        public bool ShouldUseWith([NotNullWhen(false)] Type? runtimeType)
         => runtimeType is null || jsonTypeInfo.Type == runtimeType || jsonTypeInfo.HasKnownPolymorphism();
    }

    extension(JsonSerializerOptions options)
    {
        public JsonTypeInfo GetReadOnlyTypeInfo(Type type)
        {
            options.MakeReadOnly();
            return options.GetTypeInfo(type);
        }
    }

    extension(JsonSerializerContext context)
    {
        public JsonTypeInfo GetRequiredTypeInfo(Type type)
        => context.GetTypeInfo(type) ?? throw new InvalidOperationException($"Unable to obtain the JsonTypeInfo for type '{type.FullName}' from the context '{context.GetType().FullName}'.");
    }
}