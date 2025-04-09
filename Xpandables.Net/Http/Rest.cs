/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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
using System.Collections.Immutable;

namespace Xpandables.Net.Http;

/// <summary>
/// Provides with <see cref="RestAttribute"/> behaviors.
/// </summary>
public static class Rest
{
    /// <summary>
    /// Represents the HTTP methods.
    /// </summary>
    public enum Method
    {
        /// <summary>
        /// Retrieves the information or entity that is identified 
        /// by the URI of the request.
        /// </summary>
        GET,

        /// <summary>
        /// Posts a new entity as an addition to a URI.
        /// </summary>
        POST,

        /// <summary>
        /// Replaces an entity that is identified by a URI.
        /// </summary>
        PUT,

        /// <summary>
        /// Requests that a specified URI be deleted.
        /// </summary>
        DELETE,

        /// <summary>
        /// Retrieves the message headers for the information or 
        /// entity that is identified by the URI of the request.
        /// </summary>
        HEAD,

        /// <summary>
        /// Requests that a set of changes described in the request 
        /// entity be applied to the resource identified by the Request- URI.
        /// </summary>
        /// <remarks>Note that there is no support for minimal Api.</remarks>
        PATCH,

        /// <summary>
        /// Represents a request for information about the communication 
        /// options available on the request/response chain identified by the Request-URI.
        /// </summary>
        OPTIONS,

        /// <summary>
        /// Performs a message loop-back test along the path to the target resource.
        /// </summary>
        TRACE,

        /// <summary>
        /// Establishes a tunnel to the server identified by the target resource.
        /// </summary>
        CONNECT
    }

    /// <summary>
    /// Represents the location of the request content in an HTTP request,
    /// can be combined.
    /// </summary>
    [Flags]
    public enum Location
    {
        /// <summary>
        /// Used in the content of the request. 
        /// You can use <see cref="IRestString"/>,
        /// <see cref="IRestPatch"/>, <see cref="IRestStream"/>, 
        /// <see cref="IRestByteArray"/>, 
        /// <see cref="IRestMultipart"/> or
        /// <see cref="IRestFormUrlEncoded"/>
        /// to customize the body content, otherwise the 
        /// whole class will be serialized.
        /// </summary>
        Body = 1,

        /// <summary>
        /// Parameters that are appended to the URL. You must implement 
        /// <see cref="IRestQueryString"/> to provide with content.
        /// </summary>
        Query = 2,

        /// <summary>
        /// Used together with Path Templating, where the parameter 
        /// value is actually part of the operation's URL.
        /// You must implement <see cref="IRestPathString"/> 
        /// to provide with content.
        /// </summary>
        Path = 4,

        /// <summary>
        /// Custom headers that are expected as part of the request. 
        /// You must implement <see cref="IRestHeader"/> 
        /// to provide with content.
        /// </summary>
        Header = 8,

        /// <summary>
        /// Used to pass a specific cookie value to the API. 
        /// You must <see cref="IRestCookie"/> to provide with content.
        /// </summary>
        Cookie = 16,

        /// <summary>
        /// Used to pass basic auth information to the API.
        /// You must implement <see cref="IRestBasicAuthentication"/>
        /// </summary> 
        BasicAuth = 32
    }

    /// <summary>
    /// Represents the format of the body content in an HTTP request.
    /// </summary>
    public enum BodyFormat
    {
        /// <summary>
        /// Body content matching the <see cref="StringContent"/>.
        /// The target class should implement <see cref="IRestString"/> or
        /// <see cref="IRestPatch"/>, otherwise the 
        /// whole class will be serialized.
        /// </summary>
#pragma warning disable CA1720 // Identifier contains type name
        String,
#pragma warning restore CA1720 // Identifier contains type name

        /// <summary>
        /// Body content matching the <see cref="ByteArrayContent"/>.
        /// The target class should implement <see cref="IRestByteArray"/>.
        /// </summary>
        ByteArray,

        /// <summary>
        /// Body content matching the <see cref="MultipartFormDataContent"/>.
        /// The target class should implement <see cref="IRestMultipart"/>.
        /// </summary>
        Multipart,

