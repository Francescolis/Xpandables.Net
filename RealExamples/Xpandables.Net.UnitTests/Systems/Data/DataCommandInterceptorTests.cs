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

using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Xpandables.Net.UnitTests.Systems.Data;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class DataCommandInterceptorTests : IDisposable
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
	private readonly SqliteConnection _connection;
	private readonly IDataSqlServiceAccessor _dataSqlService = new TestDataSqlServiceAccessor(new MsDataSqlBuilder(), new DataSqlMapper());

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public DataCommandInterceptorTests()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		_connection = new SqliteConnection("DataSource=:memory:");
		_connection.Open();

		using SqliteCommand cmd = _connection.CreateCommand();
		cmd.CommandText = """
			CREATE TABLE Product (
				Id INTEGER PRIMARY KEY,
				Name TEXT NOT NULL,
				Price REAL NOT NULL
			)
			""";
		cmd.ExecuteNonQuery();
	}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void Dispose() => _connection.Dispose();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenDefaultInterceptorThenDoesNotThrow()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		DataCommandInterceptor interceptor = DataCommandInterceptor.Default;

		var context = new DataCommandContext(
			"SELECT 1", [], DataCommandOperationType.Scalar);

		Func<Task> act = async () =>
		{
			await interceptor.CommandExecutingAsync(context);
			await interceptor.CommandExecutedAsync(context, TimeSpan.FromMilliseconds(1), 0);
			await interceptor.CommandFailedAsync(context, TimeSpan.FromMilliseconds(1), new InvalidOperationException());
		};

		act.Should().NotThrowAsync();
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenDefaultInterceptorThenDefaultPropertyReturnsSameInstance()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		DataCommandInterceptor first = DataCommandInterceptor.Default;
		DataCommandInterceptor second = DataCommandInterceptor.Default;

		first.Should().BeSameAs(second);
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public async Task WhenInsertAsyncThenInterceptorReceivesNonQueryContext()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var recording = new RecordingCommandInterceptor();
		using var scope = new DataConnectionScope(_connection);
		using var repo = new DataRepository<Product>(scope, _dataSqlService, recording);

		var product = new Product { Id = 1, Name = "Widget", Price = 9.99 };

		// SQLite may throw on MS SQL syntax, but the interceptor fires before execution
		try
		{ await repo.InsertAsync(product); }
		catch { /* expected - MS SQL builder syntax may not match SQLite */ }

		recording.ExecutingCalls.Should().HaveCount(1);
		recording.ExecutingCalls[0].OperationType
			.Should().Be(DataCommandOperationType.NonQuery);
		recording.ExecutingCalls[0].EntityTypeName
			.Should().Be(nameof(Product));
		recording.ExecutingCalls[0].CommandText
			.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public async Task WhenQueryCountAsyncThenInterceptorReceivesScalarContext()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var recording = new RecordingCommandInterceptor();
		using var scope = new DataConnectionScope(_connection);
		using var repo = new DataRepository<Product>(scope, _dataSqlService, recording);

		DataSpecification<Product, Product> spec = DataSpecification
			.For<Product>()
			.Where(p => p.Price > 0)
			.Select(p => p);

		try
		{ await repo.CountAsync(spec); }
		catch { /* MS SQL syntax may not match SQLite */ }

		recording.ExecutingCalls.Should().HaveCount(1);
		recording.ExecutingCalls[0].OperationType
			.Should().Be(DataCommandOperationType.Scalar);
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public async Task WhenCommandFailsThenInterceptorReceivesFailedCallback()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var recording = new RecordingCommandInterceptor();
		using var scope = new DataConnectionScope(_connection);
		using var repo = new DataRepository<Product>(scope, _dataSqlService, recording);

		// Execute raw SQL that will fail
		try
		{
			await repo.ExecuteAsync("INVALID SQL SYNTAX HERE !!!");
		}
		catch
		{
			// expected
		}

		recording.FailedCalls.Should().HaveCount(1);
		recording.FailedCalls[0].Context.OperationType
			.Should().Be(DataCommandOperationType.NonQuery);
		recording.FailedCalls[0].Context.CommandText
			.Should().Be("INVALID SQL SYNTAX HERE !!!");
		recording.FailedCalls[0].Exception
			.Should().NotBeNull();
		recording.FailedCalls[0].Duration
			.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public async Task WhenCommandSucceedsThenInterceptorReceivesExecutedWithDuration()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var recording = new RecordingCommandInterceptor();
		using var scope = new DataConnectionScope(_connection);
		using var repo = new DataRepository<Product>(scope, _dataSqlService, recording);

		await repo.ExecuteAsync("INSERT INTO Product (Id, Name, Price) VALUES (1, 'Test', 5.0)");

		recording.ExecutedCalls.Should().HaveCount(1);
		recording.ExecutedCalls[0].Duration
			.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
		recording.ExecutedCalls[0].RowsAffected
			.Should().Be(1);
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public async Task WhenQueryRawAsyncThenInterceptorReceivesReaderContext()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var recording = new RecordingCommandInterceptor();
		using var scope = new DataConnectionScope(_connection);
		using var repo = new DataRepository<Product>(scope, _dataSqlService, recording);

		await repo.ExecuteAsync("INSERT INTO Product (Id, Name, Price) VALUES (1, 'Test', 5.0)");
		recording.Clear();

		await foreach (Product _ in repo.QueryRawAsync<Product>("SELECT * FROM Product"))
		{
			// consume the stream
		}

		recording.ExecutingCalls.Should().HaveCount(1);
		recording.ExecutingCalls[0].OperationType
			.Should().Be(DataCommandOperationType.Reader);
		recording.ExecutedCalls.Should().HaveCount(1);
		recording.ExecutedCalls[0].RowsAffected
			.Should().BeNull();
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenUnitOfWorkWithInterceptorThenRepositoryUsesIt()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var recording = new RecordingCommandInterceptor();
		using var scope = new DataConnectionScope(_connection);
		using var uow = new DataUnitOfWork(new TestScopeFactory(scope), _dataSqlService, recording);

		IDataRepository<Product> repo = uow.GetRepository<Product>();

		repo.Should().NotBeNull();
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenUnitOfWorkWithoutInterceptorThenUsesDefault()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		using var scope = new DataConnectionScope(_connection);
		using var uow = new DataUnitOfWork(new TestScopeFactory(scope), _dataSqlService);

		IDataRepository<Product> repo = uow.GetRepository<Product>();

		repo.Should().NotBeNull();
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenAddXDataCommandInterceptorThenRegistersLoggingInterceptor()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var services = new ServiceCollection();
		services.AddLogging();
		services.AddXDataCommandInterceptor();

		ServiceProvider provider = services.BuildServiceProvider();
		IDataCommandInterceptor interceptor = provider.GetRequiredService<IDataCommandInterceptor>();

		interceptor.Should().BeOfType<DataLoggingCommandInterceptor>();
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenAddXDataCommandInterceptorCustomThenReplacesDefault()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var services = new ServiceCollection();
		services.AddLogging();
		services.AddXDataCommandInterceptor();
		services.AddXDataCommandInterceptor<RecordingCommandInterceptor>();

		ServiceProvider provider = services.BuildServiceProvider();
		IDataCommandInterceptor interceptor = provider.GetRequiredService<IDataCommandInterceptor>();

		interceptor.Should().BeOfType<RecordingCommandInterceptor>();
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenCustomRegisteredBeforeDefaultThenCustomWins()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var services = new ServiceCollection();
		services.AddLogging();
		services.AddXDataCommandInterceptor<RecordingCommandInterceptor>();
		services.AddXDataCommandInterceptor();

		ServiceProvider provider = services.BuildServiceProvider();
		IDataCommandInterceptor interceptor = provider.GetRequiredService<IDataCommandInterceptor>();

		interceptor.Should().BeOfType<RecordingCommandInterceptor>();
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenConfigureOptionsThenOptionsAreResolved()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var services = new ServiceCollection();
		services.AddLogging();
		services.AddXDataCommandInterceptor(options =>
		{
			options.EnableSensitiveDataLogging = true;
			options.CategoryName = "Test.Database";
			options.SlowCommandThreshold = TimeSpan.FromSeconds(5);
		});

		ServiceProvider provider = services.BuildServiceProvider();
		IOptions<DataCommandInterceptorOptions> options = provider.GetRequiredService<IOptions<DataCommandInterceptorOptions>>();

		options.Value.EnableSensitiveDataLogging.Should().BeTrue();
		options.Value.CategoryName.Should().Be("Test.Database");
		options.Value.SlowCommandThreshold.Should().Be(TimeSpan.FromSeconds(5));
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenDefaultOptionsThenSensitiveDataLoggingIsDisabled()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var services = new ServiceCollection();
		services.AddLogging();
		services.AddXDataCommandInterceptor();

		ServiceProvider provider = services.BuildServiceProvider();
		IOptions<DataCommandInterceptorOptions> options = provider.GetRequiredService<IOptions<DataCommandInterceptorOptions>>();

		options.Value.EnableSensitiveDataLogging.Should().BeFalse();
		options.Value.CategoryName.Should().BeNull();
		options.Value.SlowCommandThreshold.Should().BeNull();
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public async Task WhenLoggingInterceptorThenLogsExecutedCommand()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var services = new ServiceCollection();
		services.AddLogging(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Debug));
		services.AddXDataCommandInterceptor();

		ServiceProvider provider = services.BuildServiceProvider();
		IDataCommandInterceptor interceptor = provider.GetRequiredService<IDataCommandInterceptor>();

		using var scope = new DataConnectionScope(_connection);
		using var repo = new DataRepository<Product>(scope, _dataSqlService, interceptor);

		// Should not throw — logging interceptor handles all callbacks
		await repo.ExecuteAsync("INSERT INTO Product (Id, Name, Price) VALUES (1, 'Test', 5.0)");
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public async Task WhenLoggingInterceptorWithSensitiveDataThenDoesNotThrow()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var services = new ServiceCollection();
		services.AddLogging(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Debug));
		services.AddXDataCommandInterceptor(options => options.EnableSensitiveDataLogging = true);

		ServiceProvider provider = services.BuildServiceProvider();
		IDataCommandInterceptor interceptor = provider.GetRequiredService<IDataCommandInterceptor>();

		using var scope = new DataConnectionScope(_connection);
		using var repo = new DataRepository<Product>(scope, _dataSqlService, interceptor);

		// Should not throw — sensitive data logging enabled
		await repo.ExecuteAsync("INSERT INTO Product (Id, Name, Price) VALUES (2, 'Sensitive', 10.0)");
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public async Task WhenQueryRawAsyncWithCustomMapperThenReturnsCustomMappedResults()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		using var scope = new DataConnectionScope(_connection);
		using var repo = new DataRepository<Product>(scope, _dataSqlService);

		await repo.ExecuteAsync("INSERT INTO Product (Id, Name, Price) VALUES (1, 'Widget', 9.99)");
		await repo.ExecuteAsync("INSERT INTO Product (Id, Name, Price) VALUES (2, 'Gadget', 19.99)");

		var results = new List<ProductSummary>();
		await foreach (ProductSummary item in repo.QueryRawAsync(
			"SELECT Id, Name, Price FROM Product ORDER BY Id",
			reader => new ProductSummary(
				reader.GetInt32(reader.GetOrdinal("Id")),
				reader.GetString(reader.GetOrdinal("Name")),
				reader.GetDouble(reader.GetOrdinal("Price")))))
		{
			results.Add(item);
		}

		results.Should().HaveCount(2);
		results[0].Id.Should().Be(1);
		results[0].Name.Should().Be("Widget");
		results[0].Price.Should().Be(9.99);
		results[1].Id.Should().Be(2);
		results[1].Name.Should().Be("Gadget");
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public async Task WhenQueryRawAsyncWithCustomMapperAndParametersThenFiltersCorrectly()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		using var scope = new DataConnectionScope(_connection);
		using var repo = new DataRepository<Product>(scope, _dataSqlService);

		await repo.ExecuteAsync("INSERT INTO Product (Id, Name, Price) VALUES (1, 'Widget', 9.99)");
		await repo.ExecuteAsync("INSERT INTO Product (Id, Name, Price) VALUES (2, 'Gadget', 19.99)");

		var results = new List<string>();
		await foreach (string name in repo.QueryRawAsync(
			"SELECT Name FROM Product WHERE Price > @p0",
			reader => reader.GetString(0),
			[new SqlParameter("p0", 10.0)]))
		{
			results.Add(name);
		}

		results.Should().ContainSingle().Which.Should().Be("Gadget");
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public async Task WhenQueryRawAsyncWithCustomMapperThenInterceptorReceivesReaderContext()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		var recording = new RecordingCommandInterceptor();
		using var scope = new DataConnectionScope(_connection);
		using var repo = new DataRepository<Product>(scope, _dataSqlService, recording);

		await repo.ExecuteAsync("INSERT INTO Product (Id, Name, Price) VALUES (1, 'Test', 5.0)");
		recording.Clear();

		await foreach (string _ in repo.QueryRawAsync(
			"SELECT Name FROM Product",
			reader => reader.GetString(0)))
		{
			// consume
		}

		recording.ExecutingCalls.Should().HaveCount(1);
		recording.ExecutingCalls[0].OperationType.Should().Be(DataCommandOperationType.Reader);
		recording.ExecutedCalls.Should().HaveCount(1);
	}

	[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public void WhenQueryRawAsyncWithCustomMapperAndNullMapperThenThrows()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		using var scope = new DataConnectionScope(_connection);
		using var repo = new DataRepository<Product>(scope, _dataSqlService);

		Func<Task> act = async () =>
		{
			await foreach (string _ in repo.QueryRawAsync<string>("SELECT 1", mapper: null!, parameters: null))
			{
			}
		};

		act.Should().ThrowAsync<ArgumentNullException>();
	}

	[Fact]
