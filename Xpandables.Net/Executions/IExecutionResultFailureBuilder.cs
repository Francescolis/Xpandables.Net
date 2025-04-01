/*******************************************************************************  
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
namespace Xpandables.Net.Executions;

/// <summary>  
/// Provides a builder interface for constructing failure execution results.  
/// </summary>  
public interface IExecutionResultFailureBuilder :
   IExecutionResultErrorBuilder<IExecutionResultFailureBuilder>,
   IExecutionResultHeaderBuilder<IExecutionResultFailureBuilder>,
   IExecutionResultLocationBuilder<IExecutionResultFailureBuilder>,
   IExecutionResultDetailBuilder<IExecutionResultFailureBuilder>,
   IExecutionResultTitleBuilder<IExecutionResultFailureBuilder>,
   IExecutionResultMergeBuilder<IExecutionResultFailureBuilder>,
   IExecutionResultStatusBuilder<IExecutionResultFailureBuilder>,
   IExecutionResultExtensionBuilder<IExecutionResultFailureBuilder>,
   IExecutionResultClearBuilder<IExecutionResultFailureBuilder>,
   IExecutionResultBuilder
{ }

/// <summary>  
/// Provides a builder interface for constructing failure execution results 
/// with a specific result type.  
/// </summary>  
/// <typeparam name="TResult">The type of the result.</typeparam>  
public interface IExecutionResultFailureBuilder<TResult> :
   IExecutionResultErrorBuilder<IExecutionResultFailureBuilder<TResult>>,
   IExecutionResultHeaderBuilder<IExecutionResultFailureBuilder<TResult>>,
   IExecutionResultLocationBuilder<IExecutionResultFailureBuilder<TResult>>,
   IExecutionResultDetailBuilder<IExecutionResultFailureBuilder<TResult>>,
   IExecutionResultTitleBuilder<IExecutionResultFailureBuilder<TResult>>,
   IExecutionResultMergeBuilder<IExecutionResultFailureBuilder<TResult>>,
   IExecutionResultStatusBuilder<IExecutionResultFailureBuilder<TResult>>,
   IExecutionResultExtensionBuilder<IExecutionResultFailureBuilder<TResult>>,
   IExecutionResultClearBuilder<IExecutionResultFailureBuilder<TResult>>,
   IExecutionResultBuilder<TResult>
{ }