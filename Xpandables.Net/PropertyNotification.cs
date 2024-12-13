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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Xpandables.Net;

/// <summary>
/// When used with <see cref="PropertyNotification"/> 
/// or <see cref="PropertyNotification{T}"/>, 
/// makes sure that the decorated property will be notified
/// when the target specified property by <see cref="Name"/> has changed.
/// </summary>
/// <remarks>
/// Specifies that the decorated property will be notified 
/// when the target specified by name has changed.
/// We advise the use of <see langword="nameof(propertyName)"/> as value.
/// </remarks>
/// <param name="name">The name of the target property 
/// which changes are notified to the decorated property.</param>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="name"/> is null.</exception>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class PropertyNotificationForAttribute(string name) : Attribute
{
    /// <summary>
    /// Gets the name of the target property which 
    /// changes are notified to the decorated property.
    /// </summary>
    public string Name { get; } = name;
}

/// <summary>
/// Provides an abstract base class for property notification, implementing 
/// <see cref = "INotifyPropertyChanged" /> and 
/// <see cref="INotifyPropertyChanging"/>.
/// </summary>
/// <remarks>This class is disposable.</remarks>
public abstract class PropertyNotification :
    INotifyPropertyChanged, INotifyPropertyChanging, IDisposable
{
    private static Func<Type, IDictionary<string, List<string>>> _dependencyPropertiesProvider =
        NotifyPropertyExtensions.DependencyPropertiesProvider;

    /// <summary>
    /// Gets or sets the function that provides the dependency properties.
    /// </summary>
    /// <exception cref="ArgumentNullException">The value is null.</exception>
    public static Func<Type, IDictionary<string, List<string>>> DependencyPropertiesProvider
    {
        get => _dependencyPropertiesProvider;
        set => _dependencyPropertiesProvider = value
            ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Contains a collection of dependencies on property changed messages.
    /// </summary>
    protected IDictionary<string, List<string>> Dependencies { get; }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    /// Notifies listeners that a property value has changed.
    /// </summary>
    /// <param name="e">The event that contains the property name.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="e"/> is <see langword="null"/></exception>
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
    /// This value is optional and can be provided 
    /// automatically when invoked from compilers
    /// that support <see cref="CallerMemberNameAttribute" />.
    /// </param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
        OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Notifies listeners that a property value is changing.
    /// </summary>
    /// <param name="e">The event that contains the property name.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="e"/> is <see langword="null"/></exception>
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
    /// This value is optional and can be provided automatically
    /// when invoked from compilers
    /// that support <see cref="CallerMemberNameAttribute" />.
    /// </param>
    protected void OnPropertyChanging([CallerMemberName] string propertyName = "") =>
        OnPropertyChanging(new PropertyChangingEventArgs(propertyName));

    /// <summary>
    /// Initializes a new instance of 
    /// <see cref="PropertyNotification"/> class and its <see cref="Dependencies"/>.
    /// </summary>
    protected PropertyNotification() =>
        Dependencies = DependencyPropertiesProvider.Invoke(GetType());

    /// <summary>
    /// Checks if the property does not match the old one.
    /// If so, sets the property and notifies listeners.
    /// </summary>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <param name="field">The field of 
    /// the property (the back-end field).</param>
    /// <param name="value">The new value of the property (the value).</param>
    /// <param name="propertyName">The name of the property. 
    /// Optional (Already known at compile time).</param>
    /// <returns><see langword="true"/>if the value was changed, <see langword="false"/>
    /// if the existing value matches the desired value.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="propertyName"/> is null or empty.</exception>
    protected bool SetProperty<TValue>(
        [NotNullIfNotNull(nameof(value))] ref TValue field,
        TValue value,
        [CallerMemberName] string propertyName = "")
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);

        if (EqualityComparer<TValue>.Default.Equals(field, value))
        {
            return false;
        }

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
    /// <param name="field">The field of 
    /// the property (the back-end field).</param>
    /// <param name="value">The new value of the property (the value).</param>
    /// <param name="onChanged">The delegate to be executed 
    /// if the value changed.</param>
    /// <param name="propertyName">The name of the property. 
    /// Optional (Already known at compile time).</param>
    /// <returns><see langword="true"/>if the value was changed, <see langword="false"/>
    /// if the existing value matches the desired value.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="propertyName"/> is null or empty.</exception>
    protected bool SetProperty<TValue>(
        [NotNullIfNotNull(nameof(value))] ref TValue field,
        TValue value,
        Action onChanged,
        [CallerMemberName] string propertyName = "")
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentNullException.ThrowIfNull(onChanged);

        if (EqualityComparer<TValue>.Default.Equals(field, value))
        {
            return false;
        }

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
    /// <param name="field">The field of the property 
    /// (the back-end field).</param>
    /// <param name="value">The new value of the property (the value).</param>
    /// <param name="onChanged">The delegate to be executed 
    /// if the value changed.</param>
    /// <param name="propertyName">The name of the property. 
    /// Optional (Already known at compile time).</param>
    /// <returns><see langword="true"/>if the value was 
    /// changed, <see langword="false"/>
    /// if the existing value matches the desired value.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="propertyName"/> is null or empty.</exception>
    protected bool SetProperty<TValue>(
        [NotNullIfNotNull(nameof(value))] ref TValue field,
        TValue value,
        Action<TValue, TValue> onChanged,
        [CallerMemberName] string propertyName = "")
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentNullException.ThrowIfNull(onChanged);

        if (EqualityComparer<TValue>.Default.Equals(field, value))
        {
            return false;
        }

        OnPropertyChanging(propertyName);

        TValue oldValue = field;
        field = value;

        onChanged.Invoke(oldValue, value);

        OnPropertyChanged(propertyName);

        PropagatePropertyChangedOnDependents(propertyName);

        return true;
    }

    /// <summary>
    /// Checks if the property does not match the old one.
    /// If so, sets the property and notifies listeners.
    /// </summary>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <param name="field">The field of the property 
    /// (the back-end field).</param>
    /// <param name="value">The new value of the property (the value).</param>
    /// <param name="comparer">The instance comparer used 
    /// for values comparison.</param>
    /// <param name="propertyName">The name of the property. 
    /// Optional (Already known at compile time).</param>
    /// <returns><see langword="true"/>if the value
    /// was changed, <see langword="false"/>
    /// if the existing value matches the desired value.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="propertyName"/> is null or empty.</exception>
    protected bool SetProperty<TValue>(
        [NotNullIfNotNull(nameof(value))] ref TValue field,
        TValue value,
        IEqualityComparer<TValue> comparer,
        [CallerMemberName] string propertyName = "")
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentNullException.ThrowIfNull(comparer);

        if (comparer.Equals(field, value))
        {
            return false;
        }

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
    /// <param name="field">The field of the property 
    /// (the back-end field).</param>
    /// <param name="value">The new value of the property (the value).</param>
    /// <param name="comparer">The instance comparer used 
    /// for values comparison.</param>
    /// <param name="onChanged">The delegate to be executed 
    /// if the value changed.</param>
    /// <param name="propertyName">The name of the property. 
    /// Optional (Already known at compile time).</param>
    /// <returns><see langword="true"/>if the value
    /// was changed, <see langword="false"/>
    /// if the existing value matches the desired value.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="propertyName"/> is null or empty.</exception>
    protected bool SetProperty<TValue>(
        [NotNullIfNotNull(nameof(value))] ref TValue field,
        TValue value,
        IEqualityComparer<TValue> comparer,
        Action onChanged,
        [CallerMemberName] string propertyName = "")
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentNullException.ThrowIfNull(comparer);
        ArgumentNullException.ThrowIfNull(onChanged);

        if (comparer.Equals(field, value))
        {
            return false;
        }

        OnPropertyChanging(propertyName);

        field = value;

        onChanged.Invoke();

        OnPropertyChanged(propertyName);

        PropagatePropertyChangedOnDependents(propertyName);

        return true;
    }

    /// <summary>
    /// Checks if the property does not match the old one.
    /// If so, sets the property and notifies listeners.
    /// </summary>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <param name="field">The field of the property 
    /// (the back-end field).</param>
    /// <param name="value">The new value of the property (the value).</param>
    /// <param name="comparer">The instance comparer used 
    /// for values comparison.</param>
    /// <param name="onChanged">The delegate to be executed 
    /// if the value changed.</param>
    /// <param name="propertyName">The name of the property. 
    /// Optional (Already known at compile time).</param>
    /// <returns><see langword="true"/>if the value was 
    /// changed, <see langword="false"/>
    /// if the existing value matches the desired value.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="propertyName"/> is null or empty.</exception>
    protected bool SetProperty<TValue>(
        [NotNullIfNotNull(nameof(value))] ref TValue field,
        TValue value,
        IEqualityComparer<TValue> comparer,
        Action<TValue, TValue> onChanged,
        [CallerMemberName] string propertyName = "")
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentNullException.ThrowIfNull(comparer);
        ArgumentNullException.ThrowIfNull(onChanged);

        if (comparer.Equals(field, value))
        {
            return false;
        }

        OnPropertyChanging(propertyName);

        TValue oldValue = field;
        field = value;

        onChanged.Invoke(oldValue, value);

        OnPropertyChanged(propertyName);

        PropagatePropertyChangedOnDependents(propertyName);

        return true;
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is disposed.
    /// </summary>
    /// <value>
    ///  <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>Default initialization for 
    /// a <see cref="bool"/> is <c>false</c>.</remarks>
    protected bool IsDisposed { get; set; }

    /// <summary>
    /// Public Implementation of Dispose according to .NET Framework 
    /// Design Guidelines
    /// callable by consumers.
    /// Do not make this method virtual.
    /// A derived class should not be able to override this method.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This object will be cleaned up by the Dispose method.
    /// Therefore, you should call GC.SuppressFinalize to take 
    /// this object off the finalization queue
    /// and prevent finalization code for this object 
    /// from executing a second time.
    /// </para>
    /// <para>Always use SuppressFinalize() in case 
    /// a subclass of this type implements a finalizer.</para>
    /// </remarks>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected implementation of Dispose pattern.
    /// When overridden in derived classes, this method get 
    /// called when the instance will be disposed.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to 
    /// release both managed and unmanaged resources;
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    /// <remarks>
    /// <list type="bulleted">
    /// <see cref="Dispose(bool)"/> executes in two distinct scenarios.
    /// <item>If <paramref name="disposing"/> equals <c>true</c>, 
    /// the method has been called directly
    /// or indirectly by a user's code. Managed and unmanaged 
    /// resources can be disposed.</item>
    /// <item>If <paramref name="disposing"/> equals <c>false</c>, 
    /// the method has been called
    /// by the runtime from inside the finalizer and you should 
    /// not reference other objects.
    /// Only unmanaged resources can be disposed.</item></list>
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            // Release all managed resources here
            // Need to unregister/detach yourself from the events.
            // Always make sure the object is not null first before trying to
            // unregister/detach them!
            // Failure to unregister can be a BIG source of memory leaks
        }

        // Release all unmanaged resources here and override a finalizer below.
        // Set large fields to null.

        // Dispose has been called.
        IsDisposed = true;

        // If it is available, make the call to the
        // base class's Dispose(boolean) method
    }

    internal void PropagatePropertyChangedOnDependents(string property)
    {
        Action<string> onPropertyChangedAction = new(OnPropertyChanged);

        (from keyValues in Dependencies
         from dependent in keyValues.Value
         where keyValues.Key.Equals(property, StringComparison.OrdinalIgnoreCase)
         select dependent)
         .ToList()
         .ForEach(onPropertyChangedAction);
    }
}

