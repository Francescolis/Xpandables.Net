using System.Reflection;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Provides extension methods for injecting ambient context into repository instances.
/// </summary>
/// <remarks>This class contains methods that facilitate the injection of a <c>DataContext</c> or its derived
/// types into repositories that implement the <see cref="IEventStore"/> interface. The injection is performed by
/// setting a writable property or an accessible field of the repository with the provided context.</remarks>
public static class DataContextExtensions
{
    internal static void InjectAmbientContext<TRepository>(TRepository repository, object context)
         where TRepository : class, IRepository
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
                    $"Failed to inject DataContext into property '{contextProperty.Name}' of event store type '{repositoryType.Name}'. " +
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
                    $"Failed to inject DataContext into field '{contextField.Name}' of event store type '{repositoryType.Name}'. " +
                    $"Ensure the field is accessible and compatible with the ambient DataContext type.", ex);
            }
        }
    }

    private static PropertyInfo? FindDataContextProperty(Type repositoryType) =>
        // Look for properties that are assignable from DataContext and are writable
        repositoryType
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(prop =>
                typeof(DataContext).IsAssignableFrom(prop.PropertyType) &&
                prop.CanWrite);

    private static FieldInfo? FindDataContextField(Type repositoryType) =>
        // Look for fields that are assignable from DataContext
        repositoryType
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(field =>
                typeof(DataContext).IsAssignableFrom(field.FieldType) &&
                !field.IsInitOnly);
}
