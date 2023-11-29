
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Xpandables.Net.Operations;

/// <summary>
/// Adds <see cref="IOperationResult"/>/<see cref="IOperationResult{TValue}"/> 
/// filters to <see cref="MvcOptions"/>.
/// You can derive from this class to customize its behavior.
/// </summary>
/// <remarks>Adds filters <see cref="OperationResultControllerValidationFilterAttribute"/>,
/// <see cref="OperationResultControllerFilter"/>
/// and model binder provider <see cref="FromModelBinderProvider"/>.
/// </remarks>
public class OperationResultControllerConfigureMvcOptions : IConfigureOptions<MvcOptions>
{
    ///<inheritdoc/>
    public virtual void Configure(MvcOptions options)
    {
        _ = options ?? throw new ArgumentNullException(nameof(options));

        options.EnableEndpointRouting = false;
        options.RespectBrowserAcceptHeader = true;
        options.ReturnHttpNotAcceptable = true;
        options.Filters.Add<OperationResultControllerValidationFilterAttribute>();
        options.Filters.Add<OperationResultControllerFilter>(int.MinValue);
        options.ModelBinderProviders.Insert(0, new FromModelBinderProvider());

        TypeDescriptor.AddAttributes(typeof(DateOnly), new TypeConverterAttribute(typeof(DateOnlyConverter)));
        TypeDescriptor.AddAttributes(typeof(TimeOnly), new TypeConverterAttribute(typeof(TimeOnlyConverter)));
    }
}