/// <summary>
/// Implementation for <see cref="INotifyPropertyChanged"/> 
/// and <see cref="INotifyPropertyChanging"/>.
/// You can combine the use with 
/// <see cref="PropertyNotificationForAttribute"/> to propagate notification.
/// </summary>
/// <typeparam name="TModel">The type of the target model.</typeparam>
public abstract class PropertyNotification<TModel> : PropertyNotification
    where TModel : class
{
    /// <summary>
    /// Initializes a new instance of 
    /// <see cref="PropertyNotification{T}"/> class and its dependencies.
    /// </summary>
    protected PropertyNotification() { }

    /// <summary>
    /// Checks if the property does not match the old one.
    /// If so, sets the property and notifies listeners.
    /// </summary>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <typeparam name="TProperty">Type of the property selector.</typeparam>
    /// <param name="field">The field of the property (the back-end field).</param>
    /// <param name="value">The new value of the property (the value).</param>
    /// <param name="selector">The expression delegate 
    /// to retrieve the property name.</param>
    /// <returns><see langword="true"/>if the value 
    /// was changed, <see langword="false"/>
    /// if the existing value matches the desired value.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="selector"/> is null.</exception>
    protected bool SetProperty<TValue, TProperty>(
        [NotNullIfNotNull(nameof(value))] ref TValue field,
        TValue value,
        Expression<Func<TModel, TProperty>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);

        string propertyName = selector.GetMemberNameFromExpression();

        if (EqualityComparer<TValue>.Default.Equals(field, value))
        {
            return false;
        }

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
    /// <param name="field">The field of the property 
    /// (the back-end field).</param>
    /// <param name="value">The new value of the property (the value).</param>
    /// <param name="comparer">The instance comparer used 
    /// for values comparison.</param>
    /// <param name="selector">The expression delegate 
    /// to retrieve the property name.</param>
    /// <returns><see langword="true"/>if the value 
    /// was changed, <see langword="false"/>
    /// if the existing value matches the desired value.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="selector"/> is null.</exception>
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
        {
            return false;
        }

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
    /// <param name="propertyName">The name of the property. 
    /// Optional (Already known at compile time).</param>
    /// <returns><see langword="true"/>if the value was changed, 
    /// <see langword="false"/>
    /// if the existing value matches the desired value.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="propertyName"/> is null or empty.</exception>
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
        {
            return false;
        }

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
    /// <param name="selector">The expression delegate 
    /// to retrieve the property name.</param>
    /// <returns><see langword="true"/>if the value was changed, 
    /// <see langword="false"/>
    /// if the existing value matches the desired value.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="selector"/> is null.</exception>
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
        {
            return false;
        }

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
    /// <param name="comparer">The instance comparer used 
    /// for values comparison.</param>
    /// <param name="propertyName">The name of the property. 
    /// Optional (Already known at compile time).</param>
    /// <returns><see langword="true"/>if the value was changed, 
    /// <see langword="false"/>
    /// if the existing value matches the desired value.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="propertyName"/> is null or empty.</exception>
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
        {
            return false;
        }

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
    /// <param name="comparer">The instance comparer used 
    /// for values comparison.</param>
    /// <param name="selector">The expression delegate to 
    /// retrieve the property name.</param>
    /// <returns><see langword="true"/>if the value was changed, 
    /// <see langword="false"/>
    /// if the existing value matches the desired value.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="selector"/> is null.</exception>
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
        {
            return false;
        }

        OnPropertyChanging(propertyName);

        updater(propertyName, model, value);

        OnPropertyChanged(propertyName);

        PropagatePropertyChangedOnDependents(propertyName);

        return true;
    }
}