        /// <summary>
        /// Body content matching the <see cref="StreamContent"/>.
        /// The target class should implement <see cref="IRestStream"/>.
        /// </summary>
        Stream,

        /// <summary>
        /// Body content matching the <see cref="FormUrlEncodedContent"/>.
        /// The target class should implement 
        /// <see cref="IRestFormUrlEncoded"/>.
        /// </summary>
        FormUrlEncoded
    }

    /// <summary>
    /// Represents the data format used in HTTP requests and responses.
    /// </summary>
    public enum DataFormat
    {
        /// <summary>
        /// Uses for the JSON format.
        /// </summary>
        Json,

        /// <summary>
        /// Uses for XML format.
        /// </summary>
        Xml,

        /// <summary>
        /// Uses for Pdf format.
        /// </summary>
        Pdf,

        /// <summary>
        /// Uses for Jpeg format.
        /// </summary>
        Jpeg,

        /// <summary>
        /// Uses for Png format.
        /// </summary>
        Png,

        /// <summary>
        /// Uses for Text format.
        /// </summary>
        Text,

        /// <summary>
        /// Uses for Multi part format.
        /// </summary>
        Multipart,

        /// <summary>
        /// No specified format.
        /// </summary>
        None
    }

    /// <summary>
    /// Provides constants for HTTP content types.
    /// </summary>
#pragma warning disable CA1034 // Nested types should not be visible
    public static class ContentType
#pragma warning restore CA1034 // Nested types should not be visible
    {
        /// <summary>
        /// Represents the JSON content type.
        /// </summary>
        public const string Json = "application/json";

        /// <summary>  
        /// Represents the JSON Patch content type.  
        /// </summary>  
        public const string JsonPatch = "application/json-patch+json";

        /// <summary>  
        /// Represents the JSON problem content type.  
        /// </summary>  
        public const string JsonProblem = "application/problem+json";

        /// <summary>
        /// Represents the XML content type.
        /// </summary>
        public const string Xml = "application/xml";

        /// <summary>
        /// Represents the JPEG content type.
        /// </summary>
        public const string Jpeg = "image/jpeg";

        /// <summary>  
        /// Represents the PNG content type.  
        /// </summary>  
        public const string Png = "image/png";

        /// <summary>  
        /// Represents the APNG content type.  
        /// </summary>  
        public const string Apng = "image/apng";

        /// <summary>
        /// Represents the GIF content type.
        /// </summary>
        public const string Gif = "image/gif";

        /// <summary>
        /// Represents the SVG content type.
        /// </summary>
        public const string Svg = "image/svg+xml";

        /// <summary>
        /// Represents the WebP content type.
        /// </summary>
        public const string Webp = "image/webp";

        /// <summary>
        /// Represents the PDF content type.
        /// </summary>
        public const string Pdf = "application/pdf";

        /// <summary>
        /// Represents the ZIP content type.
        /// </summary>
        public const string Zip = "application/zip";

        /// <summary>
        /// Represents the plain text content type.
        /// </summary>
        public const string TextPlain = "text/plain";

        /// <summary>
        /// Represents the HTML content type.
        /// </summary>
        public const string TextHtml = "text/html";

        /// <summary>
        /// Represents the XML content type.
        /// </summary>
        public const string TextXml = "text/xml";

        /// <summary>
        /// Represents the CSS content type.
        /// </summary>
        public const string TextCss = "text/css";

        /// <summary>
        /// Represents the JavaScript content type.
        /// </summary>
        public const string TextJavascript = "text/javascript";

        /// <summary>  
        /// Represents the multipart/byteranges content type.  
        /// </summary>  
        public const string MultipartByteRanges = "multipart/byteranges";

        /// <summary>
        /// Represents the multipart/form-data content type.
        /// </summary>
        public const string MultipartFormData = "multipart/form-data";

        /// <summary>
        /// Represents the form URL encoded content type.
        /// </summary>
        public const string FormUrlEncoded = "application/x-www-form-urlencoded";

        /// <summary>
        /// Represents the octet-stream content type.
        /// </summary>
        public const string OctetStream = "application/octet-stream";

        /// <summary>
        /// Represents collection of content types from <see cref="DataFormat"/>.
        /// </summary>
        public static readonly ImmutableDictionary<DataFormat, string>
            DataFormats = new Dictionary<DataFormat, string>()
        {
            { DataFormat.Xml, Xml },
            { DataFormat.Json, Json },
            { DataFormat.Jpeg, Jpeg },
            { DataFormat.Multipart, MultipartFormData },
            { DataFormat.Pdf, Pdf },
            { DataFormat.Png, Png },
            { DataFormat.Text, TextPlain }
        }.ToImmutableDictionary();

        /// <summary>
        /// Represents the JSON accept headers.
        /// </summary>
        public static readonly string[] JsonHeader =
        [
            Json,
            "text/json",
            "text/x-json",
            TextJavascript,
            "*+json"
        ];

        /// <summary>
        /// Represents the XML accept headers.
        /// </summary>
        public static readonly string[] XmlHeader =
        [
            Xml,
            TextXml,
            "*+xml",
            "*"
        ];
    }

