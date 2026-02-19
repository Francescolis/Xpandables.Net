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
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace System.Results;

/// <summary>
/// Provides extension methods for converting collections of <see cref="ValidationResult"/> objects to results
/// suitable for API responses.
/// </summary>
/// <remarks>These extension methods facilitate mapping validation errors to standardized result
/// formats, enabling consistent error handling in API endpoints. The methods are intended to be used with collections
/// of <see cref="ValidationResult"/> produced by validation frameworks.</remarks>
public static class ValidationResultExtensions
{
    ///<summary>
    /// Extension methods for converting collections of <see cref="ValidationResult"/> objects to results.
    ///</summary>  
    extension(IEnumerable<ValidationResult> validations)
    {
        /// <summary>
        /// Creates a <see cref="Result"/> representing a bad request, including validation errors from the current
        /// collection.
        /// </summary>
        /// <remarks>This method requires that the validation collection is not null and contains at least
        /// one item. The returned result includes a title, detail, and a list of errors based on the validations. Use
        /// this method to generate standardized error responses for invalid input scenarios.</remarks>
        /// <returns>A <see cref="Result"/> instance configured as a bad request, containing details and errors derived from the
        /// validation collection.</returns>
        public Result ToResult()
        {
            ArgumentNullException.ThrowIfNull(validations);
            ArgumentOutOfRangeException.ThrowIfZero(validations.Count());

            var elements = validations.ToElementCollection();
            return Result
                .BadRequest()
                .WithTitle(HttpStatusCode.BadRequest.Title)
                .WithDetail(HttpStatusCode.BadRequest.Detail)
                .WithErrors(elements)
                .Build();
        }
    }
}
