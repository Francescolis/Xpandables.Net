
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
using System.Globalization;
using System.Reflection;
using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using Xpandables.Net.Extensions;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Operations;

/// <summary>
/// Abstract class that holds the <see cref="FromModelBinder"/> dictionary.
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
        {
            BindingWithModel(bindingContext, modelType);
        }

        return Task.CompletedTask;
    }

    private static void BindingWithModel(ModelBindingContext bindingContext, Type modelType)
    {
#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            List<PropertyInfo> modelProperties = modelType.GetProperties()
                .Where(p => p.GetSetMethod()?.IsPublic == true)
                .ToList();

            object? model = default;

            // try parameter constructor
            var propertyTypes = modelProperties.Select(p => p.PropertyType).ToArray();
            var propertyValues = new Dictionary<string, object?>(propertyTypes.Length);

            foreach (var property in modelProperties)
            {
                CreateProperties(bindingContext, propertyValues, property);
            }

            Exception? constructorException = BindingWithConstructor(bindingContext, modelType, ref model, propertyValues);

            if (constructorException is not null)
                model = CreateInstance(bindingContext, modelType, modelProperties, propertyValues);

            if (bindingContext.ModelState.IsValid)
                bindingContext.Result = ModelBindingResult.Success(model);
        }
        catch (Exception exception)
        {
            bindingContext.ModelState.TryAddModelException(bindingContext.FieldName, exception);
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    private static object? CreateInstance(
        ModelBindingContext bindingContext,
        Type modelType,
        List<PropertyInfo> modelProperties,
        Dictionary<string, object?> propertyValues)
    {
        object? model = Activator.CreateInstance(modelType);
        if (model is not null) // parameterless constructor
        {
            foreach (var property in modelProperties)
            {
#pragma warning disable CA1031 // Do not catch general exception types
                try
                {
                    property.SetValue(model, propertyValues[property.Name]);
                }
                catch (Exception exception)
                {
                    bindingContext.ModelState.TryAddModelException(property.Name, exception);
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }
        }
        else // deserialization
        {
            string dictString = JsonSerializer.Serialize(propertyValues, JsonSerializerDefaultOptions.OptionDefaultWeb);
            model = JsonSerializer.Deserialize(dictString, modelType, JsonSerializerDefaultOptions.OptionDefaultWeb);
        }

        return model;
    }

    private static void CreateProperties(
        ModelBindingContext bindingContext,
        Dictionary<string, object?> propertyValues,
        PropertyInfo property)
    {
#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            object? value = RequestAttributeModelReader[new TAttribute()](bindingContext.HttpContext, property.Name);
            object? converted = default;
            if (value is not null)
                converted = value.ChangeTypeNullable(property.PropertyType, CultureInfo.CurrentCulture);

            propertyValues.Add(property.Name, converted);
        }
        catch (Exception exception)
        {
            bindingContext.ModelState.TryAddModelException(property.Name, exception);
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    private static Exception? BindingWithConstructor(ModelBindingContext bindingContext, Type modelType, ref object? model, Dictionary<string, object?> propertyValues)
    {
        Exception? constructorException = default;
        if (bindingContext.ModelState.IsValid)
        {
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                model = Activator.CreateInstance(modelType, [.. propertyValues.Values]);
            }
            catch (Exception exception)
            {
                constructorException = exception;
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        return constructorException;
    }

    private static void BindingWithModelName(
        ModelBindingContext bindingContext,
        string modelName,
        Type modelType)
    {
        object? attributeValue = RequestAttributeModelReader[new TAttribute()](bindingContext.HttpContext, modelName);
        if (attributeValue is string value)
        {
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                var model = JsonSerializer.Deserialize(value, modelType, JsonSerializerDefaultOptions.OptionDefaultWeb);
                bindingContext.Result = ModelBindingResult.Success(model);
            }
            catch (Exception exception)
            {
                bindingContext.ModelState.TryAddModelException(bindingContext.FieldName, exception);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }
    }
}
