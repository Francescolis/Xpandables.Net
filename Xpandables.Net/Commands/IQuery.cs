﻿/*******************************************************************************
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
namespace Xpandables.Net.Commands;

/// <summary>
/// This interface is used as a marker for query.
/// Class implementation is used with the <see cref="ICommandHandler{TQuery}"/> 
/// where "TQuery" is a class that implements <see cref="IQuery{TResult}"/>.
/// This can also be enhanced with some useful decorators.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
#pragma warning disable CA1040 // Avoid empty interfaces
public interface IQuery<out TResult>
#pragma warning restore CA1040 // Avoid empty interfaces
{
}
