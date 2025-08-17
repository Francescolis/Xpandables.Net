
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
/// Represents an annotation with metadata about its usage.
/// </summary>
/// <remarks>This interface provides a mechanism to associate metadata, such as the usage timestamp, with an
/// annotation.</remarks>
public interface IAnnotation
{
    /// <summary>
    /// The date and time when the annotation was used.
    /// </summary>
    public DateTime UsedOn => DateTime.UtcNow;
}

/// <summary>
/// Represents an interface that indicates a requirement for validation capabilities.
/// </summary>
/// <remarks>Implementing this interface suggests that the object supports or requires validation processes,
/// typically to ensure that its state or data meets certain criteria.</remarks>
public interface IRequiresValidation : IAnnotation;

/// <summary>
/// Defines a marker interface to indicate that the request requires 
/// its aggregate dependency to be stored after successful processing.
/// </summary>
/// <remarks>Implementing this interface indicates that a type depends on aggregate storage for its operations or
/// state management. This interface is typically used in scenarios where dependency injection or service location is
/// employed to provide storage capabilities.</remarks>
public interface IRequiresEventStorage : IAnnotation;

/// <summary>
/// Marker interface to indicate that a request requires automatic 
/// unit of work management, ensuring database changes are persisted
/// regardless of the request outcome.
/// </summary>
/// <remarks>Implement this interface to signal that the implementing class should be used within a unit of work
/// pattern. This interface does not define any members and serves as a semantic indicator for dependency injection or
/// other framework-specific behaviors.</remarks>
public interface IRequiresUnitOfWork : IAnnotation;