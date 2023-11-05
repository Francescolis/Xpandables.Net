
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Xpandables.Net.Operations;

/// <summary>
/// <see cref="IOperationResult"/> extensions.
/// </summary>
public static partial class OperationResultExtensions
{
    internal static string GetProblemDetail(this IOperationResult operationResult)
         => operationResult.Detail.IsNotEmpty switch
         {
             true => operationResult.Detail,
             _ => operationResult.StatusCode.GetProblemDetail()
         };

    internal static string GetProblemTitle(this IOperationResult operationResult)
         => operationResult.Title.IsNotEmpty switch
         {
             true => operationResult.Title,
             _ => operationResult.StatusCode.GetProblemTitle()
         };


    /// <summary>
    /// Returns a <see cref="ModelStateDictionary"/> from the collection of errors.
    /// </summary>
    /// <param name="errors">The collection of errors to act on.</param>
    /// <returns>A new instance of <see cref="ModelStateDictionary"/> that contains found errors.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="errors"/> is null.</exception>
    public static ModelStateDictionary ToModelStateDictionary(this ElementCollection errors)
    {
        var modelStateDictionary = new ModelStateDictionary();
        foreach (var error in errors)
        {
            foreach (var errorMessage in error.Values)
                modelStateDictionary.AddModelError(error.Key, errorMessage);
        }

        return modelStateDictionary;
    }

    /// <summary>
    /// Returns a <see cref="IDictionary{TKey, TValue}"/> from the collection of errors to act for errors property on <see cref="ValidationProblemDetails"/>.
    /// </summary>
    /// <param name="errors">The collection of errors to act on.</param>
    /// <returns>A collection of key and values that contains found errors.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="errors"/> is null.</exception>
    public static IDictionary<string, string[]> ToMinimalErrors(this ElementCollection errors)
    {
        var modelDictionary = new Dictionary<string, string[]>();
        foreach (var error in errors)
        {
            var messages = new List<string>();
            foreach (var errorMessage in error.Values)
                messages.Add(errorMessage);

            modelDictionary.Add(error.Key, [.. messages]);
        }

        return modelDictionary;
    }

    /// <summary>
    /// Returns an instance of <see cref="ObjectResult"/> that contains the current operation as value and <see cref="IOperationResult.StatusCode"/> as status code.
    /// In that case, the response will be converted to the matching action result by the <see cref="OperationResultControllerFilter"/>.
    /// </summary>
    /// <param name="operationResult">The current operation result to act with.</param>
    /// <returns>An implementation of <see cref="IActionResult"/> response with <see cref="IOperationResult.StatusCode"/>.</returns>
    /// <remarks>You may derive from <see cref="OperationResultControllerFilter"/> class to customize its behaviors.</remarks>
    public static IActionResult ToActionResult(this IOperationResult operationResult)
    {
        ArgumentNullException.ThrowIfNull(operationResult);

        return new ObjectResult(operationResult) { StatusCode = (int)operationResult.StatusCode };
    }

    /// <summary>
    /// Returns an implementation of <see cref="IResult"/> that contains the current operation as value.
    /// </summary>
    /// <param name="operationResult">The current operation result to act with.</param>
    /// <returns>An implementation of <see cref="IResult"/> response with <see cref="IOperationResult.StatusCode"/>.</returns>
    public static IResult ToMinimalResult(this IOperationResult operationResult)
    {
        return new OperationResultMinimal(operationResult);
    }
}
