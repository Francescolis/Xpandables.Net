/************************************************************************************************************
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
************************************************************************************************************/
namespace Xpandables.Net.CQRS;

/// <summary>
/// Provides a base class for CQRS.
/// </summary>
public interface ICQRS
{
    /// <summary>
    /// Gets the event identifier.
    /// </summary>
    public Guid Id => Guid.NewGuid();

    /// <summary>
    /// Gets When the event occurred.
    /// </summary>
    public DateTimeOffset OccurredOn => DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the name of the user running associated with the current event.
    /// The default value is associated with the current thread.
    /// </summary>
    public string CreatedBy => Environment.UserName;
}
