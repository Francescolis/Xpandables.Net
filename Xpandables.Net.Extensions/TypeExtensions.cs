
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
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;

using Xpandables.Net.Primitives.Collections;

namespace Xpandables.Net.Extensions;

/// <summary>
/// Provides with methods to extend use of <see cref="Type"/>.
/// </summary>
public static partial class XpandablesExtensions
{
    /// <summary>
    /// Returns the name of the type without the generic arity '`'.
    /// Useful for generic types.
    /// </summary>
    /// <param name="type">The type to act on.</param>
    /// <returns>The name of the type without the generic arity '`'.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is null.</exception>
    public static string GetNameWithoutGenericArity(this Type type)
    {
        _ = type ?? throw new ArgumentNullException(nameof(type));

        int index = type.GetTypeInfo().Name.IndexOf('`', StringComparison.OrdinalIgnoreCase);
        return index == -1 ? type.GetTypeInfo().Name : type.GetTypeInfo().Name[..index];
    }

    /// <summary>
    /// Determines whether the current type is a struct or not.
    /// </summary>
    /// <param name="type">The object type to  check.</param>
    /// <returns><see langword="true"/> if so, otherwise <see langword="false"/>.</returns>
    public static bool IsStruct(this Type type)
    {
        _ = type ?? throw new ArgumentNullException(nameof(type));
        return type.IsValueType && !type.IsPrimitive && !type.IsEnum;
    }

    /// <summary>
    /// Returns the description string attribute of the current <see cref="Enum"/> value type.
    /// if not found, returns the value as string.
    /// </summary>
    /// <typeparam name="TEnum">Type of enumeration.</typeparam>
    /// <param name="value">Enumeration field value to act on.</param>
    /// <returns>The description string. If not found, returns the value as string.</returns>
    public static string GetDescription<TEnum>(this TEnum value)
        where TEnum : Enum
        => typeof(TEnum).GetField($"{value}")
            ?.GetCustomAttribute<DescriptionAttribute>()
            ?.Description ?? $"{value}";

    /// <summary>
    /// Determines whether the specified method is overridden in its current implementation.
    /// The method info should come from the <see cref="object.GetType()"/>.
    /// </summary>
    /// <param name="methodInfo">The method info to act on.</param>
    /// <returns><see langword="true"/> if so, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="methodInfo"/> is null.</exception>
    public static bool IsOverridden(this MethodInfo methodInfo)
    {
        _ = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
        return methodInfo.GetBaseDefinition().DeclaringType != methodInfo.DeclaringType;
    }

    /// <summary>
    /// Determines whether the specified method is a <see cref="Task"/>.
    /// </summary>
    /// <param name="methodInfo">The method info to act on.</param>
    /// <returns><see langword="true"/> if so, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="methodInfo"/> is null.</exception>
    public static bool IsAwaitable(this MethodInfo methodInfo)
    {
        _ = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
        return methodInfo.ReturnType.GetMethod(nameof(Task.GetAwaiter)) is not null;
    }

    /// <summary>
    /// Determines whether the current type is a null-able type.
    /// </summary>
    /// <param name="type">The type to act on.</param>
    /// <returns><see langword="true"/> if found, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is null.</exception>
    public static bool IsNullable(this Type type)
    {
        _ = type ?? throw new ArgumentNullException(nameof(type));
        return Nullable.GetUnderlyingType(type) is not null;
    }

    /// <summary>
    /// Determines whether the current type implements <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="type">The type to act on.</param>
    /// <returns><see langword="true"/> if found, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is null.</exception>
    public static bool IsEnumerable(this Type type)
    {
        _ = type ?? throw new ArgumentNullException(nameof(type));

        return !type.IsPrimitive
            && type != typeof(string)
            && type.GetInterfaces().Exists(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    }

    /// <summary>
    /// Determines whether the current type implements or it's <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <param name="type">The type to act on.</param>
    /// <returns><see langword="true"/> if found, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is null.</exception>
    public static bool IsAsyncEnumerable(this Type type)
    {
        _ = type ?? throw new ArgumentNullException(nameof(type));

        return type.IsInterface switch
        {
            true => type.IsGenericType
                && type.Name.Equals(typeof(IAsyncEnumerable<>).Name, StringComparison.OrdinalIgnoreCase),
            _ => type.GetInterfaces()
                .Exists(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>))
        };
    }

