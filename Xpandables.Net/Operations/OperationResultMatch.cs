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

namespace Xpandables.Net.Operations;

class OperationResultMatch : IOperationResultMatch
{
    private readonly OperationResult _operationResult;

    internal OperationResultMatch(OperationResult operationResult) => _operationResult = operationResult;

    public OperationResult Failure(Func<OperationResult, OperationResult> onFailure)
    {
        if (_operationResult.IsSuccess)
            return _operationResult;

        return onFailure(_operationResult);
    }

    public async ValueTask<OperationResult> FailureAsync(Func<OperationResult, ValueTask<OperationResult>> onFailure)
    {
        if (_operationResult.IsSuccess)
            return _operationResult;

        return await onFailure(_operationResult).ConfigureAwait(false);
    }

    public OperationResult Success(Func<OperationResult, OperationResult> onSuccess)
    {
        if (_operationResult.IsFailure)
            return _operationResult;

        return onSuccess(_operationResult);
    }

    public async ValueTask<OperationResult> SuccessAsync(Func<OperationResult, ValueTask<OperationResult>> onSuccess)
    {
        if (_operationResult.IsFailure)
            return _operationResult;

        return await onSuccess(_operationResult).ConfigureAwait(false);
    }
}

internal sealed class OperationResultMatch<TResult> : OperationResultMatch, IOperationResultMatch<TResult>
{
    private readonly OperationResult<TResult> _operationResult;

    internal OperationResultMatch(OperationResult<TResult> operationResult)
        : base(operationResult) => _operationResult = operationResult;

    public OperationResult<TResult> Failure(Func<OperationResult<TResult>, OperationResult<TResult>> onFailure)
    {
        if (_operationResult.IsSuccess)
            return _operationResult;

        return onFailure(_operationResult);
    }

    public async ValueTask<OperationResult<TResult>> FailureAsync(Func<OperationResult<TResult>, ValueTask<OperationResult<TResult>>> onFailure)
    {
        if (_operationResult.IsSuccess)
            return _operationResult;

        return await onFailure(_operationResult).ConfigureAwait(false);
    }

    public OperationResult<TResult> Success(Func<OperationResult<TResult>, OperationResult<TResult>> onSuccess)
    {
        if (_operationResult.IsFailure)
            return _operationResult;

        return onSuccess(_operationResult);
    }

    public async ValueTask<OperationResult<TResult>> SuccessAsync(Func<OperationResult<TResult>, ValueTask<OperationResult<TResult>>> onSuccess)
    {
        if (_operationResult.IsFailure)
            return _operationResult;

        return await onSuccess(_operationResult).ConfigureAwait(false);
    }
}