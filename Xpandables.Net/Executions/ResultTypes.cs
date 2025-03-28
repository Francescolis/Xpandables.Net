﻿/*******************************************************************************
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
namespace Xpandables.Net.Executions;

/// <summary>  
/// Represents a file result with content, file name, and content type.  
/// </summary>  
public sealed record ResultFile
{
    /// <summary>  
    /// Gets the content of the file.  
    /// </summary>  
    public required IReadOnlyCollection<byte> Content { get; init; }

    /// <summary>  
    /// Gets the name of the file.  
    /// </summary>  
    public required string FileName { get; init; }

    /// <summary>  
    /// Gets the content type of the file.  
    /// </summary>  
    public required string ContentType { get; init; }
}
