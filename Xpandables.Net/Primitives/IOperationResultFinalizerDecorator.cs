﻿
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

// Ignore Spelling: Finalizer

using Xpandables.Net.Operations;

namespace Xpandables.Net.Primitives;
/// <summary>
/// Defines a marker interface that allows the command/query class 
/// to add correlation decorator context result after a control flow.
/// In the calls handling the query/command, using the
/// <see cref="IOperationResultFinalizer"/>
/// </summary>
#pragma warning disable CA1040 // Avoid empty interfaces
public interface IOperationResultFinalizerDecorator { }
#pragma warning restore CA1040 // Avoid empty interfaces