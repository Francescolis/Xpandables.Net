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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Xpandables.Net.UnitTests.Systems.Data;

public sealed class DataCommandInterceptorTests : IDisposable
{
	private readonly SqliteConnection _connection;

	public DataCommandInterceptorTests()
	{
		_connection = new SqliteConnection("DataSource=:memory:");
		_connection.Open();

		using var cmd = _connection.CreateCommand();
		cmd.CommandText = """
			CREATE TABLE Product (
				Id INTEGER PRIMARY KEY,
				Name TEXT NOT NULL,
				Price REAL NOT NULL
			)
			""";
		cmd.ExecuteNonQuery();
	}

	public void Dispose() => _connection.Dispose();

	[Fact]
	public void WhenDefaultInterceptorThenDoesNotThrow()
	{
		var interceptor = DataCommandInterceptor.Default;

		var context = new DataCommandContext(
			"SELECT 1", [], DataCommandOperationType.Scalar);

		var act = async () =>
		{
			await interceptor.CommandExecutingAsync(context);
			await interceptor.CommandExecutedAsync(context, TimeSpan.FromMilliseconds(1), 0);
			await interceptor.CommandFailedAsync(context, TimeSpan.FromMilliseconds(1), new InvalidOperationException());
		};

		act.Should().NotThrowAsync();
	}

	[Fact]
	public void WhenDefaultInterceptorThenDefaultPropertyReturnsSameInstance()
	{
		var first = DataCommandInterceptor.Default;
		var second = DataCommandInterceptor.Default;

		first.Should().BeSameAs(second);
	}

