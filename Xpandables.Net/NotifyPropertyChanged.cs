
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

using Xpandables.Net.Expressions;

namespace Xpandables.Net;

/// <summary>
/// When used with <see cref="NotifyPropertyChanged"/> or <see cref="NotifyPropertyChanged{T}"/>, 
/// makes sure that the decorated property will be notified
/// when the target specified property by <see cref="Name"/> has changed.
/// </summary>
/// <remarks>
/// Specifies that the decorated property will be notified when the target specified by name has changed.
/// We advise the use of <see langword="nameof(propertyName)"/> as value.
/// </remarks>
/// <param name="name">The name of the target property which changes are notified to the decorated property.</param>
/// <exception cref="ArgumentNullException">The <paramref name="name"/> is null.</exception>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class NotifyPropertyChangedForAttribute(string name) : Attribute
{
    /// <summary>
    /// Gets the name of the target property which changes are notified to the decorated property.
    /// </summary>
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
}

/// <summary>
/// Implementation for <see cref="INotifyPropertyChanged"/>.
/// You can combine the use with <see cref="NotifyPropertyChangedForAttribute"/> to propagate message.
/// </summary>
public abstract class NotifyPropertyChanged : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <summary>
    /// Contains a collection of dependencies on property changed messages.
    /// </summary>
    private IDictionary<string, List<string>> Dependencies { get; }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    /// Notifies listeners that a property value has changed.
    /// </summary>
    /// <param name="e">The event that contains the property name.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="e"/> is <see langword="null"/></exception>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        PropertyChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Notifies listeners that a property value has changed.
    /// </summary>
    /// <param name="propertyName">
    /// Name of the property used to notify listeners.  
    /// This value is optional and can be provided automatically when invoked from compilers
    /// that support <see cref="CallerMemberNameAttribute" />.
    /// </param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Notifies listeners that a property value is changing.
    /// </summary>
    /// <param name="e">The event that contains the property name.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="e"/> is <see langword="null"/></exception>
    protected virtual void OnPropertyChanging(PropertyChangingEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        PropertyChanging?.Invoke(this, e);
    }

    /// <summary>
    /// Notifies listeners that a property value is changing.
    /// </summary>
    /// <param name="propertyName">
    /// Name of the property used to notify listeners.  
    /// This value is optional and can be provided automatically when invoked from compilers
    /// that support <see cref="CallerMemberNameAttribute" />.
    /// </param>
    protected void OnPropertyChanging([CallerMemberName] string propertyName = "")
        => OnPropertyChanging(new PropertyChangingEventArgs(propertyName));

    /// <summary>
    /// Initializes a new instance of <see cref="NotifyPropertyChanged"/> class and its <see cref="Dependencies"/>.
    /// </summary>
    protected NotifyPropertyChanged() => Dependencies = GetType().DependencyPropertiesProvider();

    /// <summary>
    /// Checks if the property does not match the old one.
    /// If so, sets the property and notifies listeners.
    /// </summary>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <param name="field">The field of the property (the back-end field).</param>
    /// <param name="value">The new value of the property (the value).</param>
    /// <param name="onChanged">The delegate to be executed if the value changed.</param>
    /// <param name="propertyName">The name of the property. Optional (Already known at compile time).</param>
    /// <returns><see langword="true"/>if the value was changed, <see langword="false"/>
    /// if the existing value matches the desired value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="propertyName"/> is null or empty.</exception>
    protected bool SetProperty<TValue>(
        [NotNullIfNotNull(nameof(value))] ref TValue field,
        TValue value,
        Action? onChanged = default,
        [CallerMemberName] string propertyName = "")
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);

        if (EqualityComparer<TValue>.Default.Equals(field, value))
            return false;

        OnPropertyChanging(propertyName);

        field = value;

        onChanged?.Invoke();

        OnPropertyChanged(propertyName);

        PropagatePropertyChangedOnDependents(propertyName);

        return true;
    }

    /// <summary>
    /// Checks if the property does not match the old one.
    /// If so, sets the property and notifies listeners.
    /// </summary>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <param name="field">The field of the property (the back-end field).</param>
    /// <param name="value">The new value of the property (the value).</param>
    /// <param name="comparer">The instance comparer used for values comparison.</param>
    /// <param name="onChanged">The delegate to be executed if the value changed.</param>
    /// <param name="propertyName">The name of the property. Optional (Already known at compile time).</param>
    /// <returns><see langword="true"/>if the value was changed, <see langword="false"/>
    /// if the existing value matches the desired value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="propertyName"/> is null or empty.</exception>
    protected bool SetProperty<TValue>(
        [NotNullIfNotNull(nameof(value))] ref TValue field,
        TValue value,
        IEqualityComparer<TValue> comparer,
        Action? onChanged = default,
        [CallerMemberName] string propertyName = "")
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentNullException.ThrowIfNull(comparer);

        if (comparer.Equals(field, value))
            return false;

        OnPropertyChanging(propertyName);

        field = value;

        onChanged?.Invoke();

        OnPropertyChanged(propertyName);

        PropagatePropertyChangedOnDependents(propertyName);

        return true;
    }

    /// <summary>
    /// Checks if the property does not match the old one.
    /// If so, sets the property and notifies listeners.
    /// </summary>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <param name="field">The field of the property (the back-end field).</param>
    /// <param name="value">The new value of the property (the value).</param>
    /// <param name="comparer">The instance comparer used for values comparison.</param>
    /// <param name="onChanged">The delegate to be executed if the value changed.</param>
    /// <param name="propertyName">The name of the property. Optional (Already known at compile time).</param>
    /// <returns><see langword="true"/>if the value was changed, <see langword="false"/>
    /// if the existing value matches the desired value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="propertyName"/> is null or empty.</exception>
    protected bool SetProperty<TValue>(
        [NotNullIfNotNull(nameof(value))] ref TValue field,
        TValue value,
        IEqualityComparer<TValue> comparer,
        Action<TValue, TValue>? onChanged = default,
        [CallerMemberName] string propertyName = "")
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentNullException.ThrowIfNull(comparer);

        if (comparer.Equals(field, value))
            return false;

        OnPropertyChanging(propertyName);

        TValue oldValue = field;
        field = value;

        onChanged?.Invoke(oldValue, value);

        OnPropertyChanged(propertyName);

        PropagatePropertyChangedOnDependents(propertyName);

        return true;
    }

    internal void PropagatePropertyChangedOnDependents(string property)
    {
        var onPropertyChangedAction = new Action<string>(OnPropertyChanged);

        (from keyValues in Dependencies
         from dependent in keyValues.Value
         where keyValues.Key.Equals(property, StringComparison.OrdinalIgnoreCase)
         select dependent)
         .ToList()
         .ForEach(onPropertyChangedAction);
    }
}

