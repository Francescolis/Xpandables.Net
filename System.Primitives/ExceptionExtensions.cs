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
using System.Text;

namespace System;

/// <summary>
/// Provides extension methods for working with <see cref="Exception"/> instances, enabling enhanced exception message
/// retrieval and analysis.
/// </summary>
/// <remarks>Use the methods in this class to extract detailed information from exception objects, such as
/// aggregating messages from nested inner exceptions for improved logging and diagnostics. All methods are static and
/// designed to simplify common exception handling scenarios.</remarks>
public static class ExceptionExtensions
{
    extension(Exception exception)
    {
        /// <summary>
        /// Retrieves the full message chain from the exception and all inner exceptions as a single string.
        /// </summary>
        /// <remarks>Use this method to obtain a comprehensive error message for logging or diagnostic
        /// purposes. The returned string includes the message from the initial exception followed by messages from each
        /// inner exception in order.</remarks>
        /// <returns>A string containing the messages from the exception and each inner exception, separated by line breaks.</returns>
        public string GetFullExceptionMessage()
        {
            ArgumentNullException.ThrowIfNull(exception);

            StringBuilder message = new();
            _ = message.AppendLine(exception.Message);

            while (exception.InnerException is not null)
            {
                exception = exception.InnerException;
                _ = message.AppendLine(exception.Message);
            }

            return message.ToString();
        }
    }
}
