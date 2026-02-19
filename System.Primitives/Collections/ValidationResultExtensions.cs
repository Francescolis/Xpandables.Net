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
using System.ComponentModel.DataAnnotations;

namespace System.Collections;

/// <summary>
/// Provides extension methods for converting <see cref="ValidationResult"/> instances to <see
/// cref="ElementCollection"/> objects.
/// </summary>
public static class ValidationResultExtensions
{
	/// <summary>
	/// Converts the current <see cref="ValidationResult"/> to an <see cref="ElementCollection"/>.
	/// </summary>
	/// <param name="validationResult">The validation result to convert.</param>
	extension(ValidationResult validationResult)
	{
		/// <summary>
		/// Creates an <see cref="ElementCollection"/> containing entries for each member name associated with the
		/// current validation result error.
		/// </summary>
		/// <remarks>Member names that are null, empty, or consist only of whitespace are excluded from
		/// the returned collection.</remarks>
		/// <returns>An <see cref="ElementCollection"/> containing one entry for each non-empty member name with the associated
		/// error message. Returns <see cref="ElementCollection.Empty"/> if there are no member names or the error
		/// message is null.</returns>
		public ElementCollection ToElementCollection()
		{
			ArgumentNullException.ThrowIfNull(validationResult);

			if (validationResult.ErrorMessage is null || !validationResult.MemberNames.Any())
			{
				return ElementCollection.Empty;
			}

			ElementEntry[] entries = [.. validationResult
				.MemberNames
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Select(s => new ElementEntry(s, validationResult.ErrorMessage))];

			return entries.Length == 0
				? ElementCollection.Empty
				: ElementCollection.With(entries);
		}
	}
}

/// <summary>
/// Provides extension methods for converting <see cref="ValidationResult"/> instances to <see
/// cref="ElementCollection"/> objects.
/// </summary>
public static class EnumerableValidationResultExtensions
{
	/// <summary>
	/// Provides extension methods for converting  IEnumerable of <see cref="ValidationResult"/> instances to <see cref="ElementCollection"/> objects.  
	/// </summary>
	/// <param name="source">The sequence to act on.</param>
	extension(IEnumerable<ValidationResult> source)
	{
		/// <summary>
		/// Converts a sequence of <see cref="ValidationResult"/> instances to an <see cref="ElementCollection"/>.
		/// </summary>
		/// <returns>An <see cref="ElementCollection"/> containing entries for all member names and error messages from the
		/// source validation results. Returns <see cref="ElementCollection.Empty"/> if the source is empty or contains no
		/// valid entries.</returns>
		public ElementCollection ToElementCollection()
		{
			ArgumentNullException.ThrowIfNull(source);

			ElementEntry[] entries = [.. source
				.Where(s => s is not null && s.ErrorMessage is not null && s.MemberNames is not null)
				.SelectMany(s => s.MemberNames!
					.Where(m => !string.IsNullOrWhiteSpace(m))
					.Select(m => new ElementEntry(m, s.ErrorMessage!)))];

			return entries.Length == 0
				? ElementCollection.Empty
				: ElementCollection.With(entries);
		}
	}
}
