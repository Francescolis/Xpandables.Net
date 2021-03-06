﻿
/************************************************************************************************************
 * Copyright (C) 2020 Francis-Black EWANE
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
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

using static System.Net.Mime.MediaTypeNames;

namespace Xpandables.Net.Http
{
    /// <summary>
    /// Describes the parameters for a request used with <see cref="IHttpRestClientHandler"/>.
    /// The attribute should decorate implementations of <see cref="IHttpRestClientRequest"/>, <see cref="IHttpRestClientAsyncRequest{TResponse}"/> or <see cref="IHttpRestClientRequest{TResponse}"/>
    /// in order to be used with <see cref="IHttpRestClientHandler"/>.
    /// Your class can implement the <see cref="IHttpRestClientAttributeProvider"/> to dynamically return a <see cref="HttpRestClientAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HttpRestClientAttribute : Attribute
    {
        /// <summary>
        /// Initializes the default instance of <see cref="HttpRestClientAttribute"/>.
        /// </summary>
        public HttpRestClientAttribute() { }

        /// <summary>
        /// Gets or sets the Uri path. If null, the root path will be set.
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// Gets or sets the location of data.
        /// The default value is <see cref="ParameterLocation.Body"/>.
        /// </summary>
        public ParameterLocation In { get; set; } = ParameterLocation.Body;

        ///// <summary>
        ///// Gets or sets the header / cookie name for <see cref="In"/> = <see cref="ParameterLocation.Cookie"/> or <see cref="ParameterLocation.Header"/>.
        ///// </summary>
        //public string HeaderCookieName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the method name.
        /// The default value is <see cref="HttpMethodVerbs.Post"/>.
        /// </summary>
        public string Method { get; set; } = HttpMethodVerbs.Post;

        /// <summary>
        /// Gets or sets the format of the data.
        /// The default value is <see cref="DataFormat.Json"/>.
        /// </summary>
        public DataFormat DataFormat { get; set; }

        /// <summary>
        /// Gets or sets the body format for data.
        /// The default value is <see cref="BodyFormat.String"/>.
        /// </summary>
        public BodyFormat BodyFormat { get; set; }

        /// <summary>
        /// Gets or sets the content type.
        /// The default value is <see cref="ContentType.Json"/>.
        /// </summary>
        public string ContentType { get; set; } = Http.ContentType.Json;

        /// <summary>
        /// Gets or sets the accept content.
        /// The default value is <see cref="ContentType.Json"/>.
        /// </summary>
        public string Accept { get; set; } = Http.ContentType.Json;

        /// <summary>
        /// Gets the value indicating whether or not the request needs authorization.
        /// The default value is <see langword="true"/>.
        /// In this case, an <see cref="AuthenticationHeaderValue"/> with the <see cref="Scheme"/> value will be initialized and filled
        /// with <see cref="IHttpHeaderAccessor"/> reading the "Authorization" key. You should add <see langword="ConfigureXPrimaryAuthorizationTokenHandler"/> extension method
        /// when registering <see cref="IHttpRestClientHandler"/>.
        /// </summary>
        public bool IsSecured { get; set; } = true;

        /// <summary>
        /// Gets the value indicating whether or not the target class should be added to the request body.
        /// If <see langword="true"/> the target class will not be added.
        /// The default value is <see langword="false"/>.
        /// Be aware of the fact that, setting this value to <see langword="true"/> will disable all parameters linked to <see cref="ParameterLocation.Body"/>.
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// Gets or sets the authorization scheme.
        /// The default value is "Bearer".
        /// </summary>
        public string Scheme { get; set; } = "Bearer";

        /// <summary>
        /// Gets or sets the built-in Uri.
        /// </summary>
        internal Uri Uri { get; set; } = null!;
    }

    /// <summary>
    /// The HTTP verbs.
    /// </summary>
    public static class HttpMethodVerbs
    {
        /// <summary>
        /// Retrieves the information or entity that is identified by the URI of the request.
        /// </summary>
        public const string Get = "Get";

        /// <summary>
        /// Posts a new entity as an addition to a URI.
        /// </summary>
        public const string Post = "Post";

        /// <summary>
        /// Replaces an entity that is identified by a URI.
        /// </summary>
        public const string Put = "Put";

        /// <summary>
        /// Requests that a specified URI be deleted.
        /// </summary>
        public const string Delete = "Delete";

        /// <summary>
        /// Retrieves the message headers for the information or entity that is identified by the URI of the request.
        /// </summary>
        public const string Head = "Head";

        /// <summary>
        /// Requests that a set of changes described in the request entity be applied to the resource identified by the Request- URI.
        /// </summary>
        public const string Patch = "Patch";

        /// <summary>
        /// Represents a request for information about the communication options available on the request/response chain identified by the Request-URI.
        /// </summary>
        public const string Options = "Options";

        /// <summary>
        /// Performs a message loop-back test along the path to the target resource.
        /// </summary>
        public const string Trace = "Trace";

        /// <summary>
        /// Establishes a tunnel to the server identified by the target resource.
        /// </summary>
        public const string Connect = "Connect";
    }

    /// <summary>
    /// The location of the parameter, can be combined.
    /// </summary>
    [Flags]
    public enum ParameterLocation
    {
        /// <summary>
        /// Used in the content of the request. You can use <see cref="IStringRequest"/>, <see cref="IPatchRequest"/>, <see cref="IStreamRequest"/>, <see cref="IByteArrayRequest"/>, 
        /// <see cref="IMultipartRequest"/> or <see cref="IFormUrlEncodedRequest"/> to customize the body content, otherwise the whole class will be serialized.
        /// </summary>
        Body = 0x0,

        /// <summary>
        /// Parameters that are appended to the URL. You must implement <see cref="IQueryStringLocationRequest"/> to provide with content.
        /// </summary>
        Query = 0x1,

        /// <summary>
        /// Used together with Path Templating, where the parameter value is actually part of the operation's URL. You must implement <see cref="IPathStringLocationRequest"/> to provide with content.
        /// </summary>
        Path = 0x2,

        /// <summary>
        /// Custom headers that are expected as part of the request. You must implement <see cref="IHeaderLocationRequest"/> to provide with content.
        /// </summary>
        Header = 0x4,

        /// <summary>
        /// Used to pass a specific cookie value to the API. You must <see cref="ICookieLocationRequest"/> to provide with content.
        /// </summary>
        Cookie = 0x8
    }

    /// <summary>
    /// Determines the body format of the request.
    /// </summary>
    public enum BodyFormat
    {
        /// <summary>
        /// Body content matching the <see cref="StringContent"/>.
        /// The target class should implement <see cref="IStringRequest"/>, <see cref="IPatchRequest"/>, otherwise the whole class will be serialized.
        /// </summary>
        String,

        /// <summary>
        /// Body content matching the <see cref="ByteArrayContent"/>.
        /// The target class should implement <see cref="IByteArrayRequest"/>.
        /// </summary>
        ByteArray,

        /// <summary>
        /// Body content matching the <see cref="MultipartFormDataContent"/>.
        /// The target class should implement <see cref="IMultipartRequest"/>.
        /// </summary>
        Multipart,

        /// <summary>
        /// Body content matching the <see cref="StreamContent"/>.
        /// The target class should implement <see cref="IStreamRequest"/>.
        /// </summary>
        Stream,

        /// <summary>
        /// Body content matching the <see cref="FormUrlEncodedContent"/>.
        /// The target class should implement <see cref="IFormUrlEncodedRequest"/>.
        /// </summary>
        FormUrlEncoded
    }

    /// <summary>
    /// Provides with the content type.
    /// </summary>
    public static class ContentType
    {
        /// <summary>
        /// Returns the application json content type.
        /// </summary>
        public const string Json = "application/json";

        /// <summary>
        /// Returns the application json patch content type
        /// </summary>
        public const string JsonPatch = "application/json-patch+json";

        /// <summary>
        /// Returns the application XML content type.
        /// </summary>
        public const string Xml = "application/xml";

        /// <summary>
        /// Returns the application pdf content type.
        /// </summary>
        public const string Pdf = "application/pdf";

        /// <summary>
        /// Returns the image jpeg content type.
        /// </summary>
        public const string Jpeg = "image/jpeg";

        /// <summary>
        /// Returns the image png content type.
        /// </summary>
        public const string Png = "image/png";

        /// <summary>
        /// Returns the multi part form data content type.
        /// </summary>
        public const string Multipart = "multipart/form-data";

        /// <summary>
        /// Returns the text plain content type.
        /// </summary>
        public const string Text = "text/plain";

        /// <summary>
        /// Collections of content type from data format.
        /// </summary>
        public static readonly IReadOnlyDictionary<DataFormat, string> DataFormats = new Dictionary<DataFormat, string>()
        {
            { DataFormat.Xml, Xml },
            { DataFormat.Json, Json },
            { DataFormat.Jpeg, Jpeg },
            { DataFormat.Multipart, Multipart },
            { DataFormat.Pdf, Pdf },
            { DataFormat.Png, Png },
            { DataFormat.Text, Text }
        };

        /// <summary>
        /// Returns the json accept header.
        /// </summary>
        public static readonly string[] JsonHeader = new string[]
        {
            "application/json",
            "text/json",
            "text/x-json",
            "text/javascript",
            "*+json"
        };

        /// <summary>
        /// Returns the XML accept header.
        /// </summary>
        public static readonly string[] XmlHeader = new string[]
        {
            "application/xml",
            "text/xml",
            "*+xml",
            "*"
        };
    }

    /// <summary>
    /// Determines the format of the target data.
    /// </summary>
    public enum DataFormat
    {
        /// <summary>
        /// Uses for the JSON format.
        /// </summary>
        Json,

        /// <summary>
        /// uses for XML format.
        /// </summary>
        Xml,

        /// <summary>
        /// uses for Pdf format.
        /// </summary>
        Pdf,

        /// <summary>
        /// uses for Jpeg format.
        /// </summary>
        Jpeg,

        /// <summary>
        /// uses for Png format.
        /// </summary>
        Png,

        /// <summary>
        /// uses for Text format.
        /// </summary>
        Text,

        /// <summary>
        /// uses for Multi part format.
        /// </summary>
        Multipart,

        /// <summary>
        /// No specified format.
        /// </summary>
        None
    }
}
