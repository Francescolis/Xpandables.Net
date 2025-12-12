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
using FluentAssertions;

namespace Xpandables.Net.UnitTests.Primitives;

public sealed class DisposableAsyncTests
{
    #region Test Disposables

    private sealed class TestDisposableAsync : DisposableAsync
    {
        public bool WasDisposedWithTrue { get; private set; }
        public bool WasDisposedWithFalse { get; private set; }
        public int DisposeCallCount { get; private set; }
        public bool IsDisposedPublic => IsDisposed;

        protected override async ValueTask DisposeAsync(bool disposing)
        {
            DisposeCallCount++;

            if (disposing)
            {
                WasDisposedWithTrue = true;
            }
            else
            {
                WasDisposedWithFalse = true;
            }

            await base.DisposeAsync(disposing).ConfigureAwait(false);
        }
    }

    private sealed class AsyncResourceHolder : DisposableAsync
    {
        public MemoryStream? ManagedStream { get; private set; }
        public bool ResourceReleased { get; private set; }
        public bool AsyncCleanupPerformed { get; private set; }
        public bool IsDisposedPublic => IsDisposed;

        public AsyncResourceHolder()
        {
            ManagedStream = new MemoryStream();
            ManagedStream.Write([1, 2, 3, 4, 5]);
        }

        protected override async ValueTask DisposeAsync(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                // Simulate async cleanup operation
                await Task.Delay(10).ConfigureAwait(false);
                AsyncCleanupPerformed = true;

                ManagedStream?.Dispose();
                ManagedStream = null;
                ResourceReleased = true;
            }

