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
using Xpandables.Net.Interceptions;

namespace Xpandables.Net.Aspects;

/// <summary>
/// This class adds visitation to the method arguments of implementation of
/// the interface decorated with <see cref="AspectVisitorAttribute{TInterface}"/>.
/// </summary> 
/// <typeparam name="TInterface">The type of the interface.</typeparam>
/// <param name="serviceProvider">The service provider.</param>

public sealed class OnAspectVisitor<TInterface>(IServiceProvider serviceProvider) :
    OnAspect<AspectVisitorAttribute<TInterface>, TInterface>
    where TInterface : class
{
    ///<inheritdoc/>
    protected override void InterceptCore(IInvocation invocation)
    {
        foreach (Parameter argument in invocation.Arguments
            .Where(p => p.PassingBy == Parameter.PassingState.In
                && p.Type != typeof(CancellationToken)))
        {
            if (argument.Value is null)
            {
                continue;
            }

            if (argument.Value is not IAspectVisitable visitable)
            {
                continue;
            }

            Type type = typeof(IAspectVisitor<>)
                .MakeGenericType(argument.Type);

            IAspectVisitor? visitor = serviceProvider
                .GetService(type) as IAspectVisitor;

            visitor?.Visit(visitable);
        }

        invocation.Proceed();
    }
}
