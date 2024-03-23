
/*******************************************************************************
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
********************************************************************************/
using System.Diagnostics;
using System.Text.Json;

using Xpandables.Net.Primitives.I18n;
using Xpandables.Net.Primitives.Text;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Aggregates.Notifications;

/// <summary>
/// Represents a notification to be written (outbox pattern).
/// Make use of <see langword="using"/> key work when call or call dispose method.
/// </summary>
[DebuggerDisplay("Id = {" + nameof(Id) + "}")]
public sealed class EntityNotification : Entity<Guid>, IDisposable
{
    /// <summary>
    /// Constructs a notification entity from the specified notification.
    /// </summary>
    /// <param name="event">The notification to act with.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>An instance of <see cref="EntityNotification"/> 
    /// built from the notification.</returns>
    /// <exception cref="InvalidOperationException">
    /// The action specified failed.</exception>
    public static EntityNotification ToEntityNotification(
        INotification @event,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(@event);

        try
        {

            string typeFullName = @event.GetTypeFullName();
            JsonDocument eventData = @event.ToJsonDocument(options);

            return new(@event.Id, typeFullName, eventData);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException
                and not InvalidOperationException)
        {
            throw new InvalidOperationException(
                    I18nXpandables.ActionSpecifiedFailedSeeException
                        .StringFormat(nameof(ToEntityNotification)),
                    exception);
        }
    }

    /// <summary>
    /// Constructs a notification from the specified entity.
    /// </summary>
    /// <param name="entity">The entity to act with.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>An instance of notification built from the entity.</returns>
    /// <exception cref="InvalidOperationException">
    /// The action specified failed.</exception>
    public static INotification? ToNotification(
        EntityNotification entity,
        JsonSerializerOptions? options)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            if (Type.GetType(entity.TypeFullName) is not { } eventType)
                return null;

            object? eventObject = entity.Data.Deserialize(eventType, options);
            return eventObject as INotification;
        }
        catch (Exception exception)
              when (exception is not ArgumentNullException
                  and not InvalidOperationException)
        {
            throw new InvalidOperationException(
                    I18nXpandables.ActionSpecifiedFailedSeeException
                        .StringFormat(nameof(ToNotification)),
                    exception);
        }
    }

    private EntityNotification(
        Guid id,
        string typeFullName,
        JsonDocument data,
        string? errorMessage = default)
    {
        Id = id;
        TypeFullName = typeFullName;
        Data = data;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Gets the type full name of the notification.
    /// </summary>
    public string TypeFullName { get; }

    /// <summary>
    /// Gets the data of the notification.
    /// </summary>
    public JsonDocument Data { get; }

    /// <summary>
    /// Gets or sets the error message of the notification.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Releases the <see cref="Data"/> resource.
    /// </summary>
    public void Dispose()
    {
        Data?.Dispose();
        GC.SuppressFinalize(this);
    }
}