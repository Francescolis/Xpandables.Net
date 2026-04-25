/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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

namespace Servitia.UnitTests.DataSql;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class DbConnectionScopeTests : IDisposable
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
	private readonly SqliteConnection _connection;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public DbConnectionScopeTests()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		_connection = new SqliteConnection("DataSource=:memory:");
		_connection.Open();
	}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void Dispose() => _connection.Dispose();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenBeginTransactionThenHasActiveTransactionIsTrue()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var scope = new DataConnectionScope(_connection);

		IDataTransaction transaction = scope.BeginTransaction();

		scope.HasActiveTransaction.Should().BeTrue();
		scope.CurrentTransaction.Should().BeSameAs(transaction);
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public async Task WhenBeginTransactionAsyncThenHasActiveTransactionIsTrue()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var scope = new DataConnectionScope(_connection);

		IDataTransaction transaction = await scope.BeginTransactionAsync();

		scope.HasActiveTransaction.Should().BeTrue();
		scope.CurrentTransaction.Should().BeSameAs(transaction);
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenTwoActiveTransactionsThenThrowsInvalidOperationException()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var scope = new DataConnectionScope(_connection);
		scope.BeginTransaction();

		Func<IDataTransaction> act = () => scope.BeginTransaction();

		act.Should().Throw<InvalidOperationException>()
			.WithMessage("*already active*");
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenTransactionCommittedThenNewTransactionAllowed()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var scope = new DataConnectionScope(_connection);

		IDataTransaction first = scope.BeginTransaction();
		first.Commit();

		IDataTransaction second = scope.BeginTransaction();
		second.Should().NotBeNull();
		scope.HasActiveTransaction.Should().BeTrue();
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenTransactionRolledBackThenNewTransactionAllowed()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var scope = new DataConnectionScope(_connection);

		IDataTransaction first = scope.BeginTransaction();
		first.Rollback();

		IDataTransaction second = scope.BeginTransaction();
		second.Should().NotBeNull();
		scope.HasActiveTransaction.Should().BeTrue();
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenCreateCommandWithActiveTransactionThenCommandHasTransaction()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var scope = new DataConnectionScope(_connection);
		IDataTransaction transaction = scope.BeginTransaction();

		DbCommand command = scope.CreateCommand();

		command.Transaction.Should().BeSameAs(transaction.DbTransaction);
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenCreateCommandWithoutTransactionThenCommandHasNoTransaction()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var scope = new DataConnectionScope(_connection);

		DbCommand command = scope.CreateCommand();

		command.Transaction.Should().BeNull();
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenCreateCommandWithQueryResultThenCommandHasSqlAndParameters()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var scope = new DataConnectionScope(_connection);
		var queryResult = new SqlQueryResult("SELECT 1 WHERE @p0 = 1", [new SqlParameter("p0", 1)]);

		DbCommand command = scope.CreateCommand(queryResult);

		command.CommandText.Should().Be("SELECT 1 WHERE @p0 = 1");
		command.Parameters.Count.Should().Be(1);
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenDisposedThenConnectionPropertyThrowsObjectDisposedException()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var scope = new DataConnectionScope(_connection);

		scope.Dispose();

		Func<DbConnection> act = () => _ = scope.Connection;
		act.Should().Throw<ObjectDisposedException>();
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenDisposedThenBeginTransactionThrowsObjectDisposedException()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var scope = new DataConnectionScope(_connection);

		scope.Dispose();

		Func<IDataTransaction> act = () => scope.BeginTransaction();
		act.Should().Throw<ObjectDisposedException>();
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenDisposedThenCreateCommandThrowsObjectDisposedException()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var scope = new DataConnectionScope(_connection);

		scope.Dispose();

		Func<DbCommand> act = () => scope.CreateCommand();
		act.Should().Throw<ObjectDisposedException>();
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenNoTransactionThenHasActiveTransactionIsFalse()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var scope = new DataConnectionScope(_connection);

		scope.HasActiveTransaction.Should().BeFalse();
		scope.CurrentTransaction.Should().BeNull();
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenTransactionCommittedThenCurrentTransactionIsNull()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var scope = new DataConnectionScope(_connection);

		IDataTransaction transaction = scope.BeginTransaction();
		transaction.Commit();

		scope.CurrentTransaction.Should().BeNull();
		scope.HasActiveTransaction.Should().BeFalse();
	}
}