/// <summary>
/// Implementation for <see cref="INotifyPropertyChanged"/> and <see cref="INotifyPropertyChanging"/>.
/// You can combine the use with <see cref="NotifyPropertyChangedForAttribute"/> to propagate notification.
/// </summary>
/// <typeparam name="TModel">The type of the target model.</typeparam>
public abstract class NotifyPropertyChanged<TModel> : NotifyPropertyChanged
    where TModel : class
{
    /// <summary>
    /// Initializes a new instance of <see cref="NotifyPropertyChanged{T}"/> class and its dependencies.
    /// </summary>
    protected NotifyPropertyChanged() : base() { }

    /// <summary>
    /// Checks if the property does not match the old one.
    /// If so, sets the property and notifies listeners.
    /// </summary>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <typeparam name="TProperty">Type of the property selector.</typeparam>
    /// <param name="field">The field of the property (the back-end field).</param>
    /// <param name="value">The new value of the property (the value).</param>
    /// <param name="selector">The expression delegate to retrieve the property name.</param>
    /// <returns><see langword="true"/>if the value was changed, <see langword="false"/>
    /// if the existing value matches the desired value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="selector"/> is null.</exception>
    protected bool SetProperty<TValue, TProperty>(
        [NotNullIfNotNull(nameof(value))] ref TValue field,
        TValue value,
        Expression<Func<TModel, TProperty>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);

        string propertyName = selector.GetMemberNameFromExpression();

        if (EqualityComparer<TValue>.Default.Equals(field, value))
            return false;

        OnPropertyChanging(propertyName);

        field = value;

        OnPropertyChanged(propertyName);

        PropagatePropertyChangedOnDependents(propertyName);

        return true;
    }

    /// <summary>
    /// Checks if the property does not match the old one.
    /// If so, sets the property and notifies listeners.
    /// </summary>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <typeparam name="TProperty">Type of the property selector.</typeparam>
    /// <param name="field">The field of the property (the back-end field).</param>
    /// <param name="value">The new value of the property (the value).</param>
    /// <param name="comparer">The instance comparer used for values comparison.</param>
    /// <param name="selector">The expression delegate to retrieve the property name.</param>
    /// <returns><see langword="true"/>if the value was changed, <see langword="false"/>
    /// if the existing value matches the desired value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="selector"/> is null.</exception>
    protected bool SetProperty<TValue, TProperty>(
        [NotNullIfNotNull(nameof(value))] ref TValue field,
        TValue value,
        IEqualityComparer<TValue> comparer,
        Expression<Func<TModel, TProperty>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(comparer);

        string propertyName = selector.GetMemberNameFromExpression();

        if (comparer.Equals(field, value))
            return false;

        OnPropertyChanging(propertyName);

        field = value;

        OnPropertyChanged(propertyName);

        PropagatePropertyChangedOnDependents(propertyName);

        return true;
    }

    /// <summary>
    /// Checks if the property does not match the old one.
    /// If so, updates the property using the updater and notifies listeners.
    /// </summary>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <param name="old">The old value of the property (the old).</param>
    /// <param name="value">The new value of the property (the value).</param>
    /// <param name="model">The model instance.</param>
    /// <param name="updater">The action that will update the property.</param>
    /// <param name="propertyName">The name of the property. Optional (Already known at compile time).</param>
    /// <returns><see langword="true"/>if the value was changed, <see langword="false"/>
    /// if the existing value matches the desired value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="propertyName"/> is null or empty.</exception>
    protected bool SetProperty<TValue>(
        TValue old,
        TValue value,
        TModel model,
        Action<string, TModel, TValue> updater,
        [CallerMemberName] string propertyName = "")
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(updater);

        if (EqualityComparer<TValue>.Default.Equals(old, value))
            return false;

        OnPropertyChanging(propertyName);

        updater(propertyName, model, value);

        OnPropertyChanged(propertyName);

        PropagatePropertyChangedOnDependents(propertyName);

        return true;
    }

    /// <summary>
    /// Checks if the property does not match the old one.
    /// If so, updates the property using the updater and notifies listeners.
    /// </summary>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <typeparam name="TProperty">Type of the property selector.</typeparam>
    /// <param name="old">The old value of the property (the old).</param>
    /// <param name="value">The new value of the property (the value).</param>
    /// <param name="model">The model instance.</param>
    /// <param name="updater">The action that will update the property.</param>
    /// <param name="selector">The expression delegate to retrieve the property name.</param>
    /// <returns><see langword="true"/>if the value was changed, <see langword="false"/>
    /// if the existing value matches the desired value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="selector"/> is null.</exception>
    protected bool SetProperty<TValue, TProperty>(
        TValue old,
        TValue value,
        TModel model,
        Action<string, TModel, TValue> updater,
        Expression<Func<TModel, TProperty>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(updater);

        string propertyName = selector.GetMemberNameFromExpression();

        if (EqualityComparer<TValue>.Default.Equals(old, value))
            return false;

        OnPropertyChanging(propertyName);

        updater(propertyName, model, value);

        OnPropertyChanged(propertyName);

        PropagatePropertyChangedOnDependents(propertyName);

        return true;
    }

    /// <summary>
    /// Checks if the property does not match the old one.
    /// If so, updates the property using the updater and notifies listeners.
    /// </summary>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <param name="old">The old value of the property (the old).</param>
    /// <param name="value">The new value of the property (the value).</param>
    /// <param name="model">The model instance.</param>
    /// <param name="updater">The action that will update the property.</param>
    /// <param name="comparer">The instance comparer used for values comparison.</param>
    /// <param name="propertyName">The name of the property. Optional (Already known at compile time).</param>
    /// <returns><see langword="true"/>if the value was changed, <see langword="false"/>
    /// if the existing value matches the desired value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="propertyName"/> is null or empty.</exception>
    protected bool SetProperty<TValue>(
        TValue old,
        TValue value,
        TModel model,
        Action<string, TModel, TValue> updater,
        IEqualityComparer<TValue> comparer,
        [CallerMemberName] string propertyName = "")
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(updater);
        ArgumentNullException.ThrowIfNull(comparer);

        if (comparer.Equals(old, value))
            return false;

        OnPropertyChanging(propertyName);

        updater(propertyName, model, value);

        OnPropertyChanged(propertyName);

        PropagatePropertyChangedOnDependents(propertyName);

        return true;
    }

    /// <summary>
    /// Checks if the property does not match the old one.
    /// If so, updates the property using the updater and notifies listeners.
    /// </summary>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <typeparam name="TProperty">Type of the property selector.</typeparam>
    /// <param name="old">The old value of the property (the old).</param>
    /// <param name="value">The new value of the property (the value).</param>
    /// <param name="model">The model instance.</param>
    /// <param name="updater">The action that will update the property.</param>
    /// <param name="comparer">The instance comparer used for values comparison.</param>
    /// <param name="selector">The expression delegate to retrieve the property name.</param>
    /// <returns><see langword="true"/>if the value was changed, <see langword="false"/>
    /// if the existing value matches the desired value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="selector"/> is null.</exception>
    protected bool SetProperty<TValue, TProperty>(
        TValue old,
        TValue value,
        TModel model,
        Action<string, TModel, TValue> updater,
        IEqualityComparer<TValue> comparer,
         Expression<Func<TModel, TProperty>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(updater);
        ArgumentNullException.ThrowIfNull(comparer);

        string propertyName = selector.GetMemberNameFromExpression();

        if (comparer.Equals(old, value))
            return false;

        OnPropertyChanging(propertyName);

        updater(propertyName, model, value);

        OnPropertyChanged(propertyName);

        PropagatePropertyChangedOnDependents(propertyName);

        return true;
    }
}

