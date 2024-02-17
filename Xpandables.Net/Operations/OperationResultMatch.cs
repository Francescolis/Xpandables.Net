
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

internal class OperationResultMatch : IOperationResultMatch
{
    private readonly IOperationResult _operationResult;

    internal OperationResultMatch(IOperationResult operationResult)
        => _operationResult = operationResult;

    public IOperationResult Failure(Func<IOperationResult, IOperationResult> onFailure)
    {
        if (_operationResult.IsSuccess)
            return _operationResult;

        return onFailure(_operationResult);
    }

    public async ValueTask<IOperationResult> FailureAsync(
        Func<IOperationResult, ValueTask<IOperationResult>> onFailure)
    {
        if (_operationResult.IsSuccess)
            return _operationResult;

        return await onFailure(_operationResult).ConfigureAwait(false);
    }

    public IOperationResult Success(Func<IOperationResult, IOperationResult> onSuccess)
    {
        if (_operationResult.IsFailure)
            return _operationResult;

        return onSuccess(_operationResult);
    }

    public async ValueTask<IOperationResult> SuccessAsync(
        Func<IOperationResult, ValueTask<IOperationResult>> onSuccess)
    {
        if (_operationResult.IsFailure)
            return _operationResult;

        return await onSuccess(_operationResult)
            .ConfigureAwait(false);
    }
}

internal sealed class OperationResultMatch<TResult> : OperationResultMatch, IOperationResultMatch<TResult>
{
    private readonly IOperationResult<TResult> _operationResult;

    internal OperationResultMatch(IOperationResult<TResult> operationResult)
        : base(operationResult) => _operationResult = operationResult;

    public IOperationResult<TResult> Failure(
        Func<IOperationResult<TResult>, IOperationResult<TResult>> onFailure)
    {
        if (_operationResult.IsSuccess)
            return _operationResult;

        return onFailure(_operationResult);
    }

    public IOperationResult<TReturn> Failure<TReturn>(
        Func<IOperationResult<TResult>, IOperationResult<TReturn>> onFailure)
    {
        if (_operationResult.IsSuccess)
            return _operationResult.ToOperationResult<TReturn>();

        return onFailure(_operationResult);
    }


    public async ValueTask<IOperationResult<TResult>> FailureAsync(
        Func<IOperationResult<TResult>, ValueTask<IOperationResult<TResult>>> onFailure)
    {
        if (_operationResult.IsSuccess)
            return _operationResult;

        return await onFailure(_operationResult)
            .ConfigureAwait(false);
    }

    public async ValueTask<IOperationResult<TReturn>> FailureAsync<TReturn>(
        Func<IOperationResult<TResult>, ValueTask<IOperationResult<TReturn>>> onFailure)
    {
        if (_operationResult.IsSuccess)
            return _operationResult.ToOperationResult<TReturn>();

        return await onFailure(_operationResult)
            .ConfigureAwait(false);
    }

    public IOperationResult<TResult> Success(
        Func<IOperationResult<TResult>, IOperationResult<TResult>> onSuccess)
    {
        if (_operationResult.IsFailure)
            return _operationResult;

        return onSuccess(_operationResult);
    }

    public IOperationResult<TReturn> Success<TReturn>(
        Func<IOperationResult<TResult>, IOperationResult<TReturn>> onSuccess)
    {
        if (_operationResult.IsFailure)
            return _operationResult.ToOperationResult<TReturn>();

        return onSuccess(_operationResult);
    }

    public async ValueTask<IOperationResult<TResult>> SuccessAsync(
        Func<IOperationResult<TResult>, ValueTask<IOperationResult<TResult>>> onSuccess)
    {
        if (_operationResult.IsFailure)
            return _operationResult;

        return await onSuccess(_operationResult)
            .ConfigureAwait(false);
    }

    public async ValueTask<IOperationResult<TReturn>> SuccessAsync<TReturn>(
        Func<IOperationResult<TResult>, ValueTask<IOperationResult<TReturn>>> onSuccess)
    {
        if (_operationResult.IsFailure)
            return _operationResult.ToOperationResult<TReturn>();

        return await onSuccess(_operationResult)
            .ConfigureAwait(false);
    }
}