            await base.DisposeAsync(disposing).ConfigureAwait(false);
        }
    }

    private sealed class AsyncDatabaseConnection(string connectionString) : DisposableAsync
    {
        public string? ConnectionString { get; private set; } = connectionString;
        public bool IsConnected { get; private set; }
        public List<string> ExecutedCommands { get; } = [];
        public bool TransactionRolledBack { get; private set; }
        public bool ConnectionClosed { get; private set; }

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            await Task.Delay(10, cancellationToken).ConfigureAwait(false);
            IsConnected = true;
        }

        public async Task ExecuteAsync(string command, CancellationToken cancellationToken = default)
        {
            await Task.Delay(5, cancellationToken).ConfigureAwait(false);
            ExecutedCommands.Add(command);
        }

        protected override async ValueTask DisposeAsync(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                // Simulate async rollback and disconnect
                await Task.Delay(10).ConfigureAwait(false);
                TransactionRolledBack = true;

                await Task.Delay(10).ConfigureAwait(false);
                IsConnected = false;
                ConnectionClosed = true;
                ConnectionString = null;
            }

            await base.DisposeAsync(disposing).ConfigureAwait(false);
        }
    }

    private sealed class AsyncHttpClientWrapper : DisposableAsync
    {
        private HttpClient? _httpClient;
        public bool ClientDisposed { get; private set; }
        public List<string> RequestsMade { get; } = [];

        public AsyncHttpClientWrapper() => _httpClient = new HttpClient();

        public async Task<string> GetAsync(string url, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_httpClient is null, _httpClient);

            RequestsMade.Add(url);
            // Simulate a request without actually making one
            await Task.Delay(5, cancellationToken).ConfigureAwait(false);
            return $"Response from {url}";
        }

        protected override async ValueTask DisposeAsync(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                await Task.Delay(5).ConfigureAwait(false);
                _httpClient?.Dispose();
                _httpClient = null;
                ClientDisposed = true;
            }

            await base.DisposeAsync(disposing).ConfigureAwait(false);
        }
    }

    #endregion

    #region Basic Disposal Tests

    [Fact]
    public async Task WhenDisposingAsyncThenIsDisposedShouldBeTrue()
    {
        // Arrange
        var disposable = new TestDisposableAsync();

        // Act
        await disposable.DisposeAsync();

        // Assert
        disposable.IsDisposedPublic.Should().BeTrue();
    }

    [Fact]
    public async Task WhenDisposingAsyncThenDisposeAsyncWithTrueShouldBeCalled()
    {
        // Arrange
        var disposable = new TestDisposableAsync();

        // Act
        await disposable.DisposeAsync();

        // Assert
        disposable.WasDisposedWithTrue.Should().BeTrue();
        disposable.WasDisposedWithFalse.Should().BeFalse();
    }

    [Fact]
    public void WhenNotDisposedThenIsDisposedShouldBeFalse()
    {
        // Arrange
        var disposable = new TestDisposableAsync();

        // Assert
        disposable.IsDisposedPublic.Should().BeFalse();
    }

    #endregion

    #region Multiple Dispose Tests

    [Fact]
    public async Task WhenDisposingAsyncMultipleTimesThenShouldHandleGracefully()
    {
        // Arrange
        var disposable = new TestDisposableAsync();

        // Act
        await disposable.DisposeAsync();
        await disposable.DisposeAsync();
        await disposable.DisposeAsync();

        // Assert
        disposable.DisposeCallCount.Should().Be(3);
        disposable.IsDisposedPublic.Should().BeTrue();
    }

    [Fact]
    public async Task WhenDisposingAsyncMultipleTimesThenResourcesShouldNotBeReleasedTwice()
    {
        // Arrange
        var disposable = new AsyncResourceHolder();

        // Act
        await disposable.DisposeAsync();
        await disposable.DisposeAsync();

        // Assert
        disposable.ResourceReleased.Should().BeTrue();
        disposable.ManagedStream.Should().BeNull();
    }

    #endregion

    #region Await Using Tests

    [Fact]
    public async Task WhenUsingAwaitUsingThenShouldBeDisposed()
    {
        // Arrange
        TestDisposableAsync? disposable;

        // Act
        await using (disposable = new TestDisposableAsync())
        {
            disposable.IsDisposedPublic.Should().BeFalse();
        }

        // Assert
        disposable.IsDisposedPublic.Should().BeTrue();
    }

    [Fact]
    public async Task WhenUsingAwaitUsingDeclarationThenShouldBeDisposed()
    {
        // Arrange
        TestDisposableAsync? captured = null;

        // Act
        async Task TestMethodAsync()
        {
            await using var disposable = new TestDisposableAsync();
            captured = disposable;
            captured.IsDisposedPublic.Should().BeFalse();
        }

        await TestMethodAsync();

        // Assert
        captured.Should().NotBeNull();
        captured!.IsDisposedPublic.Should().BeTrue();
    }

    #endregion

    #region Async Resource Management Tests

    [Fact]
    public async Task WhenDisposingAsyncThenAsyncCleanupShouldBePerformed()
    {
        // Arrange
        var disposable = new AsyncResourceHolder();

        // Act
        await disposable.DisposeAsync();

        // Assert
        disposable.AsyncCleanupPerformed.Should().BeTrue();
        disposable.ResourceReleased.Should().BeTrue();
        disposable.ManagedStream.Should().BeNull();
    }

    [Fact]
    public async Task WhenAsyncResourceDisposedThenStreamShouldNotBeAccessible()
    {
        // Arrange
        var disposable = new AsyncResourceHolder();
        var stream = disposable.ManagedStream;

        // Act
        await disposable.DisposeAsync();

        // Assert
        var act = () => stream!.ReadByte();
        act.Should().Throw<ObjectDisposedException>();
    }

    #endregion

    #region Real World Scenario Tests

    [Fact]
    public async Task WhenDatabaseConnectionDisposedAsyncThenShouldCloseConnection()
    {
        // Arrange
        var connection = new AsyncDatabaseConnection("Server=test;Database=testdb");
        await connection.ConnectAsync();
        await connection.ExecuteAsync("SELECT * FROM Users");

        connection.IsConnected.Should().BeTrue();
        connection.ExecutedCommands.Should().Contain("SELECT * FROM Users");

        // Act
        await connection.DisposeAsync();

        // Assert
        connection.IsConnected.Should().BeFalse();
        connection.ConnectionClosed.Should().BeTrue();
        connection.TransactionRolledBack.Should().BeTrue();
        connection.ConnectionString.Should().BeNull();
    }

    [Fact]
    public async Task WhenHttpClientWrapperDisposedThenShouldReleaseClient()
    {
        // Arrange
        var wrapper = new AsyncHttpClientWrapper();
        var response = await wrapper.GetAsync("http://example.com/api/data");

        response.Should().Contain("example.com");
        wrapper.RequestsMade.Should().HaveCount(1);

        // Act
        await wrapper.DisposeAsync();

        // Assert
        wrapper.ClientDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task WhenHttpClientWrapperDisposedThenFurtherRequestsShouldThrow()
    {
        // Arrange
        var wrapper = new AsyncHttpClientWrapper();
        await wrapper.DisposeAsync();

        // Act
        var act = async () => await wrapper.GetAsync("http://example.com/api/data");

        // Assert
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task WhenUsingAsyncDatabaseConnectionInUsingBlockThenShouldBeDisposed()
    {
        // Arrange
        AsyncDatabaseConnection? captured = null;

        // Act
        await using (var connection = new AsyncDatabaseConnection("Server=test"))
        {
            captured = connection;
            await connection.ConnectAsync();
            await connection.ExecuteAsync("INSERT INTO Logs VALUES ('test')");
        }

        // Assert
        captured!.ConnectionClosed.Should().BeTrue();
        captured.IsConnected.Should().BeFalse();
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task WhenExceptionThrownInUsingBlockThenResourceShouldStillBeDisposed()
    {
        // Arrange
        TestDisposableAsync? disposable = null;

        // Act
        var act = async () =>
        {
            await using (disposable = new TestDisposableAsync())
            {
                throw new InvalidOperationException("Test exception");
            }
        };

        await act.Should().ThrowAsync<InvalidOperationException>();

        // Assert
        disposable.Should().NotBeNull();
        disposable!.IsDisposedPublic.Should().BeTrue();
    }

    #endregion

    #region Concurrent Disposal Tests

    [Fact]
    public async Task WhenDisposingConcurrentlyThenShouldHandleGracefully()
    {
        // Arrange
        var disposable = new AsyncResourceHolder();

        // Act
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => disposable.DisposeAsync().AsTask())
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert
        disposable.IsDisposedPublic.Should().BeTrue();
        disposable.ResourceReleased.Should().BeTrue();
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public async Task WhenDerivedAsyncClassDisposedThenBaseAndDerivedShouldBeDisposed()
    {
        // Arrange
        var derived = new DerivedDisposableAsync();

        // Act
        await derived.DisposeAsync();

        // Assert
        derived.DerivedResourceReleased.Should().BeTrue();
        derived.IsDisposedPublic.Should().BeTrue();
    }

    private class BaseDisposableAsyncWithResource : DisposableAsync
    {
        public bool BaseResourceReleased { get; protected set; }
        public bool IsDisposedPublic => IsDisposed;

        protected override async ValueTask DisposeAsync(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                await Task.Delay(5).ConfigureAwait(false);
                BaseResourceReleased = true;
            }

            await base.DisposeAsync(disposing).ConfigureAwait(false);
        }
    }

    private sealed class DerivedDisposableAsync : BaseDisposableAsyncWithResource
    {
        public bool DerivedResourceReleased { get; private set; }

        protected override async ValueTask DisposeAsync(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                await Task.Delay(5).ConfigureAwait(false);
                DerivedResourceReleased = true;
            }

            await base.DisposeAsync(disposing).ConfigureAwait(false);
        }
    }

    #endregion

    #region File System Tests

    [Fact]
    public async Task WhenAsyncFileWriterDisposedThenFileShouldBeFlushedAndClosed()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var writer = new AsyncFileWriter(tempFile);

        // Act
        await writer.WriteLineAsync("Line 1");
        await writer.WriteLineAsync("Line 2");
        await writer.DisposeAsync();

        // Assert
        writer.IsClosed.Should().BeTrue();
        writer.WasFlushed.Should().BeTrue();

        var content = await File.ReadAllTextAsync(tempFile);
        content.Should().Contain("Line 1");
        content.Should().Contain("Line 2");

        // Cleanup
        File.Delete(tempFile);
    }

    private sealed class AsyncFileWriter(string path) : DisposableAsync
    {
        private StreamWriter? _writer = new(path);
        public bool IsClosed { get; private set; }
        public bool WasFlushed { get; private set; }

        public async Task WriteLineAsync(string content)
        {
            ObjectDisposedException.ThrowIf(_writer is null, _writer);

            await _writer.WriteLineAsync(content).ConfigureAwait(false);
        }

        protected override async ValueTask DisposeAsync(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                if (_writer is not null)
                {
                    await _writer.FlushAsync().ConfigureAwait(false);
                    WasFlushed = true;
                    _writer.Dispose();
                    _writer = null;
                }

                IsClosed = true;
            }

            await base.DisposeAsync(disposing).ConfigureAwait(false);
        }
    }

    #endregion
}
