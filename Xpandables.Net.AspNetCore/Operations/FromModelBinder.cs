
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using System.Globalization;
using System.Reflection;
using System.Text.Json;

using Xpandables.Net.Primitives;

namespace Xpandables.Net.Operations;

/// <summary>
/// Abstract class that holds the <see cref="FromModelBinder"/> dictionary attributes.
/// </summary>
public abstract class FromModelBinder
{
    /// <summary>
    /// Creates an instance of <see cref="FromModelBinder"/>.
    /// </summary>
    protected FromModelBinder() { }

    /// <summary>
    /// Contains a dictionary with key as attribute and the request path values matching the specified name.
    /// </summary>
    protected static IDictionary<Attribute, Func<HttpContext, string, object?>> RequestAttributeModelReader
        => new Dictionary<Attribute, Func<HttpContext, string, object?>>
        {
            { new FromHeaderAttribute(), (context, name) => context.Request.Headers[name].FirstOrDefault() },
            { new FromRouteAttribute(), (context, name) => context.Request.RouteValues[name] },
            { new FromQueryAttribute(), (context, name) => context.Request.Query[CultureInfo.CurrentCulture.TextInfo.ToTitleCase( name)].FirstOrDefault() }
        };
}

/// <summary>
/// Model binder used to bind models from the specified attributes :
/// <see cref="FromHeaderAttribute"/>, <see cref="FromRouteAttribute"/> and <see cref="FromQueryAttribute"/>.
/// <para>It tries to bind with a parameterless constructor, 
/// if not found, uses serialization with <see cref="System.Text.Json"/>.</para>
/// </summary>
/// <typeparam name="TAttribute">the type of the attribute.</typeparam>
public sealed class FromModelBinder<TAttribute> : FromModelBinder, IModelBinder
     where TAttribute : Attribute, new()
{
    ///<inheritdoc/>
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        _ = bindingContext ?? throw new ArgumentNullException(nameof(bindingContext));

        string? modelName = bindingContext.ModelMetadata.BinderModelName;
        Type modelType = bindingContext.ModelMetadata.ModelType;

        if (modelName is not null)
            BindingWithModelName(bindingContext, modelName, modelType);
        else
            BindingWithModelType(bindingContext, modelType);

        return Task.CompletedTask;
    }

    private static void BindingWithModelType(ModelBindingContext bindingContext, Type modelType)
    {
        List<PropertyInfo> modelProperties = modelType.GetProperties()
            .Where(p => p.GetSetMethod()?.IsPublic == true)
            .ToList();

        object? model = default;

        // try default constructor
        if (modelProperties.Count == 0)
        {
            model = CreateDefaultInstance(bindingContext, modelType);
            if (model is not null)
                bindingContext.Result = ModelBindingResult.Success(model);

            // no property to bind
            return;
        }

        // create properties
        Dictionary<string, object?> propertyValues = CreateProperties(bindingContext, modelProperties);
        if (bindingContext.ModelState.IsValid is false)
            return;

        // try parameter constructor
        model = CreateConstructorInstance(bindingContext, modelType, propertyValues);
        if (model is not null)
        {
            bindingContext.Result = ModelBindingResult.Success(model);
            return;
        }

        // try parameterless constructor
        model = CreateParameterlessInstance(bindingContext, modelType, modelProperties, propertyValues);
        if (model is not null)
        {
            bindingContext.Result = ModelBindingResult.Success(model);
            return;
        }

        // try deserialization
        model = CreateDeserializedInstance(bindingContext, modelType, propertyValues);
        if (model is not null)
        {
            bindingContext.Result = ModelBindingResult.Success(model);
        }
    }

    private static void BindingWithModelName(
        ModelBindingContext bindingContext,
        string modelName,
        Type modelType)
    {
        object? attributeValue = RequestAttributeModelReader[new TAttribute()](bindingContext.HttpContext, modelName);

        if (attributeValue is string value)
        {
            try
            {
                object? model = JsonSerializer
                    .Deserialize(value, modelType, JsonSerializerDefaultOptions.OptionDefaultWeb);

                bindingContext.Result = ModelBindingResult.Success(model);
            }
            catch (Exception exception)
                when (exception is ArgumentNullException
                        or JsonException
                        or NotSupportedException)
            {
                _ = bindingContext.ModelState
                    .TryAddModelException(bindingContext.ModelName, exception);
            }
        }
        else
        {
            _ = bindingContext.ModelState
                .TryAddModelError(bindingContext.ModelName, "Invalid value");
        }
    }

    private static object? CreateDefaultInstance(
        ModelBindingContext bindingContext,
        Type modelType)
    {
        object? model = default;
        if (modelType.GetConstructor(Type.EmptyTypes) is not null)
            try
            {
                model = Activator.CreateInstance(modelType);
            }
            catch (Exception exception)
                when (exception is ArgumentNullException
                        or ArgumentException
                        or NotSupportedException
                        or TargetInvocationException
                        or MethodAccessException
                        or MemberAccessException
                        or System.Runtime.InteropServices.InvalidComObjectException
                        or MissingMethodException
                        or System.Runtime.InteropServices.COMException
                        or TypeLoadException)
            {
                _ = bindingContext.ModelState
                    .TryAddModelException(bindingContext.ModelName, exception);
            }

        return model;
    }

    private static object? CreateDeserializedInstance(
        ModelBindingContext bindingContext,
        Type modelType,
        Dictionary<string, object?> propertyValues)
    {
        try
        {
            string dictString = JsonSerializer.Serialize(propertyValues, JsonSerializerDefaultOptions.OptionDefaultWeb);
            return JsonSerializer.Deserialize(dictString, modelType, JsonSerializerDefaultOptions.OptionDefaultWeb);
        }
        catch (Exception exception)
            when (exception is ArgumentNullException
                    or JsonException
                    or NotSupportedException)
        {
            _ = bindingContext.ModelState
                .TryAddModelException(bindingContext.ModelName, exception);

            return default;
        }
    }

    private static object? CreateConstructorInstance(
        ModelBindingContext bindingContext,
        Type modelType,
        Dictionary<string, object?> propertyValues)
    {
        if (bindingContext.ModelState.IsValid)
        {
            try
            {
                return Activator.CreateInstance(modelType, [.. propertyValues.Values]);
            }
            catch (Exception exception)
                when (exception is ArgumentNullException
                        or ArgumentException
                        or NotSupportedException
                        or TargetInvocationException
                        or MethodAccessException
                        or MemberAccessException
                        or System.Runtime.InteropServices.InvalidComObjectException
                        or MissingMethodException
                        or System.Runtime.InteropServices.COMException
                        or TypeLoadException)
            {
                _ = bindingContext.ModelState
                    .TryAddModelException(bindingContext.ModelName, exception);
            }
        }

        return default;
    }

    private static object? CreateParameterlessInstance(
        ModelBindingContext bindingContext,
        Type modelType,
        List<PropertyInfo> propertyInfos,
        Dictionary<string, object?> propertyValues)
    {
        try
        {
            object? model = Activator.CreateInstance(modelType);
            if (model is not null) // parameterless constructor
            {
                foreach (PropertyInfo property in propertyInfos)
                {
                    try
                    {
                        property.SetValue(model, propertyValues[property.Name]);
                    }
                    catch (Exception exception)
                        when (exception is ArgumentNullException
                                or ArgumentException
                                or NotSupportedException
                                or TargetInvocationException
                                or MethodAccessException
                                or MemberAccessException
                                or System.Runtime.InteropServices.InvalidComObjectException
                                or MissingMethodException
                                or System.Runtime.InteropServices.COMException
                                or TypeLoadException)
                    {
                        _ = bindingContext.ModelState
                            .TryAddModelException(property.Name, exception);
                    }
                }
            }
            else
            {
                _ = bindingContext.ModelState
                    .TryAddModelError(bindingContext.ModelName, "Invalid value : creating instance null");
            }

            return model;
        }
        catch (Exception exception)
            when (exception is ArgumentNullException
                    or ArgumentException
                    or NotSupportedException
                    or TargetInvocationException
                    or MethodAccessException
                    or MemberAccessException
                    or System.Runtime.InteropServices.InvalidComObjectException
                    or MissingMethodException
                    or System.Runtime.InteropServices.COMException
                    or TypeLoadException)
        {
            _ = bindingContext.ModelState
                .TryAddModelException(bindingContext.ModelName, exception);

            return default;
        }
    }

    private static Dictionary<string, object?> CreateProperties(
        ModelBindingContext bindingContext,
        List<PropertyInfo> propertyInfos)
    {
        Dictionary<string, object?> propertyValues = new(propertyInfos.Count);
        PropertyInfo? currentPropertyInfo = default;
        try
        {
            foreach (PropertyInfo property in propertyInfos)
            {
                currentPropertyInfo = property;
                object? value = RequestAttributeModelReader[new TAttribute()](bindingContext.HttpContext, property.Name);
                object? converted = default;

                if (value is not null)
                    converted = value.ChangeTypeNullable(property.PropertyType, CultureInfo.CurrentCulture);

                propertyValues.Add(property.Name, converted);
            }
        }
        catch (Exception exception)
            when (exception is ArgumentNullException
                    or ArgumentException
                    or NotSupportedException
                    or InvalidCastException
                    or FormatException
                    or OverflowException
                    or InvalidCastException)
        {
            if (currentPropertyInfo is not null)
                _ = bindingContext.ModelState.TryAddModelException(currentPropertyInfo.Name, exception);
            else
                _ = bindingContext.ModelState.TryAddModelException(bindingContext.ModelName, exception);
        }

        return propertyValues;
    }

}