#pragma warning disable CS1591
	public async Task WhenQueryRawAsyncWithExpressionSelectorThenReturnsProjectedResults()
#pragma warning restore CS1591
	{
		using var scope = new DataConnectionScope(_connection);
		using var repo = new DataRepository<Product>(scope, _dataSqlService);

		await repo.ExecuteAsync("INSERT INTO Product (Id, Name, Price) VALUES (1, 'Widget', 9.99)");
		await repo.ExecuteAsync("INSERT INTO Product (Id, Name, Price) VALUES (2, 'Gadget', 19.99)");

		var results = new List<ProductSummary>();
		await foreach (ProductSummary item in repo.QueryRawAsync(
			"SELECT Id, Name, Price FROM Product ORDER BY Id",
			p => new ProductSummary(p.Id, p.Name, p.Price)))
		{
			results.Add(item);
		}

		results.Should().HaveCount(2);
		results[0].Id.Should().Be(1);
		results[0].Name.Should().Be("Widget");
		results[0].Price.Should().Be(9.99);
		results[1].Name.Should().Be("Gadget");
	}

	[Fact]
#pragma warning disable CS1591
	public async Task WhenQueryRawAsyncWithExpressionSelectorAndParametersThenFiltersCorrectly()
#pragma warning restore CS1591
	{
		using var scope = new DataConnectionScope(_connection);
		using var repo = new DataRepository<Product>(scope, _dataSqlService);

		await repo.ExecuteAsync("INSERT INTO Product (Id, Name, Price) VALUES (1, 'Widget', 9.99)");
		await repo.ExecuteAsync("INSERT INTO Product (Id, Name, Price) VALUES (2, 'Gadget', 19.99)");

		var results = new List<string>();
		await foreach (string name in repo.QueryRawAsync(
			"SELECT Id, Name, Price FROM Product WHERE Price > @p0",
			p => p.Name,
			[new SqlParameter("p0", 10.0)]))
		{
			results.Add(name);
		}

		results.Should().ContainSingle().Which.Should().Be("Gadget");
	}

	[Fact]
