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

public sealed class DbConnectionScopeTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public DbConnectionScopeTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public void Dispose() => _connection.Dispose();

    [Fact]
    public void WhenBeginTransactionThenHasActiveTransactionIsTrue()
    {
        var scope = new DbConnectionScope(_connection);

        var transaction = scope.BeginTransaction();

        scope.HasActiveTransaction.Should().BeTrue();
        scope.CurrentTransaction.Should().BeSameAs(transaction);
    }

    [Fact]
    public async Task WhenBeginTransactionAsyncThenHasActiveTransactionIsTrue()
    {
        var scope = new DbConnectionScope(_connection);

        var transaction = await scope.BeginTransactionAsync();

        scope.HasActiveTransaction.Should().BeTrue();
        scope.CurrentTransaction.Should().BeSameAs(transaction);
    }

    [Fact]
    public void WhenTwoActiveTransactionsThenThrowsInvalidOperationException()
    {
        var scope = new DbConnectionScope(_connection);
        scope.BeginTransaction();

        var act = () => scope.BeginTransaction();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already active*");
    }

    [Fact]
    public void WhenTransactionCommittedThenNewTransactionAllowed()
    {
        var scope = new DbConnectionScope(_connection);

        var first = scope.BeginTransaction();
        first.Commit();

        var second = scope.BeginTransaction();
        second.Should().NotBeNull();
        scope.HasActiveTransaction.Should().BeTrue();
    }

    [Fact]
    public void WhenTransactionRolledBackThenNewTransactionAllowed()
    {
        var scope = new DbConnectionScope(_connection);

        var first = scope.BeginTransaction();
        first.Rollback();

        var second = scope.BeginTransaction();
        second.Should().NotBeNull();
        scope.HasActiveTransaction.Should().BeTrue();
    }

    [Fact]
    public void WhenCreateCommandWithActiveTransactionThenCommandHasTransaction()
    {
        var scope = new DbConnectionScope(_connection);
        var transaction = scope.BeginTransaction();

        var command = scope.CreateCommand();

        command.Transaction.Should().BeSameAs(transaction.DbTransaction);
    }

    [Fact]
    public void WhenCreateCommandWithoutTransactionThenCommandHasNoTransaction()
    {
        var scope = new DbConnectionScope(_connection);

        var command = scope.CreateCommand();

        command.Transaction.Should().BeNull();
    }

    [Fact]
    public void WhenCreateCommandWithQueryResultThenCommandHasSqlAndParameters()
    {
        var scope = new DbConnectionScope(_connection);
        var queryResult = new SqlQueryResult("SELECT 1 WHERE @p0 = 1", [new SqlParameter("p0", 1)]);

        var command = scope.CreateCommand(queryResult);

        command.CommandText.Should().Be("SELECT 1 WHERE @p0 = 1");
        command.Parameters.Count.Should().Be(1);
    }

    [Fact]
    public void WhenDisposedThenConnectionPropertyThrowsObjectDisposedException()
    {
        var scope = new DbConnectionScope(_connection);

        scope.Dispose();

        var act = () => _ = scope.Connection;
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void WhenDisposedThenBeginTransactionThrowsObjectDisposedException()
    {
        var scope = new DbConnectionScope(_connection);

        scope.Dispose();

        var act = () => scope.BeginTransaction();
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void WhenDisposedThenCreateCommandThrowsObjectDisposedException()
    {
        var scope = new DbConnectionScope(_connection);

        scope.Dispose();

        var act = () => scope.CreateCommand();
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void WhenNoTransactionThenHasActiveTransactionIsFalse()
    {
        var scope = new DbConnectionScope(_connection);

        scope.HasActiveTransaction.Should().BeFalse();
        scope.CurrentTransaction.Should().BeNull();
    }

    [Fact]
    public void WhenTransactionCommittedThenCurrentTransactionIsNull()
    {
        var scope = new DbConnectionScope(_connection);

        var transaction = scope.BeginTransaction();
        transaction.Commit();

        scope.CurrentTransaction.Should().BeNull();
        scope.HasActiveTransaction.Should().BeFalse();
    }
}