internal static class NotifyPropertyExtensions
{
    internal static string GetMemberNameFromExpression<T, TProperty>(
          this Expression<Func<T, TProperty>> propertyExpression)
          where T : class
    {
        _ = propertyExpression
            ?? throw new ArgumentNullException(nameof(propertyExpression));

        return (propertyExpression.Body as MemberExpression
            ?? ((UnaryExpression)propertyExpression.Body).Operand as MemberExpression)
            ?.Member.Name ??
            throw new ArgumentException("A member expression is expected.");
    }

    internal static IDictionary<string, List<string>>
        DependencyPropertiesProvider(this Type target)
    {
        Dictionary<string, List<string>> dependencies = [];

        PropertyInfo[] properties
            = [.. (from p in target
               .GetProperties(BindingFlags.Instance | BindingFlags.Public)
               where p
                .GetCustomAttributes<PropertyNotificationForAttribute>(true)
                .Any()
               select p)];

        foreach (ref PropertyInfo property in properties.AsSpan())
        {
            string[] attributes
                = [.. (from a in property
                        .GetCustomAttributes<PropertyNotificationForAttribute>(false)
                   select a.Name)];

            foreach (string? dependency in attributes)
            {
                if (property.Name == dependency)
                {
                    throw new InvalidOperationException(
                        "Circular dependency found.",
                        new ArgumentException(
                            $"Property {dependency} of {target.Name} can " +
                            $"not depends on itself."));
                }

                if (dependencies.TryGetValue(
                    dependency,
                    out List<string>? notifiers))
                {
                    string propertyName = property.Name;

                    Predicate<string> predicateProperty
                        = new(v => PredicateFindProperty(v, propertyName));
                    if (notifiers.Find(predicateProperty) is { })
                    {
                        throw new InvalidOperationException(
                            "Duplicate dependency found.",
                            new ArgumentException($"The property {propertyName} " +
                            $"has already a dependency on {dependency}"));
                    }

                    notifiers.Add(property.Name);
                }
                else
                {
                    Predicate<string> predicateFind
                        = new(v => PredicateFindDependency(v, dependency));
                    if (dependencies.TryGetValue(
                        property.Name,
                        out List<string>? propertyNotifiers)
                        && propertyNotifiers.Find(predicateFind) != null)
                    {
                        throw new InvalidOperationException(
                            "Circular dependency found.",
                            new ArgumentException($"The {property.Name} owns " +
                            $"a dependency on {dependency} which one " +
                            $"depends on {property.Name}."));
                    }

                    dependencies.Add(dependency, [property.Name]);
                }

                static bool PredicateFindDependency(
                    string value, string dependencyName) =>
                    value == dependencyName;

                static bool PredicateFindProperty(
                    string value, string propertyName) =>
                    value == propertyName;
            }
        }

        return dependencies;
    }
}