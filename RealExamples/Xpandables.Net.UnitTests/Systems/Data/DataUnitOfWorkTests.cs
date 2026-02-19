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

public sealed class DataUnitOfWorkTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public DataUnitOfWorkTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public void Dispose() => _connection.Dispose();

    [Fact]
    public void WhenGetRepositoryThenReturnsSameInstanceForSameType()
    {
        using var scope = new DataDbConnectionScope(_connection);
        using var uow = new DataUnitOfWork(scope, new MsDataSqlBuilder(), new DataSqlMapper());

		IDataRepository<Person> repo1 = uow.GetRepository<Person>();
		IDataRepository<Person> repo2 = uow.GetRepository<Person>();

        repo1.Should().BeSameAs(repo2);
    }

    [Fact]
    public void WhenGetRepositoryForDifferentTypesThenReturnsDifferentInstances()
    {
        using var scope = new DataDbConnectionScope(_connection);
        using var uow = new DataUnitOfWork(scope, new MsDataSqlBuilder(), new DataSqlMapper());

		IDataRepository<Person> personRepo = uow.GetRepository<Person>();
		IDataRepository<Address> addressRepo = uow.GetRepository<Address>();

        personRepo.Should().NotBeSameAs(addressRepo);
    }

    [Fact]
    public void WhenDisposedThenGetRepositoryThrowsObjectDisposedException()
    {
        var scope = new DataDbConnectionScope(_connection);
        var uow = new DataUnitOfWork(scope, new MsDataSqlBuilder(), new DataSqlMapper());

        uow.Dispose();

		Func<IDataRepository<Person>> act = () => uow.GetRepository<Person>();
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void WhenDisposedThenConnectionScopeThrowsObjectDisposedException()
    {
        var scope = new DataDbConnectionScope(_connection);
        var uow = new DataUnitOfWork(scope, new MsDataSqlBuilder(), new DataSqlMapper());

        uow.Dispose();

		Func<IDataDbConnectionScope> act = () => _ = uow.ConnectionScope;
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void WhenDisposedThenCurrentTransactionThrowsObjectDisposedException()
    {
        var scope = new DataDbConnectionScope(_connection);
        var uow = new DataUnitOfWork(scope, new MsDataSqlBuilder(), new DataSqlMapper());

        uow.Dispose();

		Func<IDataTransaction> act = () => _ = uow.CurrentTransaction;
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void WhenDisposedThenHasActiveTransactionThrowsObjectDisposedException()
    {
        var scope = new DataDbConnectionScope(_connection);
        var uow = new DataUnitOfWork(scope, new MsDataSqlBuilder(), new DataSqlMapper());

        uow.Dispose();

		Func<bool> act = () => _ = uow.HasActiveTransaction;
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void WhenBeginTransactionThenDelegatesToScope()
    {
        using var scope = new DataDbConnectionScope(_connection);
        using var uow = new DataUnitOfWork(scope, new MsDataSqlBuilder(), new DataSqlMapper());

		IDataTransaction transaction = uow.BeginTransaction();

        transaction.Should().NotBeNull();
        uow.HasActiveTransaction.Should().BeTrue();
        uow.CurrentTransaction.Should().BeSameAs(transaction);
    }

    [Fact]
    public async Task WhenBeginTransactionAsyncThenDelegatesToScope()
    {
        using var scope = new DataDbConnectionScope(_connection);
        using var uow = new DataUnitOfWork(scope, new MsDataSqlBuilder(), new DataSqlMapper());

		IDataTransaction transaction = await uow.BeginTransactionAsync();

        transaction.Should().NotBeNull();
        uow.HasActiveTransaction.Should().BeTrue();
        uow.CurrentTransaction.Should().BeSameAs(transaction);
    }

    [Fact]
    public void WhenNoTransactionThenHasActiveTransactionIsFalse()
    {
        using var scope = new DataDbConnectionScope(_connection);
        using var uow = new DataUnitOfWork(scope, new MsDataSqlBuilder(), new DataSqlMapper());

        uow.HasActiveTransaction.Should().BeFalse();
        uow.CurrentTransaction.Should().BeNull();
    }

    [Fact]
    public async Task WhenDisposeAsyncThenDisposesScope()
    {
        var scope = new DataDbConnectionScope(_connection);
        var uow = new DataUnitOfWork(scope, new MsDataSqlBuilder(), new DataSqlMapper());

        await uow.DisposeAsync();

		Func<DbConnection> act = () => _ = scope.Connection;
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void WhenDisposedThenBeginTransactionThrowsObjectDisposedException()
    {
        var scope = new DataDbConnectionScope(_connection);
        var uow = new DataUnitOfWork(scope, new MsDataSqlBuilder(), new DataSqlMapper());

        uow.Dispose();

		Func<IDataTransaction> act = () => uow.BeginTransaction();
        act.Should().Throw<ObjectDisposedException>();
    }

    private sealed class Person
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class Address
    {
        public int Id { get; set; }
        public string City { get; set; } = string.Empty;
    }
}
