
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

// Ignore Spelling: Multipart

using static Xpandables.Net.Http.Requests.HttpClientParameters;

namespace Xpandables.Net.Http.Requests;

/// <summary>
/// Provides with a method to retrieve the request content 
/// for <see cref="BodyFormat.Multipart"/> type.
/// </summary>
public interface IHttpRequestMultipart : IHttpRequest, IHttpRequestStream, IHttpRequestString
{
    /// <summary>
    /// Returns the file name of the HTTP content to add.
    /// </summary>
    string GetFileName();

    /// <summary>
    /// Returns the name of the HTTP content to add.
    /// </summary>
    /// <remarks>The default value is 'file'.</remarks>
    public string GetName() => "file";
}