    /// <summary>
    /// Provides constants for HTTP operations used in PATCH requests.
    /// </summary>
#pragma warning disable CA1034 // Nested types should not be visible
    public static class Operation
#pragma warning restore CA1034 // Nested types should not be visible
    {
        /// <summary>
        /// The <see cref="Add"/> operation performs one of the following 
        /// functions, depending upon what the target location references:
        /// <list type="number">
        ///     <item>If path points to an array element: inserts new element 
        ///     before the one specified by path.</item>
        ///     <item>If path points to a property: sets the property value.</item>
        ///     <item>If path points to a nonexistent location:
        ///         <list type="bullet">
        ///             <item>If the resource to patch is a dynamic object: 
        ///                 adds a property.</item>
        ///             <item>If the resource to patch is a static object: 
        ///                 the request fails.</item>
        ///         </list>
        ///     </item>
        /// </list>
        /// <example>
        /// For example:
        /// <code> { "op": "add", "path": "/a/b/c", "value": [ "foo", "bar" ] }</code>
        /// </example>
        /// </summary>
        public const string Add = "add";

        /// <summary>
        /// The <see cref="Remove"/> operation removes the value at the target 
        /// location :
        /// <list type="number">
        ///     <item>If path points to an array element: removes the element.</item>
        ///     <item>If path points to a property:</item>
        ///     <list type="bullet">
        ///         <item>If resource to patch is a dynamic object: removes 
        ///             the property.</item>
        ///         <item>If resource to patch is a static object:
        ///             <list type="number">
        ///                 <item>If the property is nullable: sets it to null.
        ///                     </item>
        ///                 <item>If the property is non-nullable, sets it to 
        ///                     default{T}.</item>
        ///             </list>
        ///         </item>
        ///     </list>
        /// </list>
        /// <example>
        /// For example:
        /// <code> { "op": "remove", "path": "/a/b/c" }</code>
        /// </example>
        /// </summary>
        public const string Remove = "remove";

        /// <summary>
        /// The <see cref="Replace"/> operation replaces the value at the target
        /// location with a new value. The operation object MUST contain a 
        /// "value" member whose content specifies the replacement value.
        /// <para>This operation is functionally the same as a remove followed 
        /// by an add.</para>
        /// <para></para>
        /// <example>
        /// For example:
        /// <code> { "op": "replace", "path": "/a/b/c", "value": 42 }</code>
        /// </example>
        /// </summary>
        /// <remarks>The target location MUST exist for the operation to 
        /// be successful.</remarks>
        public const string Replace = "replace";

        /// <summary>
        /// The <see cref="Move"/> operation removes the value at a specified 
        /// location and adds it to the target location.
        /// <list type="number">
        ///     <item>If path points to an array element: copies from element 
        ///     to location of path element, then runs a remove operation on 
        ///     the from element.</item>
        ///     <item>If path points to a property: copies value of from 
        ///     property to path property, then runs a remove operation on the 
        ///     from property.</item>
        ///     <item>If path points to a nonexistent property:
        ///         <list type="bullet">
        ///             <item>If the resource to patch is a static object: 
        ///                 the request fails.</item>
        ///             <item>If the resource to patch is a dynamic object: 
        ///                 copies from property to location indicated by path,
        /// then runs a remove operation on the from property.</item>
        ///         </list>
        ///     </item>
        /// </list>
        /// <para></para>
        /// <example>
        /// For example:
        /// <code> { "op": "move", "from": "/a/b/c", "path": "/a/b/d" }</code>
        /// </example>
        /// </summary>
        public const string Move = "move";

        /// <summary>
        /// The <see cref="Copy"/> operation copies the value at a specified 
        /// location to the target location. The operation object MUST contain 
        /// a "from" member, which is a string containing a JSON Pointer value
        /// that references the location in the target document to copy the 
        /// value from.
        /// <para></para>
        /// <example>
        /// For example:
        /// <code> { "op": "copy", "from": "/a/b/c", "path": "/a/b/e" }</code>
        /// </example>
        /// </summary>
        /// <remarks>This operation is functionally the same as a move operation
        /// without the final remove step.</remarks>
        public const string Copy = "copy";

        /// <summary>
        ///  The <see cref="Test"/> operation tests that a value at the target 
        ///  location is equal to a specified value.
        ///  <para>If the value at the location indicated by path is different 
        ///  from the value provided in value, the request fails. In that case, 
        ///  the whole PATCH request fails even if all other operations in the 
        ///  patch document would otherwise succeed.</para>
        /// <para></para>
        /// <example>
        /// For example:
        /// <code>  { "op": "test", "path": "/a/b/c", "value": "foo" }</code>
        /// </example>
        /// </summary>
        /// <remarks>The test operation is commonly used to prevent an update 
        /// when there's a concurrency conflict.</remarks>
        public const string Test = "test";
    }

