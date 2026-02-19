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
using System.Data.Common;

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
        var scope = new DataDbConnectionScope(_connection);

		IDataTransaction transaction = scope.BeginTransaction();

        scope.HasActiveTransaction.Should().BeTrue();
        scope.CurrentTransaction.Should().BeSameAs(transaction);
    }

    [Fact]
    public async Task WhenBeginTransactionAsyncThenHasActiveTransactionIsTrue()
    {
        var scope = new DataDbConnectionScope(_connection);

		IDataTransaction transaction = await scope.BeginTransactionAsync();

        scope.HasActiveTransaction.Should().BeTrue();
        scope.CurrentTransaction.Should().BeSameAs(transaction);
    }

    [Fact]
    public void WhenTwoActiveTransactionsThenThrowsInvalidOperationException()
    {
        var scope = new DataDbConnectionScope(_connection);
        scope.BeginTransaction();

		Func<IDataTransaction> act = () => scope.BeginTransaction();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already active*");
    }

    [Fact]
    public void WhenTransactionCommittedThenNewTransactionAllowed()
    {
        var scope = new DataDbConnectionScope(_connection);

		IDataTransaction first = scope.BeginTransaction();
        first.Commit();

		IDataTransaction second = scope.BeginTransaction();
        second.Should().NotBeNull();
        scope.HasActiveTransaction.Should().BeTrue();
    }

    [Fact]
    public void WhenTransactionRolledBackThenNewTransactionAllowed()
    {
        var scope = new DataDbConnectionScope(_connection);

		IDataTransaction first = scope.BeginTransaction();
        first.Rollback();

		IDataTransaction second = scope.BeginTransaction();
        second.Should().NotBeNull();
        scope.HasActiveTransaction.Should().BeTrue();
    }

    [Fact]
    public void WhenCreateCommandWithActiveTransactionThenCommandHasTransaction()
    {
        var scope = new DataDbConnectionScope(_connection);
		IDataTransaction transaction = scope.BeginTransaction();

		DbCommand command = scope.CreateCommand();

        command.Transaction.Should().BeSameAs(transaction.DbTransaction);
    }

    [Fact]
    public void WhenCreateCommandWithoutTransactionThenCommandHasNoTransaction()
    {
        var scope = new DataDbConnectionScope(_connection);

		DbCommand command = scope.CreateCommand();

        command.Transaction.Should().BeNull();
    }

    [Fact]
    public void WhenCreateCommandWithQueryResultThenCommandHasSqlAndParameters()
    {
        var scope = new DataDbConnectionScope(_connection);
        var queryResult = new SqlQueryResult("SELECT 1 WHERE @p0 = 1", [new SqlParameter("p0", 1)]);

		DbCommand command = scope.CreateCommand(queryResult);

        command.CommandText.Should().Be("SELECT 1 WHERE @p0 = 1");
        command.Parameters.Count.Should().Be(1);
    }

    [Fact]
    public void WhenDisposedThenConnectionPropertyThrowsObjectDisposedException()
    {
        var scope = new DataDbConnectionScope(_connection);

        scope.Dispose();

		Func<DbConnection> act = () => _ = scope.Connection;
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void WhenDisposedThenBeginTransactionThrowsObjectDisposedException()
    {
        var scope = new DataDbConnectionScope(_connection);

        scope.Dispose();

		Func<IDataTransaction> act = () => scope.BeginTransaction();
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void WhenDisposedThenCreateCommandThrowsObjectDisposedException()
    {
        var scope = new DataDbConnectionScope(_connection);

        scope.Dispose();

		Func<DbCommand> act = () => scope.CreateCommand();
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void WhenNoTransactionThenHasActiveTransactionIsFalse()
    {
        var scope = new DataDbConnectionScope(_connection);

        scope.HasActiveTransaction.Should().BeFalse();
        scope.CurrentTransaction.Should().BeNull();
    }

    [Fact]
    public void WhenTransactionCommittedThenCurrentTransactionIsNull()
    {
        var scope = new DataDbConnectionScope(_connection);

		IDataTransaction transaction = scope.BeginTransaction();
        transaction.Commit();

        scope.CurrentTransaction.Should().BeNull();
        scope.HasActiveTransaction.Should().BeFalse();
    }
}
