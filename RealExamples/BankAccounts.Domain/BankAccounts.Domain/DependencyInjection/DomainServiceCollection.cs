/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.ComponentModel.Composition;
using System.Composition;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankAccounts.Domain.DependencyInjection;

[Export(typeof(IAddServiceExport))]
public sealed class DomainServiceCollection : IAddServiceExport
{
	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
	[UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
	public void AddServices(IServiceCollection services, IConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(configuration);

		services.AddXRequestHandlers()
			.AddXEventHandlers()
			.AddXEventConverterContext()
			.AddXCompositeValidator()
			.AddXEventPublisher()
			.AddXAggregateStore()
			.AddXDataUnitOfWork()
			.AddXSqlMapper()
			.AddXMsSqlBuilder()
			.AddXDbConnectionScopeFactory()
			.AddXDbConnectionScope()
			.AddXEventStores()
			.AddXEventConverterFactory()
			.AddXCacheTypeResolver()
			.AddXEventContextAccessor()
			.AddXDomainEventEnricher()
			.AddXIntegrationEventEnricher();
	}
}
