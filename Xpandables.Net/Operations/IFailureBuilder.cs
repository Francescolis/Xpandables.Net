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
namespace Xpandables.Net.Operations;

/// <summary>  
/// Provides a builder interface for constructing failure operation results.  
/// </summary>  
public interface IFailureBuilder :
   IErrorBuilder<IFailureBuilder>,
   IHeaderBuilder<IFailureBuilder>,
   ILocationBuilder<IFailureBuilder>,
   IDetailBuilder<IFailureBuilder>,
   ITitleBuilder<IFailureBuilder>,
   IMergeBuilder<IFailureBuilder>,
   IStatusBuilder<IFailureBuilder>,
   IExtensionBuilder<IFailureBuilder>,
   IClearBuilder<IFailureBuilder>,
   IBuilder
{ }

/// <summary>  
/// Provides a builder interface for constructing failure operation results 
/// with a specific result type.  
/// </summary>  
/// <typeparam name="TResult">The type of the result.</typeparam>  
public interface IFailureBuilder<TResult> :
   IErrorBuilder<IFailureBuilder<TResult>>,
   IHeaderBuilder<IFailureBuilder<TResult>>,
   ILocationBuilder<IFailureBuilder<TResult>>,
   IDetailBuilder<IFailureBuilder<TResult>>,
   ITitleBuilder<IFailureBuilder<TResult>>,
   IMergeBuilder<IFailureBuilder<TResult>>,
   IStatusBuilder<IFailureBuilder<TResult>>,
   IExtensionBuilder<IFailureBuilder<TResult>>,
   IClearBuilder<IFailureBuilder<TResult>>,
   IBuilder<TResult>
{ }