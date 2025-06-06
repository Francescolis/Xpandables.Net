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
namespace Xpandables.Net.DataAnnotations;

/// <summary>
/// Represents an annotation with metadata about its usage.
/// </summary>
/// <remarks>This interface provides a mechanism to associate metadata, such as the usage timestamp, with an
/// annotation.</remarks>
public interface IAnnotation
{
    /// <summary>
    /// The date and time when the annotation was used.
    /// </summary>
    public DateTime UsedOn => DateTime.UtcNow;
}