    /// <summary>
    /// Tries to load assembly from its assembly name.
    /// </summary>
    /// <param name="assemblyName">The assembly name to act with.</param>
    /// <param name="loadedAssembly">The loaded assembly if succeeded.</param>
    /// <param name="assemblyException">The handled exception during assembly loading if fails.</param>
    /// <returns>Returns <see langword="true"/> if loading OK and <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="assemblyName"/> is null.</exception>
    public static bool TryLoadAssembly(
        this AssemblyName assemblyName,
        [MaybeNullWhen(returnValue: false)] out Assembly loadedAssembly,
        [MaybeNullWhen(returnValue: true)] out Exception assemblyException)
    {
        _ = assemblyName ?? throw new ArgumentNullException(nameof(assemblyName));

        try
        {
            assemblyException = default;
            loadedAssembly = Assembly.Load(assemblyName);
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException
                                        || exception is FileNotFoundException
                                        || exception is FileLoadException
                                        || exception is BadImageFormatException)
        {
            loadedAssembly = default;
            assemblyException = exception;
            return false;
        }
    }

    /// <summary>
    /// Tries to load assembly from its name.
    /// </summary>
    /// <param name="assemblyName">The full assembly name.</param>
    /// <param name="loadedAssembly">The loaded assembly if succeeded.</param>
    /// <param name="assemblyException">The handled exception during assembly loading.</param>
    /// <returns>Returns <see langword="true"/> if loading OK and <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="assemblyName"/> is null.</exception>
    public static bool TryLoadAssembly(
        this string assemblyName,
        [MaybeNullWhen(returnValue: false)] out Assembly loadedAssembly,
        [MaybeNullWhen(returnValue: true)] out Exception assemblyException)
    {
        _ = assemblyName ?? throw new ArgumentNullException(nameof(assemblyName));

        try
        {
            assemblyException = default;
            loadedAssembly = Assembly.Load(assemblyName);
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException
                                        || exception is FileNotFoundException
                                        || exception is FileLoadException
                                        || exception is BadImageFormatException
                                        || exception is PathTooLongException
                                        || exception is System.Security.SecurityException)
        {
            loadedAssembly = default;
            assemblyException = exception;
            return false;
        }
    }

    /// <summary>
    /// Tries to invoke the specified member, using the specified binding constraints and matching
    /// the specified argument list.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="result">An object representing the return value of the invoked member
    /// or an empty result with handled exception.</param>
    /// <param name="invokeException">The handled invoke exception.</param>
    /// <param name="memberName">The string containing the name of the constructor, method, property, or field
    /// member to invoke. /// -or- /// An empty string (&quot;&quot;) to invoke the default
    /// member. /// -or- /// For IDispatch members, a string representing the DispID,
    /// for example &quot;[DispID=3]&quot;.</param>
    /// <param name="invokeAttr">A bit-mask comprised of one or more System.Reflection.BindingFlags that specify
    /// how the search is conducted. The access can be one of the BindingFlags such as
    /// Public, NonPublic, Private, InvokeMethod, GetField, and so on. The type of lookup
    /// need not be specified. If the type of lookup is omitted, BindingFlags.Public
    /// | BindingFlags.Instance | BindingFlags.Static are used.</param>
    /// <param name="binder">An object that defines a set of properties and enables binding, which can involve
    /// selection of an overloaded method, coercion of argument types, and invocation
    /// of a member through reflection. /// -or- /// A null reference (Nothing in Visual
    /// Basic), to use the System.Type.DefaultBinder. Note that explicitly defining a
    /// System.Reflection.Binder object may be required for successfully invoking method
    /// overloads with variable arguments.</param>
    /// <param name="target">The object on which to invoke the specified member, if available.</param>
    /// <param name="args">An array containing the arguments to pass to the member to invoke.</param>
    /// <returns>Returns <see langword="true"/> if invoke OK and <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="memberName"/> is null.</exception>
    public static bool TryTypeInvokeMember(
        this Type type,
        [MaybeNull] out object result,
        [MaybeNullWhen(returnValue: true)] out Exception invokeException,
        string memberName,
        BindingFlags invokeAttr,
        Binder? binder,
        object? target,
        object[]? args)
    {
        _ = type ?? throw new ArgumentNullException(nameof(type));
        _ = memberName ?? throw new ArgumentNullException(nameof(memberName));

