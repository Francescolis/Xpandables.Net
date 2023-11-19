/************************************************************************************************************
 * Copyright (C) 2022 Francis-Black EWANE
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

namespace Xpandables.Net.Operations.Specifications;

/// <summary>
/// Defines one method which returns boolean to assert that a collection 
/// of specifications is satisfied or not.
/// method used to check whether or not the specification is satisfied 
/// by the <typeparamref name="TSource"/> object.
/// </summary>
/// <typeparam name="TSource">Type of the argument to be validated.</typeparam>
public interface ICompositeSpecification<TSource> : ISpecification<TSource>
{ }

/// <summary>
/// The composite specification class used to wrap all specifications for a specific type.
/// </summary>
/// <typeparam name="TSource">The type of the object to check for.</typeparam>
[Serializable]
public record class CompositeSpecification<TSource> :
    Specification<TSource>, ICompositeSpecification<TSource>
{
    private readonly IEnumerable<ISpecification<TSource>> _specificationInstances;

    /// <summary>
    /// Initializes the composite specification with all specification instances for the argument.
    /// </summary>
    /// <param name="specificationInstances">The collection of specifications to act with.</param>
    public CompositeSpecification(IEnumerable<ISpecification<TSource>> specificationInstances)
        => _specificationInstances = specificationInstances;

    ///<inheritdoc/>
    protected sealed override void ApplySpecification(TSource source)
    {
        if (!_specificationInstances.Any())
            return;

        var failedResults = _specificationInstances
            .Where(spec => !spec.IsSatisfiedBy(source))
            .Select(spec => spec.Result)
            .ToArray();

        if (failedResults.Length <= 0)
            return;

        ElementCollection errors = [];
        foreach (var result in failedResults)
        {
            errors.Merge(result.Errors);
        }

        Result = OperationResults
            .Failure(failedResults.First().StatusCode)
            .WithErrors(errors)
            .Build();
    }
}
