﻿
/************************************************************************************************************
 * Copyright (C) 2020 Francis-Black EWANE
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
using Microsoft.Extensions.DependencyInjection;

using System;

using Xpandables.Net.EmailEvents;

namespace Xpandables.Net.DependencyInjection
{
    /// <summary>
    /// Provides method to register Email objects.
    /// </summary>
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the specified <see cref="IEmailSender{TEmailMessage}"/> implementation 
        /// to the services with scope life time.
        /// </summary>
        /// <typeparam name="TEmailMessage">the type of message.</typeparam>
        /// <typeparam name="TEmailSender">The type of the email sender.</typeparam>
        /// <param name="services">The collection of services.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
        public static IXpandableServiceBuilder AddXEmailSender<TEmailMessage, TEmailSender>(
            this IXpandableServiceBuilder services)
            where TEmailMessage : notnull
            where TEmailSender : class, IEmailSender<TEmailMessage>
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));

            services.Services.AddScoped<IEmailSender<TEmailMessage>, TEmailSender>();
            services.Services.AddScoped(provider => (IEmailSender)provider.GetRequiredService<IEmailSender<TEmailMessage>>());
            return services;
        }
    }
}
