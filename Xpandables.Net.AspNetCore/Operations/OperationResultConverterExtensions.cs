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
using System.Net;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Operations;

/// <summary>  
/// Provides extension methods for converting operation results.  
/// </summary>  
public static class OperationResultConverterExtensions
{
    /// <summary>  
    /// Converts an <see cref="ElementCollection"/> to a <see cref="ModelStateDictionary"/>.  
    /// </summary>  
    /// <param name="entries">The collection of elements to convert.</param>  
    /// <returns>A <see cref="ModelStateDictionary"/> containing the converted  
    /// elements.</returns>  
    public static ModelStateDictionary ToModelStateDictionary(
        this ElementCollection entries)
    {
        ModelStateDictionary modelState = new();
        foreach (ElementEntry entry in entries)
        {
            foreach (string value in entry.Values)
            {
                modelState.AddModelError(entry.Key, value);
            }
        }

        return modelState;
    }

    /// <summary>  
    /// Converts a <see cref="ModelStateDictionary"/> to an <see cref="IOperationResult"/>.  
    /// </summary>  
    /// <param name="modelState">The model state dictionary to convert.</param>  
    /// <param name="statusCode">The HTTP status code to use for the operation 
    /// result. Defaults to <see cref="HttpStatusCode.BadRequest"/>.</param>  
    /// <returns>An <see cref="IOperationResult"/> representing the operation 
    /// result.</returns>  
    public static IOperationResult ToOperationResult(
        this ModelStateDictionary modelState,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest) =>
        OperationResults
            .Failure(statusCode)
            .WithErrors(ElementCollection.With(
                modelState
                    .Keys
                    .Where(key => modelState[key]!.Errors.Count > 0)
                    .Select(key =>
                        new ElementEntry(
                            key,
                            modelState[key]!.Errors
                                .Select(error => error.ErrorMessage)
                                .ToArray()))
                    .ToList()))
            .Build();

    /// <summary>  
    /// Converts an <see cref="IOperationResult"/> to an <see cref="IActionResult"/>.  
    /// </summary>  
    /// <param name="operationResult">The operation result to convert.</param>  
    /// <returns>An <see cref="IActionResult"/> representing the operation
    /// result.</returns>  
    public static IActionResult ToActionResult(
        this IOperationResult operationResult) =>
        new ObjectResult(operationResult)
        {
            StatusCode = (int)operationResult.StatusCode,
        };
}
