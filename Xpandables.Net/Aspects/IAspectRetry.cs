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
namespace Xpandables.Net.Aspects;

/// <summary>
/// Represents a marker interface that allows the class implementation to be
/// recognized as an aspect retry.
/// </summary>
public interface IAspectRetry : IAspect
{
    /// <summary>
    /// Handles the exception that occurred during the retry.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="attempt">The current attempt number.</param>
    void OnException(Exception exception, int attempt);
}