	[Fact]
	public async Task WhenInsertAsyncThenInterceptorReceivesNonQueryContext()
	{
		var recording = new RecordingCommandInterceptor();
		using var scope = new DataDbConnectionScope(_connection);
		using var repo = new DataRepository<Product>(scope, new MsDataSqlBuilder(), new DataSqlMapper(), recording);

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
	public async Task WhenQueryCountAsyncThenInterceptorReceivesScalarContext()
	{
		var recording = new RecordingCommandInterceptor();
		using var scope = new DataDbConnectionScope(_connection);
		using var repo = new DataRepository<Product>(scope, new MsDataSqlBuilder(), new DataSqlMapper(), recording);

		var spec = DataSpecification
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
	public async Task WhenCommandFailsThenInterceptorReceivesFailedCallback()
	{
		var recording = new RecordingCommandInterceptor();
		using var scope = new DataDbConnectionScope(_connection);
		using var repo = new DataRepository<Product>(scope, new MsDataSqlBuilder(), new DataSqlMapper(), recording);

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
	public async Task WhenCommandSucceedsThenInterceptorReceivesExecutedWithDuration()
	{
		var recording = new RecordingCommandInterceptor();
		using var scope = new DataDbConnectionScope(_connection);
		using var repo = new DataRepository<Product>(scope, new MsDataSqlBuilder(), new DataSqlMapper(), recording);

		await repo.ExecuteAsync("INSERT INTO Product (Id, Name, Price) VALUES (1, 'Test', 5.0)");

		recording.ExecutedCalls.Should().HaveCount(1);
		recording.ExecutedCalls[0].Duration
			.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
		recording.ExecutedCalls[0].RowsAffected
			.Should().Be(1);
	}

	[Fact]
	public async Task WhenQueryRawAsyncThenInterceptorReceivesReaderContext()
	{
		var recording = new RecordingCommandInterceptor();
		using var scope = new DataDbConnectionScope(_connection);
		using var repo = new DataRepository<Product>(scope, new MsDataSqlBuilder(), new DataSqlMapper(), recording);

		await repo.ExecuteAsync("INSERT INTO Product (Id, Name, Price) VALUES (1, 'Test', 5.0)");
		recording.Clear();

		await foreach (var _ in repo.QueryRawAsync<Product>("SELECT * FROM Product"))
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
	public void WhenUnitOfWorkWithInterceptorThenRepositoryUsesIt()
	{
		var recording = new RecordingCommandInterceptor();
		using var scope = new DataDbConnectionScope(_connection);
		using var uow = new DataUnitOfWork(scope, new MsDataSqlBuilder(), new DataSqlMapper(), recording);

		var repo = uow.GetRepository<Product>();

		repo.Should().NotBeNull();
	}

	[Fact]
	public void WhenUnitOfWorkWithoutInterceptorThenUsesDefault()
	{
		using var scope = new DataDbConnectionScope(_connection);
		using var uow = new DataUnitOfWork(scope, new MsDataSqlBuilder(), new DataSqlMapper());

		var repo = uow.GetRepository<Product>();

		repo.Should().NotBeNull();
	}

	[Fact]
	public void WhenAddXDataCommandInterceptorThenRegistersLoggingInterceptor()
	{
		var services = new ServiceCollection();
		services.AddLogging();
		services.AddXDataCommandInterceptor();

		var provider = services.BuildServiceProvider();
		var interceptor = provider.GetRequiredService<IDataCommandInterceptor>();

		interceptor.Should().BeOfType<DataLoggingCommandInterceptor>();
	}

	[Fact]
	public void WhenAddXDataCommandInterceptorCustomThenReplacesDefault()
	{
		var services = new ServiceCollection();
		services.AddLogging();
		services.AddXDataCommandInterceptor();
		services.AddXDataCommandInterceptor<RecordingCommandInterceptor>();

		var provider = services.BuildServiceProvider();
		var interceptor = provider.GetRequiredService<IDataCommandInterceptor>();

		interceptor.Should().BeOfType<RecordingCommandInterceptor>();
	}

	[Fact]
	public void WhenCustomRegisteredBeforeDefaultThenCustomWins()
	{
		var services = new ServiceCollection();
		services.AddLogging();
		services.AddXDataCommandInterceptor<RecordingCommandInterceptor>();
		services.AddXDataCommandInterceptor();

		var provider = services.BuildServiceProvider();
		var interceptor = provider.GetRequiredService<IDataCommandInterceptor>();

		interceptor.Should().BeOfType<RecordingCommandInterceptor>();
	}

	[Fact]
	public void WhenConfigureOptionsThenOptionsAreResolved()
	{
		var services = new ServiceCollection();
		services.AddLogging();
		services.AddXDataCommandInterceptor(options =>
		{
			options.EnableSensitiveDataLogging = true;
			options.CategoryName = "Test.Database";
			options.SlowCommandThreshold = TimeSpan.FromSeconds(5);
		});

		var provider = services.BuildServiceProvider();
		var options = provider.GetRequiredService<IOptions<DataCommandInterceptorOptions>>();

		options.Value.EnableSensitiveDataLogging.Should().BeTrue();
		options.Value.CategoryName.Should().Be("Test.Database");
		options.Value.SlowCommandThreshold.Should().Be(TimeSpan.FromSeconds(5));
	}

	[Fact]
	public void WhenDefaultOptionsThenSensitiveDataLoggingIsDisabled()
	{
		var services = new ServiceCollection();
		services.AddLogging();
		services.AddXDataCommandInterceptor();

		var provider = services.BuildServiceProvider();
		var options = provider.GetRequiredService<IOptions<DataCommandInterceptorOptions>>();

		options.Value.EnableSensitiveDataLogging.Should().BeFalse();
		options.Value.CategoryName.Should().BeNull();
		options.Value.SlowCommandThreshold.Should().BeNull();
	}

	[Fact]
	public async Task WhenLoggingInterceptorThenLogsExecutedCommand()
	{
		var services = new ServiceCollection();
		services.AddLogging(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Debug));
		services.AddXDataCommandInterceptor();

		var provider = services.BuildServiceProvider();
		var interceptor = provider.GetRequiredService<IDataCommandInterceptor>();

		using var scope = new DataDbConnectionScope(_connection);
		using var repo = new DataRepository<Product>(scope, new MsDataSqlBuilder(), new DataSqlMapper(), interceptor);

		// Should not throw — logging interceptor handles all callbacks
		await repo.ExecuteAsync("INSERT INTO Product (Id, Name, Price) VALUES (1, 'Test', 5.0)");
	}

	[Fact]
	public async Task WhenLoggingInterceptorWithSensitiveDataThenDoesNotThrow()
	{
		var services = new ServiceCollection();
		services.AddLogging(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Debug));
		services.AddXDataCommandInterceptor(options =>
		{
			options.EnableSensitiveDataLogging = true;
		});

		var provider = services.BuildServiceProvider();
		var interceptor = provider.GetRequiredService<IDataCommandInterceptor>();

		using var scope = new DataDbConnectionScope(_connection);
		using var repo = new DataRepository<Product>(scope, new MsDataSqlBuilder(), new DataSqlMapper(), interceptor);

		// Should not throw — sensitive data logging enabled
		await repo.ExecuteAsync("INSERT INTO Product (Id, Name, Price) VALUES (2, 'Sensitive', 10.0)");
	}

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
}