/// <summary>
/// Provides with extensions method for <see cref="NotifyPropertyChanged"/>.
/// </summary>
public static class NotifyPropertyExtensions
{
    /// <summary>
    /// Provides with the collection of dependencies found in the specified type, to be used with <see cref="NotifyPropertyChanged"/> messages.
    /// </summary>
    /// <param name="target">The type that derived from <see cref="NotifyPropertyChanged"/>.</param>
    internal static IDictionary<string, List<string>> DependencyPropertiesProvider(this Type target)
    {
        var dependencies = new Dictionary<string, List<string>>();

        var properties = (from p in target.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                          where p.GetCustomAttributes<NotifyPropertyChangedForAttribute>(true).Any()
                          select p)
                        .ToArray();

        foreach (var property in properties)
        {
            var attributes = (from a in property.GetCustomAttributes<NotifyPropertyChangedForAttribute>(true) select a.Name).ToArray();

            foreach (var dependency in attributes)
            {
                if (property.Name == dependency)
                {
                    throw new InvalidOperationException("Circular dependency found.",
                        new ArgumentException($"Property {dependency} of {target.Name} can not depends on itself."));
                }

                if (dependencies.TryGetValue(dependency, out var notifiers))
                {
                    var predicateProperty = new Predicate<string>(PredicateFindProperty);
                    if (notifiers.Find(predicateProperty) is { })
                    {
                        throw new InvalidOperationException("Duplicate dependency found.",
                            new ArgumentException($"The property {property.Name} has already a dependency on {dependency}"));
                    }

                    notifiers.Add(property.Name);
                }
                else
                {
                    var predicateFind = new Predicate<string>(PredicateFindDependency);
                    if (dependencies.TryGetValue(property.Name, out var propertyNotifiers) && propertyNotifiers.Find(predicateFind) != null)
                    {
                        throw new InvalidOperationException("Circular dependency found.",
                            new ArgumentException($"The {property.Name} owns a dependency on {dependency} which one depends on {property.Name}."));
                    }

                    dependencies.Add(dependency, [property.Name]);
                }

                bool PredicateFindDependency(string value) => value == dependency;

                bool PredicateFindProperty(string value) => value == property.Name;
            }
        }
        return dependencies;
    }

    internal static bool SetProperty<TValue>(
        ref TValue storage,
        TValue value,
        string propertyName,
        Action<string> onPropertyChanged,
        IDictionary<string, List<string>> dependencies)
    {
        if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullException(nameof(propertyName));

        if (EqualityComparer<TValue>.Default.Equals(storage, value))
            return false;

        storage = value;

        onPropertyChanged(propertyName);

        PropagatePropertyChangedOnDependents(propertyName);

        return true;

        void PropagatePropertyChangedOnDependents(string property)
        {
            var onPropertyChangedAction = new Action<string>(onPropertyChanged);

            (from keyValues in dependencies
             from dependent in keyValues.Value
             where keyValues.Key.Equals(property, StringComparison.OrdinalIgnoreCase)
             select dependent)
             .ToList()
             .ForEach(onPropertyChangedAction);
        }
    }
}