        try
        {
            invokeException = default;
            result = type.InvokeMember(memberName, invokeAttr, binder, target, args, CultureInfo.InvariantCulture);

            return true;
        }
        catch (Exception exception) when (exception is ArgumentNullException
                                        || exception is ArgumentException
                                        || exception is MethodAccessException
                                        || exception is MissingFieldException
                                        || exception is MissingMethodException
                                        || exception is TargetException
                                        || exception is AmbiguousMatchException
                                        || exception is InvalidOperationException)
        {
            result = default;
            invokeException = exception;
            return false;
        }
    }

    /// <summary>
    /// Tries to substitute the elements of an array of types for the type parameters of the
    /// current generic type definition and returns a System.Type object representing
    /// the resulting constructed type. If error, return false with exception.
    /// </summary>
    /// <param name="type">The type to act on.</param>
    /// <param name="genericType">The generic type result.</param>
    /// <param name="typeException">The handled type exception.</param>
    /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic
    /// type.</param>
    /// <returns>Returns <see langword="true"/> if make OK and <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is null.</exception>
    public static bool TryMakeGenericType(
        this Type type,
        [MaybeNullWhen(returnValue: false)] out Type genericType,
        [MaybeNullWhen(returnValue: true)] out Exception typeException,
        params Type[] typeArguments)
    {
        _ = type ?? throw new ArgumentNullException(nameof(type));

        try
        {
            typeException = default;
            genericType = type.MakeGenericType(typeArguments);
            return true;
        }
        catch (Exception exception) when (exception is InvalidOperationException
                                            || exception is ArgumentException
                                            || exception is NotSupportedException)
        {
            typeException = exception;
            genericType = default;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the specified delegate type associated to the constructor.
    /// </summary>
    /// <typeparam name="TDelegate"></typeparam>
    /// <param name="type">The type to act on.</param>
    /// <param name="constructorDelegate">The built constructor delegate.</param>
    /// <param name="constructorException">The handled exception.</param>
    /// <param name="parameterTypes">The collection of parameter types.</param>
    /// <returns>Returns <see langword="true"/> if make OK and <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="parameterTypes"/> is null.</exception>
    public static bool TryGetConstructorDelegate<TDelegate>(
        this Type type,
        [MaybeNullWhen(returnValue: false)] out TDelegate constructorDelegate,
        [MaybeNullWhen(returnValue: true)] out Exception constructorException,
        params Type[] parameterTypes)
        where TDelegate : Delegate
    {
        _ = type ?? throw new ArgumentNullException(nameof(type));
        _ = parameterTypes ?? throw new ArgumentNullException(nameof(parameterTypes));

        if (parameterTypes.Length == 0)
        {
            try
            {
                var body = Expression.New(type);
                constructorDelegate = Expression.Lambda<TDelegate>(body).Compile();
                constructorException = default;
                return true;
            }
            catch (Exception ex) when (ex is ArgumentException)
            {
                constructorException = ex;
                constructorDelegate = default;
                return false;
            }
        }

        if (!type.TryGetConstructorInfo(out var constructor, out constructorException, parameterTypes))
        {
            constructorDelegate = default;
            return false;
        }

        var parameterExpressions = GetParameterExpression(parameterTypes);

        if (!constructor.TryGetConstructorExpression(out var constructorExpression, out constructorException, parameterExpressions))
        {
            constructorDelegate = default!;
            return false;
        }

        try
        {
            constructorDelegate = Expression
                .Lambda<TDelegate>(constructorExpression, parameterExpressions)
                .Compile();
            constructorException = default!;
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException || exception is ArgumentNullException)
        {
            constructorDelegate = default!;
            constructorException = exception;
            return false;
        }
    }

    /// <summary>
    /// Tries to get an Expression representing the constructor call, passing in the constructor parameters.
    /// </summary>
    /// <param name="constructorInfo">The constructor info to act on.</param>
    /// <param name="constructorExpression">The built constructor expression.</param>
    /// <param name="constructorException">The handled constructor exception.</param>
    /// <param name="parameterExpressions">A collection of parameter expressions.</param>
    /// <returns>Returns <see langword="true"/> if OK and <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="constructorInfo"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="parameterExpressions"/> is null.</exception>
    public static bool TryGetConstructorExpression(
        this ConstructorInfo constructorInfo,
        [MaybeNullWhen(returnValue: false)] out Expression constructorExpression,
        [MaybeNullWhen(returnValue: true)] out Exception constructorException,
        params Expression[] parameterExpressions)
    {
        _ = constructorInfo ?? throw new ArgumentNullException(nameof(constructorInfo));
        _ = parameterExpressions ?? throw new ArgumentNullException(nameof(parameterExpressions));

        try
        {
            constructorException = default;
            constructorExpression = Expression.New(constructorInfo, parameterExpressions);
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException)
        {
            constructorException = exception;
            constructorExpression = default;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the constructor from the type that matches the specified arguments type.
    /// </summary>
    /// <param name="type">The type to act on.</param>
    /// <param name="constructorInfo">The found constructor.</param>
    /// <param name="constructorException">The handled constructor exception.</param>
    /// <param name="parameterTypes">The optional parameters types.</param>
    /// <returns>Returns <see langword="true"/> if make OK and <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is null.</exception>
    public static bool TryGetConstructorInfo(
        this Type type,
        [MaybeNullWhen(returnValue: false)] out ConstructorInfo constructorInfo,
        [MaybeNullWhen(returnValue: true)] out Exception constructorException,
        params Type[] parameterTypes)
    {
        _ = type ?? throw new ArgumentNullException(nameof(type));

        try
        {
            constructorInfo = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                CallingConventions.HasThis,
                parameterTypes,
                []);
            constructorException = default;
            if (constructorInfo is null)
            {
                constructorException = new ArgumentNullException(nameof(constructorInfo), "Constructor not found.");
                return false;
            }
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException)
        {
            constructorException = exception;
            constructorInfo = default;
            return false;
        }
    }

    /// <summary>
    /// Determines whether or not the type contains the specified type attribute.
    /// </summary>
    /// <param name="type">The type to act on.</param>
    /// <param name="attributeType">The type of the attribute.</param>
    /// <returns>Returns <see langword="true"/> if OK and <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="attributeType"/> is null.</exception>
    public static bool HasAttribute(this Type type, Type attributeType)
    {
        _ = type ?? throw new ArgumentNullException(nameof(type));
        _ = attributeType ?? throw new ArgumentNullException(nameof(attributeType));

        return type.GetTypeInfo().IsDefined(attributeType, inherit: true);
    }

    /// <summary>
    /// Determines whether or not the type contains the specified type attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <param name="type">The type to act on.</param>
    /// <param name="predicate">The predicate to use.</param>
    /// <returns>Returns <see langword="true"/> if OK and <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="predicate"/> is null.</exception>
    public static bool HasAttribute<TAttribute>(this Type type, Func<TAttribute, bool> predicate)
        where TAttribute : Attribute
    {
        _ = type ?? throw new ArgumentNullException(nameof(type));
        _ = predicate ?? throw new ArgumentNullException(nameof(predicate));

        return type.GetTypeInfo()
            .GetCustomAttributes<TAttribute>(inherit: true)
            .Any(predicate);
    }

    /// <summary>
    /// Re-throws the current exception using <see cref="ExceptionDispatchInfo.Capture"/> method while preserving stack trace.
    /// </summary>
    /// <param name="exception">The exception to be re-thrown.</param>
    /// <exception cref="ArgumentException">The <paramref name="exception"/> is null.</exception>
    public static void ReThrow(this Exception exception) => ExceptionDispatchInfo.Capture(exception).Throw();

    /// <summary>
    /// Checks whether the specified target name exists in the <see cref="SerializationInfo"/> instance.
    /// </summary>
    /// <param name="this">The current <see cref="SerializationInfo"/> instance to act on.</param>
    /// <param name="target">The string target name to search for.</param>
    /// <returns><see langword="true"/> if found, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="this"/> or <paramref name="target"/> is null.</exception>
    public static bool Exists(this SerializationInfo @this, string target)
    {
        ArgumentNullException.ThrowIfNull(@this);
        ArgumentNullException.ThrowIfNull(target);
        foreach (SerializationEntry entry in @this)
            if (entry.Name.Equals(target, StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }
    /// <summary>
    /// Return a collection of base types found in the specified type.
    /// </summary>
    /// <param name="type">The type to act on.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is null.</exception>
    public static IEnumerable<Type> GetBaseTypes(this Type type)
    {
        _ = type ?? throw new ArgumentNullException(nameof(type));

        TypeInfo typeInfo = type.GetTypeInfo();

        foreach (Type implementedInterface in typeInfo.ImplementedInterfaces)
        {
            yield return implementedInterface;
        }

        Type? baseType = typeInfo.BaseType;

        while (baseType != null)
        {
            TypeInfo baseTypeInfo = baseType.GetTypeInfo();

            yield return baseType;

            baseType = baseTypeInfo.BaseType;
        }
    }

    /// <summary>
    /// Sets properties via lambda expression scope.
    /// This is similar to the VB.Net key word <see lanwgord="With"/>..<see lanwgord="EndWith"/>.
    /// </summary>
    /// <typeparam name="TSource">Type source.</typeparam>
    /// <param name="source">The source item to act on.</param>
    /// <param name="action">The action to be applied.</param>
    /// <returns>The same object after applying the action on it.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="action"/> is null.</exception>
    public static TSource Assign<TSource>(this TSource source, Action<TSource> action)
        where TSource : class
    {
        _ = source ?? throw new ArgumentNullException(nameof(source));
        _ = action ?? throw new ArgumentNullException(nameof(action));

        action.Invoke(source);
        return source;
    }

    /// <summary>
    /// Sets properties via lambda expressions scope.
    /// This is similar to the VB.Net key word <see lanwgord="With"/>..<see lanwgord="EndWith"/>.
    /// </summary>
    /// <typeparam name="TSource">Type source.</typeparam>
    /// <param name="source">The source item to act on.</param>
    /// <param name="actions">The actions to be applied.</param>
    /// <returns>The same object after applying the action on it.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="actions"/> is null.</exception>
    public static TSource Assign<TSource>(this TSource source, params Action<TSource>[] actions)
        where TSource : class
    {
        _ = source ?? throw new ArgumentNullException(nameof(source));
        _ = actions ?? throw new ArgumentNullException(nameof(actions));

        actions.ForEach(action => action.Invoke(source));
        return source;
    }

    /// <summary>
    /// Sets properties via lambda expression. This is useful when dealing with <see cref="Expression{TDelegate}"/>.
    /// </summary>
    /// <typeparam name="TSource">Type source.</typeparam>
    /// <param name="source">The source instance to act on.</param>
    /// <param name="nameOfExpression">The expression delegate for the property.
    /// Just use <see langword="nameof"/> as expression for the delegate.</param>
    /// <param name="value">The value for the property.</param>
    /// <returns>The current instance with modified property.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="nameOfExpression"/> is null.</exception>
    /// <exception cref="ArgumentException">The <paramref name="nameOfExpression"/> is not valid.</exception>
    /// <exception cref="InvalidOperationException">An error occurred while setting the property value. See inner exception.</exception>
    public static TSource Assign<TSource>(
        this TSource source,
        Expression<Func<TSource, string>> nameOfExpression,
        object value)
        where TSource : class
    {
        _ = source ?? throw new ArgumentNullException(nameof(source));
        _ = nameOfExpression ?? throw new ArgumentNullException(nameof(nameOfExpression));

        if (nameOfExpression.Body is not ConstantExpression constantExpression)
            throw new ArgumentNullException(nameof(nameOfExpression), "Constant Expression expected.");

        if (constantExpression.Value?.ToString() is not { } propertyName)
            throw new ArgumentException($"Constant Expression Value is null. {nameof(nameOfExpression)}");

        if (source.GetType().GetProperty(propertyName) is not { } propertyInfo)
            throw new ArgumentException($"Property {propertyName} does not exist in the {source.GetType().Name}.");

        if (propertyInfo.GetSetMethod() is not { })
            throw new ArgumentException($"Property {propertyInfo.Name} is not settable.");

        if (value is not null && !propertyInfo.PropertyType.IsInstanceOfType(value))
            throw new ArgumentException(
                $"Property type '{propertyInfo.PropertyType.Name}' of {propertyName} and type of the value '{value.GetType().Name}' does not match.");

        if (value is null && Nullable.GetUnderlyingType(propertyInfo.PropertyType) is not { })
            throw new ArgumentException(
                $"Unable to assign null to a property type '{propertyInfo.PropertyType.Name}' of {propertyName} that is not nullable.");

        try
        {
            propertyInfo.SetValue(source, value);
        }
        catch (Exception exception) when (exception is ArgumentException
                                          || exception is TargetException
                                          || exception is MethodAccessException
                                          || exception is TargetInvocationException)
        {
            throw new InvalidOperationException(
                $"Unable to set '{typeof(TSource).Name}.{propertyName}' with the specified value : {value}.", exception);
        }

        return source;
    }


    /// <summary>
    /// Assigns the property named <paramref name="propertyName"/> with the value <paramref name="value"/> from the specified object <paramref name="source"/>.
    /// </summary>
    /// <typeparam name="TSource">Type source.</typeparam>
    /// <param name="source">The source instance to act on.</param>
    /// <param name="propertyName">The name of the property</param>
    /// <param name="value">The value for the property.</param>
    /// <returns>The current instance with modified property.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="propertyName"/> is null.</exception>
    /// <exception cref="InvalidOperationException">An error occurred while setting the property value. See inner exception.</exception>
    public static TSource Assign<TSource>(
        this TSource source,
        string propertyName,
        object value)
        where TSource : class
    {
        _ = source ?? throw new ArgumentNullException(nameof(source));
        _ = propertyName ?? throw new ArgumentNullException(nameof(propertyName));

        if (source.GetType().GetProperty(propertyName) is not { } propertyInfo)
            throw new ArgumentException($"Property {propertyName} does not exist in the {source.GetType().Name}.");

        if (propertyInfo.GetSetMethod() is not { })
            throw new ArgumentException($"Property {propertyInfo.Name} is not settable.");

        if (value is not null && !propertyInfo.PropertyType.IsInstanceOfType(value))
            throw new ArgumentException(
                $"Property type '{propertyInfo.PropertyType.Name}' of {propertyName} and type of the value '{value.GetType().Name}' does not match.");

        if (value is null && Nullable.GetUnderlyingType(propertyInfo.PropertyType) is not { })
            throw new ArgumentException(
                $"Unable to assign null to a property type '{propertyInfo.PropertyType.Name}' of {propertyName} that is not nullable.");

        try
        {
            propertyInfo.SetValue(source, value);
        }
        catch (Exception exception) when (exception is ArgumentException
                                          || exception is TargetException
                                          || exception is MethodAccessException
                                          || exception is TargetInvocationException)
        {
            throw new InvalidOperationException(
                $"Unable to set '{typeof(TSource).Name}.{propertyName}' with the specified value : {value}.", exception);
        }

        return source;
    }

    /// <summary>
    /// Sets properties via lambda expression scope.
    /// This is similar to the VB.Net key word <see lanwgord="With"/>..<see lanwgord="EndWith"/>.
    /// </summary>
    /// <typeparam name="TSource">Type source.</typeparam>
    /// <param name="source">The source item to act on.</param>
    /// <param name="action">The action to be applied.</param>
    /// <returns>The same object after applying the action on it.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="action"/> is null.</exception>
    public static async Task<TSource> AssignAsync<TSource>(this TSource source, Func<TSource, Task> action)
        where TSource : class
    {
        _ = source ?? throw new ArgumentNullException(nameof(source));
        _ = action ?? throw new ArgumentNullException(nameof(action));

        await action.Invoke(source).ConfigureAwait(false);
        return source;
    }

    /// <summary>
    /// Tries to get the type specified by its name from the loaded assemblies.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <param name="type">The type result if found.</param>
    /// <param name="typeException">The handled execution exception.</param>
    /// <returns>Returns <see langword="true"/> if get OK and <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="typeName"/> is null.</exception>
    public static bool TryGetTypeFromTypeName(
        this string typeName,
        [MaybeNullWhen(returnValue: false)] out Type type,
        [MaybeNullWhen(returnValue: true)] out Exception typeException)
    {
        _ = typeName ?? throw new ArgumentNullException(nameof(typeName));

        try
        {
            typeException = default;
            type = Type.GetType(typeName, true, true);
            if (type is null)
            {
                typeException = new ArgumentNullException(nameof(type), "Type not found.");
                return false;
            }
            return true;
        }
        catch (Exception exception) when (exception is TargetInvocationException
                                    || exception is TypeLoadException
                                    || exception is ArgumentException
                                    || exception is FileNotFoundException
                                    || exception is FileLoadException
                                    || exception is BadImageFormatException)
        {
            typeException = exception;
            type = default;
            return false;
        }
    }

    /// <summary>
    /// Tries to get type from its string name.
    /// </summary>
    /// <param name="typeName">The name of the type to find.</param>
    /// <param name="foundType">The found type.</param>
    /// <param name="typeException">The handled type exception.</param>
    /// <returns>Returns <see langword="true"/> if OK and <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="typeName"/> is null.</exception>
    public static bool TryGetType(
        this string typeName,
        [MaybeNullWhen(returnValue: false)] out Type foundType,
        [MaybeNullWhen(returnValue: true)] out Exception typeException)
    {
        _ = typeName ?? throw new ArgumentNullException(nameof(typeName));

        try
        {
            typeException = default;
            foundType = Type.GetType(typeName, true, true);
            if (foundType is null)
            {
                typeException = new ArgumentException("Expected type not found.");
                return false;
            }

            return true;
        }
        catch (Exception exception) when (exception is TargetInvocationException
                                        || exception is TypeLoadException
                                        || exception is ArgumentException
                                        || exception is FileNotFoundException
                                        || exception is FileLoadException
                                        || exception is BadImageFormatException)
        {
            typeException = exception;
            foundType = default;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the type from string, if not found, try to load from the assembly.
    /// </summary>
    /// <param name="typeName">The name of the type to find.</param>
    /// <param name="assemblyName">The assembly to act on.</param>
    /// <param name="foundType">The found type.</param>
    /// <param name="typeException">The handled type exception.</param>
    /// <returns>Returns <see langword="true"/> if OK and <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="typeName"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="assemblyName"/> is null.</exception>
    public static bool TryGetType(
        this string typeName,
        string assemblyName,
        [MaybeNullWhen(returnValue: false)] out Type foundType,
        [MaybeNullWhen(returnValue: true)] out Exception typeException)
    {
        _ = typeName ?? throw new ArgumentNullException(nameof(typeName));
        _ = assemblyName ?? throw new ArgumentNullException(nameof(assemblyName));

        if (typeName.TryGetType(out foundType, out typeException))
            return true;

        if (!assemblyName.TryLoadAssembly(out var assembly, out typeException))
        {
            foundType = default;
            return false;
        }

        try
        {
            typeException = default;
            foundType = Array
                .Find(assembly!
                    .GetExportedTypes(), type => type.FullName!.Equals(typeName, StringComparison.OrdinalIgnoreCase));

            if (foundType is null)
            {
                typeException = new ArgumentException("Expected type not found.");
                return false;
            }

            return true;
        }
        catch (Exception exception) when (exception is NotSupportedException || exception is FileNotFoundException)
        {
            typeException = exception;
            return false;
        }
    }

}
