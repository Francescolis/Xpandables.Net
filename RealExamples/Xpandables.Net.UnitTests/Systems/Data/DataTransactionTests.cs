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
using System.Data;

using FluentAssertions;

using Microsoft.Data.Sqlite;

namespace Xpandables.Net.UnitTests.Systems.Data;

public sealed class DataTransactionTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public DataTransactionTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public void Dispose() => _connection.Dispose();

    [Fact]
    public void WhenCommitThenIsCommittedIsTrue()
    {
        var dbTransaction = _connection.BeginTransaction();
        var transaction = new DataTransaction(dbTransaction);

        transaction.Commit();

        transaction.IsCommitted.Should().BeTrue();
        transaction.IsCompleted.Should().BeTrue();
        transaction.IsRolledBack.Should().BeFalse();
    }

    [Fact]
    public void WhenRollbackThenIsRolledBackIsTrue()
    {
        var dbTransaction = _connection.BeginTransaction();
        var transaction = new DataTransaction(dbTransaction);

        transaction.Rollback();

        transaction.IsRolledBack.Should().BeTrue();
        transaction.IsCompleted.Should().BeTrue();
        transaction.IsCommitted.Should().BeFalse();
    }

    [Fact]
    public async Task WhenCommitAsyncThenIsCommittedIsTrue()
    {
        var dbTransaction = _connection.BeginTransaction();
        var transaction = new DataTransaction(dbTransaction);

        await transaction.CommitAsync();

        transaction.IsCommitted.Should().BeTrue();
        transaction.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task WhenRollbackAsyncThenIsRolledBackIsTrue()
    {
        var dbTransaction = _connection.BeginTransaction();
        var transaction = new DataTransaction(dbTransaction);

        await transaction.RollbackAsync();

        transaction.IsRolledBack.Should().BeTrue();
        transaction.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void WhenCommitTwiceThenThrowsInvalidOperationException()
    {
        var dbTransaction = _connection.BeginTransaction();
        var transaction = new DataTransaction(dbTransaction);

        transaction.Commit();

        var act = () => transaction.Commit();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already been committed*");
    }

    [Fact]
    public void WhenRollbackTwiceThenThrowsInvalidOperationException()
    {
        var dbTransaction = _connection.BeginTransaction();
        var transaction = new DataTransaction(dbTransaction);

        transaction.Rollback();

        var act = () => transaction.Rollback();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already been rolled back*");
    }

    [Fact]
    public void WhenCommitThenRollbackThenThrowsInvalidOperationException()
    {
        var dbTransaction = _connection.BeginTransaction();
        var transaction = new DataTransaction(dbTransaction);

        transaction.Commit();

        var act = () => transaction.Rollback();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void WhenDisposeWithoutCommitThenAutoRollback()
    {
        var dbTransaction = _connection.BeginTransaction();
        var transaction = new DataTransaction(dbTransaction);

        transaction.Dispose();

        transaction.IsRolledBack.Should().BeTrue();
        transaction.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task WhenDisposeAsyncWithoutCommitThenAutoRollback()
    {
        var dbTransaction = _connection.BeginTransaction();
        var transaction = new DataTransaction(dbTransaction);

        await transaction.DisposeAsync();

        transaction.IsRolledBack.Should().BeTrue();
        transaction.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void WhenDisposeAfterCommitThenNoRollback()
    {
        var dbTransaction = _connection.BeginTransaction();
        var transaction = new DataTransaction(dbTransaction);

        transaction.Commit();
        transaction.Dispose();

        transaction.IsCommitted.Should().BeTrue();
        transaction.IsRolledBack.Should().BeFalse();
    }

    [Fact]
    public void WhenCallbackRegisteredThenFiresOnCommit()
    {
        var callbackFired = false;
        var dbTransaction = _connection.BeginTransaction();
        var transaction = new DataTransaction(dbTransaction, () => callbackFired = true);

        transaction.Commit();

        callbackFired.Should().BeTrue();
    }

    [Fact]
    public void WhenCallbackRegisteredThenFiresOnRollback()
    {
        var callbackFired = false;
        var dbTransaction = _connection.BeginTransaction();
        var transaction = new DataTransaction(dbTransaction, () => callbackFired = true);

        transaction.Rollback();

        callbackFired.Should().BeTrue();
    }

    [Fact]
    public void WhenDisposedThenCommitThrowsObjectDisposedException()
    {
        var dbTransaction = _connection.BeginTransaction();
        var transaction = new DataTransaction(dbTransaction);

        transaction.Dispose();

        var act = () => transaction.Commit();
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void WhenDisposedThenRollbackThrowsObjectDisposedException()
    {
        var dbTransaction = _connection.BeginTransaction();
        var transaction = new DataTransaction(dbTransaction);

        transaction.Dispose();

        var act = () => transaction.Rollback();
        act.Should().Throw<ObjectDisposedException>();
    }
}
