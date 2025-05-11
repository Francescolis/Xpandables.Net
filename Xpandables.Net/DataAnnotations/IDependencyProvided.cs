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
namespace Xpandables.Net.DataAnnotations;

/// <summary>
/// Represents a dependency that is provided by an implementation of this interface.
/// </summary>
/// <remarks>This interface is intended to be implemented by types that provide specific dependencies in a
/// dependency injection or service resolution context. It extends the <see cref="IAnnotation"/> interface, allowing
/// additional metadata or functionality to be associated with the dependency.
/// Used with the <see langword="IDependencyRequest{TDependency}"/>.
/// </remarks>
public interface IDependencyProvided : IAnnotation
{
}
