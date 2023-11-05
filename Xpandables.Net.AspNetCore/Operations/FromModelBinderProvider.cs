
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Xpandables.Net.Operations;

/// <summary>
/// Provides with the binder matching one of the attributes : <see cref="FromHeaderAttribute"/>, <see cref="FromQueryAttribute"/> or <see cref="FromRouteAttribute"/>.
/// </summary>
public sealed class FromModelBinderProvider : IModelBinderProvider
{
    /// <summary>
    /// Creates a <see cref="IModelBinder" /> based on <see cref="ModelBinderProviderContext" />.
    /// </summary>
    /// <param name="context">The <see cref="ModelBinderProviderContext" />.</param>
    /// <returns>An <see cref="IModelBinder" />.</returns>
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        _ = context ?? throw new ArgumentNullException(nameof(context));

        if (context.Metadata.IsComplexType)
        {
            var metaData = (DefaultModelMetadata)context.Metadata;

            if (metaData.Attributes.Attributes.OfType<FromHeaderAttribute>().FirstOrDefault() is not null)
                return new BinderTypeModelBinder(typeof(FromModelBinder<FromHeaderAttribute>));

            if (metaData.Attributes.Attributes.OfType<FromQueryAttribute>().FirstOrDefault() is not null)
                return new BinderTypeModelBinder(typeof(FromModelBinder<FromQueryAttribute>));

            if (metaData.Attributes.Attributes.OfType<FromRouteAttribute>().FirstOrDefault() is not null)
                return new BinderTypeModelBinder(typeof(FromModelBinder<FromRouteAttribute>));
        }

        return default;
    }
}
