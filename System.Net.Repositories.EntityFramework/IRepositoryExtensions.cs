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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace System.Net.Repositories;

/// <summary>
/// Provides extension methods for repository types to enable advanced behaviors such as ambient context injection.
/// </summary>
/// <remarks>This static class contains helper methods that extend the functionality of repository
/// implementations, allowing them to participate in shared data contexts and other coordinated operations. These
/// extensions are intended to simplify repository integration with infrastructure components such as ambient
/// contexts.</remarks>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class IRepositoryExtensions
{
    /// <summary>
    /// <see cref="IRepository"/> extensions.
    /// </summary>
    /// <typeparam name="TRepository">The type of the repository. </typeparam>
    extension<TRepository>(TRepository repository)
        where TRepository : class, IRepository
    {
        /// <summary>
        /// Injects the specified ambient <see cref="DataContext"/> into the underlying repository instance, enabling it
        /// to participate in the current data context.
        /// </summary>
        /// <remarks>This method attempts to set a <see cref="DataContext"/> on the repository by first
        /// searching for a writable property, then for an accessible field of type <see cref="DataContext"/>. If
        /// neither is found or injection fails, an exception is thrown. This enables repositories to share the same
        /// ambient context for coordinated data operations.</remarks>
        /// <param name="context">The <see cref="DataContext"/> to inject into the repository. Cannot be null.</param>
        /// <exception cref="InvalidOperationException">Thrown if the repository does not have a writable property or accessible field of type <see
        /// cref="DataContext"/>, or if the injection fails due to type incompatibility or access restrictions.</exception>
        public void InjectAmbientContext(DataContext context)
        {
            var repositoryType = repository.GetType();

            // Try to find a writable property of DataContext type
            var contextProperty = FindDataContextProperty(repositoryType);
            if (contextProperty != null)
            {
                try
                {
                    contextProperty.SetValue(repository, context);
                    return;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Failed to inject DataContext into property '{contextProperty.Name}' of store type '{repositoryType.Name}'. " +
                        $"Ensure the property has a public setter and is compatible with the ambient DataContext type.", ex);
                }
            }

            // Try to find a field of DataContext type
            var contextField = FindDataContextField(repositoryType);
            if (contextField != null)
            {
                try
                {
                    contextField.SetValue(repository, context);
                    return;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Failed to inject DataContext into field '{contextField.Name}' of store type '{repositoryType.Name}'. " +
                        $"Ensure the field is accessible and compatible with the ambient DataContext type.", ex);
                }
            }
        }
    }

    private static PropertyInfo? FindDataContextProperty([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type repositoryType) =>
    // Look for properties that are assignable from DataContext and are writable
        repositoryType
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(prop =>
                typeof(DataContext).IsAssignableFrom(prop.PropertyType) &&
                prop.CanWrite);

    private static FieldInfo? FindDataContextField([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type repositoryType) =>
        // Look for fields that are assignable from DataContext
        repositoryType
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(field =>
                typeof(DataContext).IsAssignableFrom(field.FieldType) &&
                !field.IsInitOnly);

}
