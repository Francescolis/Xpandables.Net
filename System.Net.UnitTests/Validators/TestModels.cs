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
using System.ComponentModel.DataAnnotations;
using System.Net.Validators;

using Xpandables.Net.Validators;

namespace Xpandables.Net.UnitTests.Validators;

public class TestModel : IRequiresValidation
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters")]
    public string Name { get; set; } = string.Empty;

    [Range(0, 120, ErrorMessage = "Age must be between 0 and 120")]
    public int Age { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }

    public DateTime UsedOn { get; set; } = DateTime.UtcNow;
}

public class EmptyTestModel : IRequiresValidation
{
    public DateTime UsedOn { get; set; } = DateTime.UtcNow;
}

public class ComplexTestModel : IRequiresValidation
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Value { get; set; }

    [MinLength(1)]
    public List<string> Items { get; set; } = [];

    public DateTime UsedOn { get; set; } = DateTime.UtcNow;
}

public class CustomTestValidator : Validator<TestModel>
{
    public override IReadOnlyCollection<ValidationResult> Validate(TestModel instance)
    {
        var results = base.Validate(instance).ToList();

        if (instance.Age < 0)
        {
            results.Add(new ValidationResult("Age cannot be negative", [nameof(TestModel.Age)]));
        }

        if (!string.IsNullOrEmpty(instance.Email) && instance.Email.Length > 100)
        {
            results.Add(new ValidationResult("Email is too long", [nameof(TestModel.Email)]));
        }

        return results;
    }
}

public sealed class AlwaysFailValidator : Validator<TestModel>
{
    public override IReadOnlyCollection<ValidationResult> Validate(TestModel instance)
    {
        return [new ValidationResult("Always fails", [nameof(TestModel.Name)])];
    }
}

public sealed class SecondValidator : Validator<TestModel>
{
    public override IReadOnlyCollection<ValidationResult> Validate(TestModel instance)
    {
        var results = new List<ValidationResult>();
        if (string.IsNullOrEmpty(instance.Name) || instance.Name.Length < 5)
        {
            results.Add(new ValidationResult("Name must be at least 5 characters", [nameof(TestModel.Name)]));
        }
        return results;
    }
}

public sealed class AsyncTestValidator : Validator<TestModel>
{
    public override async ValueTask<IReadOnlyCollection<ValidationResult>> ValidateAsync(TestModel instance)
    {
        // Simulate async work
        await Task.Delay(1);

        var results = new List<ValidationResult>();
        if (instance.Age > 100)
        {
            results.Add(new ValidationResult("Age is too high for async validation", [nameof(TestModel.Age)]));
        }

        return results;
    }
}