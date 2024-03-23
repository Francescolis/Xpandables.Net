
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
using System.Text.Json;

using Xpandables.Net.Primitives;

namespace Xpandables.Net.Http;

/// <summary>
/// For route, query and header binding sources in minimal Api, 
/// </summary>
/// <typeparam name="TRequest">The type of the custom binding parameter.</typeparam>
public interface IHttpRequestTryParse<TRequest>
{
    /// <summary>
    /// The method discovered by <see langword="RequestDelegateFactory"/> 
    /// on types used as parameters of route
    /// handler delegates to support custom binding.
    /// </summary>
    /// <param name="value">The string value from the route, 
    /// query and header.</param>
    /// <param name="provider">An instance of provider used to 
    /// control formatting.</param>
    /// <param name="result">An instance of <typeparamref name="TRequest"/> 
    /// if binding successful or null.</param>
    /// <returns><see langword="true"/> if parse successful 
    /// otherwise <see langword="false"/>.</returns>
    public static bool TryParse(
        string? value,
        IFormatProvider provider,
        out TRequest? result)
    {
        result = default;
        _ = provider;

        if (value is null)
            return false;

        Dictionary<string, string> dict = value.Split(',')
            .Chunk(2)
            .ToDictionary(d => d[0], d => d[1]);

        string jsonString = JsonSerializer.
            Serialize(
            dict,
            JsonSerializerDefaultOptions.OptionPropertyNameCaseInsensitiveTrue);

        result = JsonSerializer
            .Deserialize<TRequest>(
            jsonString,
            JsonSerializerDefaultOptions.OptionPropertyNameCaseInsensitiveTrue);

        return result is not null;
    }
}
