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
using System.Data.Common;

namespace System.Data;

/// <summary>
/// Provides a default implementation of <see cref="IDataConnectionScope"/> that manages the 
/// lifecycle of a database connection and its associated transaction.
/// </summary>
/// <remarks>
/// <para>
/// The scope takes ownership of the connection and will dispose it when the scope is disposed.
/// Only one transaction can be active at a time within a scope.
/// </para>
/// </remarks>
/// <param name="connection">The database connection to manage. May be open or closed;
/// if closed, it will be opened lazily on first use via <see cref="EnsureOpenAsync"/>.</param>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="connection"/> is null.</exception>
public sealed class DataConnectionScope(DbConnection connection) : IDataConnectionScope
{
	private readonly DbConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));
	private DataTransaction? _currentTransaction;
	private bool _isDisposed;
	private readonly SemaphoreSlim _openLock = new(1, 1);

	/// <inheritdoc />
	public DbConnection Connection
	{
		get
		{
			ObjectDisposedException.ThrowIf(_isDisposed, this);
			return _connection;
		}
	}

	/// <inheritdoc />
	/// <remarks>
	/// This implementation uses a <see cref="SemaphoreSlim"/> to ensure only one
	/// concurrent caller opens the connection, making it safe for use from
	/// multiple async code paths.
	/// </remarks>
	public async ValueTask EnsureOpenAsync(CancellationToken cancellationToken = default)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		if (_connection.State is ConnectionState.Open
			or ConnectionState.Connecting
			or ConnectionState.Executing
			or ConnectionState.Fetching)
		{
			return;
		}

		await _openLock.WaitAsync(cancellationToken).ConfigureAwait(false);

		try
		{
			if (_connection.State is ConnectionState.Broken)
			{
				await _connection.CloseAsync().ConfigureAwait(false);
			}

			await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			_openLock.Release();
		}
	}

	/// <inheritdoc/>
	/// <remarks>
	/// This implementation uses a <see cref="SemaphoreSlim"/> to ensure only one
	/// concurrent caller opens the connection, making it safe for use from
	/// multiple async code paths.
	/// </remarks>/// 
	public void EnsureOpen()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		if (_connection.State is ConnectionState.Open
			or ConnectionState.Connecting
			or ConnectionState.Executing
			or ConnectionState.Fetching)
		{
			return;
		}

		_openLock.Wait();

		try
		{
			if (_connection.State is ConnectionState.Broken)
			{
				_connection.Close();
			}

			_connection.Open();
		}
		finally
		{
			_openLock.Release();
		}
	}

	/// <inheritdoc />
	public IDataTransaction? CurrentTransaction => _currentTransaction;

	/// <inheritdoc />
	public bool HasActiveTransaction => _currentTransaction is { IsCompleted: false };

	/// <inheritdoc />
	public async Task<IDataTransaction> BeginTransactionAsync(
		IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
		CancellationToken cancellationToken = default)
	{
		ThrowIfDisposed();
		ThrowIfTransactionActive();

		await EnsureOpenAsync(cancellationToken).ConfigureAwait(false);

		DbTransaction transaction = await _connection
			.BeginTransactionAsync(isolationLevel, cancellationToken)
			.ConfigureAwait(false);

		_currentTransaction = new DataTransaction(transaction, OnTransactionCompleted);
		return _currentTransaction;
	}

	/// <inheritdoc />
	public IDataTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
	{
		ThrowIfDisposed();
		ThrowIfTransactionActive();

		DbTransaction transaction = _connection.BeginTransaction(isolationLevel);
		_currentTransaction = new DataTransaction(transaction, OnTransactionCompleted);
		return _currentTransaction;
	}

	/// <inheritdoc />
	public DbCommand CreateCommand()
	{
		ThrowIfDisposed();

		DbCommand command = _connection.CreateCommand();

		if (_currentTransaction is { IsCompleted: false })
		{
			command.Transaction = _currentTransaction.DbTransaction;
		}

		return command;
	}

	/// <inheritdoc/>
	[Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Pending>")]
	public DbCommand CreateCommand(SqlQueryResult queryResult)
	{
		DbCommand command = CreateCommand();
		command.CommandText = queryResult.Sql;
		queryResult.ApplyParameters(command);
		return command;
	}

	/// <inheritdoc />
	public void Dispose()
	{
		if (_isDisposed)
		{
			return;
		}

		_isDisposed = true;

		// Dispose the current transaction first
		if (_currentTransaction is IDisposable disposableTransaction)
		{
			disposableTransaction.Dispose();
		}

		_currentTransaction = null;
		_connection.Dispose();
		_openLock.Dispose();
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		if (_isDisposed)
		{
			return;
		}

		_isDisposed = true;

		// Dispose the current transaction first
		if (_currentTransaction is IAsyncDisposable asyncDisposableTransaction)
		{
			await asyncDisposableTransaction.DisposeAsync().ConfigureAwait(false);
		}
		else if (_currentTransaction is IDisposable disposableTransaction)
		{
			disposableTransaction.Dispose();
		}

		_currentTransaction = null;
		await _connection.DisposeAsync().ConfigureAwait(false);
		_openLock.Dispose();
	}

	private void ThrowIfDisposed()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);
	}

	private void ThrowIfTransactionActive()
	{
		if (HasActiveTransaction)
		{
			throw new InvalidOperationException(
				"A transaction is already active in this scope. " +
				"Complete or dispose the current transaction before starting a new one.");
		}
	}

	private void OnTransactionCompleted()
	{
		// Clear the current transaction reference when it's completed
		// This allows starting a new transaction in the same scope
		_currentTransaction = null;
	}
}