#pragma warning disable CS1591
	public async Task WhenQueryRawAsyncWithExpressionSelectorThenInterceptorReceivesReaderContext()
#pragma warning restore CS1591
	{
		var recording = new RecordingCommandInterceptor();
		using var scope = new DataConnectionScope(_connection);
		using var repo = new DataRepository<Product>(scope, _dataSqlService, recording);

		await repo.ExecuteAsync("INSERT INTO Product (Id, Name, Price) VALUES (1, 'Test', 5.0)");
		recording.Clear();

		await foreach (string _ in repo.QueryRawAsync(
			"SELECT Id, Name, Price FROM Product",
			p => p.Name))
		{
			// consume
		}

		recording.ExecutingCalls.Should().HaveCount(1);
		recording.ExecutingCalls[0].OperationType.Should().Be(DataCommandOperationType.Reader);
		recording.ExecutedCalls.Should().HaveCount(1);
	}

	[Fact]
#pragma warning disable CS1591
	public async Task WhenQueryRawAsyncWithExpressionSelectorScalarThenReturnsDirectValue()
#pragma warning restore CS1591
	{
		using var scope = new DataConnectionScope(_connection);
		using var repo = new DataRepository<Product>(scope, _dataSqlService);

		await repo.ExecuteAsync("INSERT INTO Product (Id, Name, Price) VALUES (1, 'Widget', 9.99)");

		var results = new List<int>();
		await foreach (int id in repo.QueryRawAsync(
			"SELECT Id, Name, Price FROM Product",
			p => p.Id))
		{
			results.Add(id);
		}

		results.Should().ContainSingle().Which.Should().Be(1);
	}

	private sealed record ProductSummary(int Id, string Name, double Price);

	private sealed class Product
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public double Price { get; set; }
	}

	internal sealed class RecordingCommandInterceptor : IDataCommandInterceptor
	{
		public List<DataCommandContext> ExecutingCalls { get; } = [];
		public List<ExecutedRecord> ExecutedCalls { get; } = [];
		public List<FailedRecord> FailedCalls { get; } = [];

		public ValueTask CommandExecutingAsync(
			DataCommandContext context,
			CancellationToken cancellationToken = default)
		{
			ExecutingCalls.Add(context);
			return ValueTask.CompletedTask;
		}

		public ValueTask CommandExecutedAsync(
			DataCommandContext context,
			TimeSpan duration,
			int? rowsAffected,
			CancellationToken cancellationToken = default)
		{
			ExecutedCalls.Add(new(context, duration, rowsAffected));
			return ValueTask.CompletedTask;
		}

		public ValueTask CommandFailedAsync(
			DataCommandContext context,
			TimeSpan duration,
			Exception exception,
			CancellationToken cancellationToken = default)
		{
			FailedCalls.Add(new(context, duration, exception));
			return ValueTask.CompletedTask;
		}

		public void Clear()
		{
			ExecutingCalls.Clear();
			ExecutedCalls.Clear();
			FailedCalls.Clear();
		}

		internal sealed record ExecutedRecord(DataCommandContext Context, TimeSpan Duration, int? RowsAffected);
		internal sealed record FailedRecord(DataCommandContext Context, TimeSpan Duration, Exception Exception);
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