    /// <summary>
    /// A helper used to build patch operations.
    /// </summary>
#pragma warning disable CA1034 // Nested types should not be visible
    public static class Patch
#pragma warning restore CA1034 // Nested types should not be visible
    {
        /// <summary>
        /// The <see cref="Add"/> operation performs one of the following 
        /// functions, depending upon what the target location references:
        /// <list type="number">
        ///     <item>If path points to an array element: inserts new element 
        ///         before the one specified by path.</item>
        ///     <item>If path points to a property: sets the property value.</item>
        ///     <item>If path points to a nonexistent location:
        ///         <list type="bullet">
        ///             <item>If the resource to patch is a dynamic object: 
        ///                 adds a property.</item>
        ///             <item>If the resource to patch is a static object: 
        ///             the request fails.</item>
        ///         </list>
        ///     </item>
        /// </list>
        /// <example>
        /// For example:
        /// <code> { "op": "add", "path": "/a/b/c", "value": [ "foo", "bar" ] }</code>
        /// </example>
        /// </summary>
        /// <param name="path">The target location.</param>
        /// <param name="value">The content specifies the value to be 
        /// added.</param>
        /// <returns>The "Add" operation with the specified path and value
        /// .</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="path"/> 
        /// or <paramref name="value"/> is null.</exception>
        public static IPatchOperation Add(string path, object value)
            => new PatchOperation(Operation.Add, path, value);

        /// <summary>
        /// The <see cref="Remove"/> operation removes the value at the target 
        /// location :
        /// <list type="number">
        ///     <item>If path points to an array element: removes the element
        ///     .</item>
        ///     <item>If path points to a property:</item>
        ///     <list type="bullet">
        ///         <item>If resource to patch is a dynamic object: removes 
        ///         the property.</item>
        ///         <item>If resource to patch is a static object:
        ///             <list type="number">
        ///                 <item>If the property is nullable: sets it to null
        ///                 .</item>
        ///                 <item>If the property is non-nullable, sets it to 
        ///                 default{T}.</item>
        ///             </list>
        ///         </item>
        ///     </list>
        /// </list>
        /// <example>
        /// For example:
        /// <code> { "op": "remove", "path": "/a/b/c" }</code>
        /// </example>
        /// </summary>
        /// <param name="path">The target location.</param>
        /// <returns>The "Remove" operation with the specified path.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="path"/> 
        /// is null.</exception>
        public static IPatchOperation Remove(string path)
            => new PatchOperation(Operation.Remove, path);

        /// <summary>
        /// The <see cref="Replace"/> operation replaces the value at the target 
        /// location with a new value. The operation object MUST contain a 
        /// "value" member whose content specifies the replacement value.
        /// <para>This operation is functionally the same as a remove followed 
        /// by an add.</para>
        /// <para></para>
        /// <example>
        /// For example:
        /// <code> { "op": "replace", "path": "/a/b/c", "value": 42 }</code>
        /// </example>
        /// </summary>
        /// <remarks>The target location MUST exist for the operation to be 
        /// successful.</remarks>
        /// <param name="path">The target location.</param>
        /// <param name="value">The content of the new value.</param>
        /// <returns>The "Replace" operation with the specified path and the 
        /// value.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="path"/> 
        /// or <paramref name="value"/> is null.</exception>
        public static IPatchOperation Replace(string path, object value)
            => new PatchOperation(Operation.Remove, path, value);

        /// <summary>
        /// The <see cref="Move"/> operation removes the value at a specified 
        /// location and adds it to the target location.
        /// <list type="number">
        ///     <item>If path points to an array element: copies from element 
        ///     to location of path element,
        /// then runs a remove operation on the from element.</item>
        ///     <item>If path points to a property: copies value of from 
        ///     property to path property,
        /// then runs a remove operation on the from property.</item>
        ///     <item>If path points to a nonexistent property:
        ///         <list type="bullet">
        ///             <item>If the resource to patch is a static object: 
        ///             the request fails.</item>
        ///             <item>If the resource to patch is a dynamic object: 
        ///             copies from property to location indicated by path,
        /// then runs a remove operation on the from property.</item>
        ///         </list>
        ///     </item>
        /// </list>
        /// <para></para>
        /// <example>
        /// For example:
        /// <code> { "op": "move", "from": "/a/b/c", "path": "/a/b/d" }</code>
        /// </example>
        /// </summary>
        /// <param name="from">A string containing a JSON pointer value that 
        /// references the location in the target document to move the value 
        /// from.</param>
        /// <param name="path">The target location to receive the value from 
        /// the 'from' location.</param>
        /// <returns>The "Move" operation with the specified from and path 
        /// values.</returns>
        public static IPatchOperation Move(string from, string path)
            => new PatchOperation(Operation.Move, from, path);

        /// <summary>
        /// The <see cref="Copy"/> operation copies the value at a specified 
        /// location to the target location. The operation object MUST contain 
        /// a "from" member, which is a string containing a JSON Pointer value 
        /// that references the location in the target document to copy the 
        /// value from.
        /// <para></para>
        /// <example>
        /// For example:
        /// <code> { "op": "copy", "from": "/a/b/c", "path": "/a/b/e" }</code>
        /// </example>
        /// </summary>
        /// <remarks>This operation is functionally the same as a move operation 
        /// without the final remove step.</remarks>
        /// <param name="from">A string containing a JSON pointer value that 
        /// references the location in the target document to copy the value 
        /// from.</param>
        /// <param name="path">The target location to receive the value from 
        /// the 'from' location.</param>
        /// <returns>The "Copy" operation with the specified from and path 
        /// values.</returns>
        public static IPatchOperation Copy(string from, string path)
            => new PatchOperation(Operation.Copy, from, path);

        /// <summary>
        ///  The <see cref="Test"/> operation tests that a value at the target 
        ///  location is equal to a specified value.
        ///  <para>If the value at the location indicated by path is different 
        ///  from the value provided in value, the request fails. 
        ///  In that case, the whole PATCH request fails even if all other 
        ///  operations in the patch document would otherwise succeed.</para>
        /// <para></para>
        /// <example>
        /// For example:
        /// <code>  { "op": "test", "path": "/a/b/c", "value": "foo" }</code>
        /// </example>
        /// </summary>
        /// <remarks>The test operation is commonly used to prevent an update 
        /// when there's a concurrency conflict.</remarks>
        /// <param name="path">The target location the value must be equal to 
        /// the specified one.</param>
        /// <param name="value">The value to be compared to the target location
        /// .</param>
        /// <returns>The "test" operation with the specified path and value
        /// .</returns>
        public static IPatchOperation Test(string path, object value)
            => new PatchOperation(Operation.Copy, path, value);
    }
}
