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
using System.Net;
using System.Text.Json;

using ElementCollection = global::System.Collections.ElementCollection;
using ElementEntry = global::System.Collections.ElementEntry;

namespace System.Results;

/// <summary>
/// Provides extension methods for converting string messages into <see cref="FailureResult"/> instances.
/// </summary>
public static class StringResultExtensions
{
    /// <summary>
    /// Creates a <see cref="FailureResult"/> from a plain message or an embedded problem-details JSON payload.
    /// </summary>
    /// <param name="message">The message to convert into a failure result.</param>
    /// <returns>A <see cref="FailureResult"/> containing the parsed status, title, detail, and errors when available.</returns>
    public static FailureResult ToResult(this string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!TryGetJsonPayload(message, out string jsonPayload))
        {
            return Result
                .Failure()
                .WithStatusCode(HttpStatusCode.BadRequest)
                .WithTitle(message)
                .WithDetail(HttpStatusCode.BadRequest.Detail)
                .Build();
        }

#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            using var document = JsonDocument.Parse(jsonPayload);
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return CreateDefaultFailure(message);
            }

            HttpStatusCode statusCode = TryGetStatusCode(root, out HttpStatusCode parsedStatusCode)
                ? parsedStatusCode
                : HttpStatusCode.BadRequest;
            string title = TryGetStringPropertyIgnoreCase(root, "title", out string? parsedTitle) && !string.IsNullOrWhiteSpace(parsedTitle)
                ? parsedTitle
                : statusCode.Title;
            string detail = TryGetStringPropertyIgnoreCase(root, "detail", out string? parsedDetail) && !string.IsNullOrWhiteSpace(parsedDetail)
                ? parsedDetail
                : statusCode.Detail;
            ElementCollection errors = GetErrors(root);

            return Result
                .Failure()
                .WithStatusCode(statusCode)
                .WithTitle(title)
                .WithDetail(detail)
                .WithErrors(errors)
                .Build();
        }
        catch
        {
            return CreateDefaultFailure(message);
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    private static FailureResult CreateDefaultFailure(string message) =>
        Result
            .Failure()
            .WithStatusCode(HttpStatusCode.BadRequest)
            .WithTitle(message)
            .WithDetail(HttpStatusCode.BadRequest.Detail)
            .Build();

    private static bool TryGetJsonPayload(string message, out string payload)
    {
        ReadOnlySpan<char> trimmed = message.AsSpan().Trim();
        if (trimmed.Length == 0)
        {
            payload = string.Empty;
            return false;
        }

        char first = trimmed[0];
        if (first is '{' or '[')
        {
            payload = trimmed.ToString();
            return true;
        }

        for (int i = 0; i < message.Length; i++)
        {
            char current = message[i];
            if (current is not '{' and not '[')
            {
                continue;
            }

            if (TryExtractBalancedJsonSegment(message, i, out payload))
            {
                return true;
            }
        }

        payload = string.Empty;
        return false;
    }

    private static bool TryExtractBalancedJsonSegment(string message, int startIndex, out string payload)
    {
        payload = string.Empty;
        if (startIndex < 0 || startIndex >= message.Length)
        {
            return false;
        }

        var delimiters = new Stack<char>();
        bool inString = false;
        bool isEscaped = false;

        for (int i = startIndex; i < message.Length; i++)
        {
            char current = message[i];

            if (inString)
            {
                if (isEscaped)
                {
                    isEscaped = false;
                    continue;
                }

                if (current == '\\')
                {
                    isEscaped = true;
                    continue;
                }

                if (current == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (current == '"')
            {
                inString = true;
                continue;
            }

            if (current is '{' or '[')
            {
                delimiters.Push(current);
                continue;
            }

            if (current is '}' or ']')
            {
                if (delimiters.Count == 0)
                {
                    return false;
                }

                char opening = delimiters.Pop();
                if ((current == '}' && opening != '{') || (current == ']' && opening != '['))
                {
                    return false;
                }

                if (delimiters.Count == 0)
                {
                    payload = message[startIndex..(i + 1)];
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryGetStatusCode(JsonElement root, out HttpStatusCode statusCode)
    {
        statusCode = HttpStatusCode.BadRequest;
        if (!TryGetPropertyIgnoreCase(root, "status", out JsonElement statusElement))
        {
            return false;
        }

        if (statusElement.ValueKind == JsonValueKind.Number && statusElement.TryGetInt32(out int statusValue)
            && Enum.IsDefined(typeof(HttpStatusCode), statusValue))
        {
            statusCode = (HttpStatusCode)statusValue;
            return true;
        }

        return false;
    }

    private static bool TryGetStringPropertyIgnoreCase(JsonElement root, string name, out string? value)
    {
        value = null;
        if (!TryGetPropertyIgnoreCase(root, name, out JsonElement element) || element.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = element.GetString();
        return true;
    }

    private static ElementCollection GetErrors(JsonElement root)
    {
        if (!TryGetPropertyIgnoreCase(root, "errors", out JsonElement errorsElement) || errorsElement.ValueKind != JsonValueKind.Object)
        {
            return ElementCollection.Empty;
        }

        var entries = new List<ElementEntry>();
        foreach (JsonProperty property in errorsElement.EnumerateObject())
        {
            if (string.IsNullOrWhiteSpace(property.Name))
            {
                continue;
            }

            if (property.Value.ValueKind == JsonValueKind.String)
            {
                string? value = property.Value.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    entries.Add(new ElementEntry(property.Name, value));
                }
                continue;
            }

            if (property.Value.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            List<string>? values = null;
            foreach (JsonElement item in property.Value.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                string? itemValue = item.GetString();
                if (string.IsNullOrWhiteSpace(itemValue))
                {
                    continue;
                }

                values ??= [];
                values.Add(itemValue);
            }

            if (values is { Count: > 0 })
            {
                entries.Add(new ElementEntry(property.Name, values.ToArray()));
            }
        }

        return entries.Count == 0 ? ElementCollection.Empty : ElementCollection.With(entries);
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement obj, string name, out JsonElement value)
    {
        if (obj.TryGetProperty(name, out value))
        {
            return true;
        }

        foreach (JsonProperty property in obj.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}
