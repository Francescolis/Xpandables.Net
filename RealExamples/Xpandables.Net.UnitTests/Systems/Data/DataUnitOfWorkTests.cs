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

namespace Xpandables.Net.UnitTests.Systems.Data;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class DataUnitOfWorkTests : IDisposable
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
	private readonly SqliteConnection _connection;
	private readonly IDataSqlServiceAccessor _dataSqlService = new TestDataSqlServiceAccessor(new MsDataSqlBuilder(), new DataSqlMapper());

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public DataUnitOfWorkTests()
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
	public void WhenGetRepositoryThenReturnsSameInstanceForSameType()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		using var scope = new DataConnectionScope(_connection);
		using var uow = new DataUnitOfWork(new TestScopeFactory(scope), _dataSqlService);

		IDataRepository<Person> repo1 = uow.GetRepository<Person>();
		IDataRepository<Person> repo2 = uow.GetRepository<Person>();

		repo1.Should().BeSameAs(repo2);
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenGetRepositoryForDifferentTypesThenReturnsDifferentInstances()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		using var scope = new DataConnectionScope(_connection);
		using var uow = new DataUnitOfWork(new TestScopeFactory(scope), _dataSqlService);

		IDataRepository<Person> personRepo = uow.GetRepository<Person>();
		IDataRepository<Address> addressRepo = uow.GetRepository<Address>();

		personRepo.Should().NotBeSameAs(addressRepo);
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenDisposedThenGetRepositoryThrowsObjectDisposedException()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var scope = new DataConnectionScope(_connection);
		var uow = new DataUnitOfWork(new TestScopeFactory(scope), _dataSqlService);

		uow.Dispose();

		Func<IDataRepository<Person>> act = () => uow.GetRepository<Person>();
		act.Should().Throw<ObjectDisposedException>();
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenDisposedThenConnectionScopeThrowsObjectDisposedException()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var scope = new DataConnectionScope(_connection);
		var uow = new DataUnitOfWork(new TestScopeFactory(scope), _dataSqlService);

		uow.Dispose();

		Func<IDataConnectionScope> act = () => _ = uow.ConnectionScope;
		act.Should().Throw<ObjectDisposedException>();
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenDisposedThenCurrentTransactionThrowsObjectDisposedException()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var scope = new DataConnectionScope(_connection);
		var uow = new DataUnitOfWork(new TestScopeFactory(scope), _dataSqlService);

		uow.Dispose();

		Func<IDataTransaction> act = () => _ = uow.CurrentTransaction!;
		act.Should().Throw<ObjectDisposedException>();
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenDisposedThenHasActiveTransactionThrowsObjectDisposedException()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var scope = new DataConnectionScope(_connection);
		var uow = new DataUnitOfWork(new TestScopeFactory(scope), _dataSqlService);

		uow.Dispose();

		Func<bool> act = () => _ = uow.HasActiveTransaction;
		act.Should().Throw<ObjectDisposedException>();
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenBeginTransactionThenDelegatesToScope()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		using var scope = new DataConnectionScope(_connection);
		using var uow = new DataUnitOfWork(new TestScopeFactory(scope), _dataSqlService);

		IDataTransaction transaction = uow.BeginTransaction();

		transaction.Should().NotBeNull();
		uow.HasActiveTransaction.Should().BeTrue();
		uow.CurrentTransaction.Should().BeSameAs(transaction);
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public async Task WhenBeginTransactionAsyncThenDelegatesToScope()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		using var scope = new DataConnectionScope(_connection);
		using var uow = new DataUnitOfWork(new TestScopeFactory(scope), _dataSqlService);

		IDataTransaction transaction = await uow.BeginTransactionAsync();

		transaction.Should().NotBeNull();
		uow.HasActiveTransaction.Should().BeTrue();
		uow.CurrentTransaction.Should().BeSameAs(transaction);
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenNoTransactionThenHasActiveTransactionIsFalse()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		using var scope = new DataConnectionScope(_connection);
		using var uow = new DataUnitOfWork(new TestScopeFactory(scope), _dataSqlService);

		uow.HasActiveTransaction.Should().BeFalse();
		uow.CurrentTransaction.Should().BeNull();
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public async Task WhenDisposeAsyncThenDisposesScope()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var scope = new DataConnectionScope(_connection);
		var uow = new DataUnitOfWork(new TestScopeFactory(scope), _dataSqlService);

		await uow.DisposeAsync();

		Func<DbConnection> act = () => _ = scope.Connection;
		act.Should().Throw<ObjectDisposedException>();
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenDisposedThenBeginTransactionThrowsObjectDisposedException()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var scope = new DataConnectionScope(_connection);
		var uow = new DataUnitOfWork(new TestScopeFactory(scope), _dataSqlService);

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

	private sealed class TestScopeFactory(IDataConnectionScope scope) : IDataConnectionScopeFactory
	{
		public IDataConnectionScope CreateOpenScope()
		{
			return scope;
		}

		public Task<IDataConnectionScope> CreateOpenScopeAsync(CancellationToken cancellationToken = default)
		{
			return Task.FromResult(scope);
		}

		public IDataConnectionScope CreateScope() => scope;

		public Task<IDataConnectionScope> CreateScopeAsync(CancellationToken cancellationToken = default)
			=> Task.FromResult(scope);
	}

	private sealed class TestDataSqlServiceAccessor(IDataSqlBuilder dataSqlBuilder, IDataSqlMapper dataSqlMapper) : IDataSqlServiceAccessor
	{
		public IDataSqlBuilder DataSqlBuilder { get; } = dataSqlBuilder;
		public IDataSqlMapper DataSqlMapper { get; } = dataSqlMapper;
	}
}
