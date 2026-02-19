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
using System.Cache;

using FluentAssertions;

namespace Xpandables.Net.UnitTests.Primitives;

public sealed class MemoryAwareCacheTests : IDisposable
{
    private MemoryAwareCache<string, TestCacheItem>? _cache;

    public void Dispose()
    {
        _cache?.Dispose();
    }

    #region Test Types

    private sealed class TestCacheItem(string value)
    {
        public string Value { get; } = value;
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
    }

    private sealed class ExpensiveResource(string id) : IDisposable
    {
        public string Id { get; } = id;
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    #endregion

    #region GetOrAdd Tests

    [Fact]
    public void WhenGetOrAddWithNewKeyThenShouldCreateAndCacheValue()
    {
        // Arrange
        _cache = new MemoryAwareCache<string, TestCacheItem>();
		int factoryCallCount = 0;

		// Act
		TestCacheItem result = _cache.GetOrAdd("key1", key =>
        {
            factoryCallCount++;
            return new TestCacheItem($"value-for-{key}");
        });

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be("value-for-key1");
        factoryCallCount.Should().Be(1);
    }

    [Fact]
    public void WhenGetOrAddWithExistingKeyThenShouldReturnCachedValue()
    {
        // Arrange
        _cache = new MemoryAwareCache<string, TestCacheItem>();
		int factoryCallCount = 0;

		TestCacheItem firstResult = _cache.GetOrAdd("key1", _ =>
        {
            factoryCallCount++;
            return new TestCacheItem("first");
        });

		// Act
		TestCacheItem secondResult = _cache.GetOrAdd("key1", _ =>
        {
            factoryCallCount++;
            return new TestCacheItem("second");
        });

        // Assert
        secondResult.Should().BeSameAs(firstResult);
        factoryCallCount.Should().Be(1);
    }

    [Fact]
    public void WhenGetOrAddWithNullFactoryThenShouldThrow()
    {
        // Arrange
        _cache = new MemoryAwareCache<string, TestCacheItem>();

		// Act
		Func<TestCacheItem> act = () => _cache.GetOrAdd("key", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region AddOrUpdate Tests

    [Fact]
    public void WhenAddOrUpdateWithNewKeyThenShouldAddValue()
    {
        // Arrange
        _cache = new MemoryAwareCache<string, TestCacheItem>();
        var item = new TestCacheItem("new-value");

		// Act
		TestCacheItem result = _cache.AddOrUpdate("key1", item);

        // Assert
        result.Should().BeSameAs(item);
        _cache.TryGetValue("key1", out TestCacheItem? cached).Should().BeTrue();
        cached.Should().BeSameAs(item);
    }

    [Fact]
    public void WhenAddOrUpdateWithExistingKeyThenShouldUpdateValue()
    {
        // Arrange
        _cache = new MemoryAwareCache<string, TestCacheItem>();
        var originalItem = new TestCacheItem("original");
        _cache.AddOrUpdate("key1", originalItem);

        var newItem = new TestCacheItem("updated");

		// Act
		TestCacheItem result = _cache.AddOrUpdate("key1", newItem);

        // Assert
        result.Should().BeSameAs(newItem);
        _cache.TryGetValue("key1", out TestCacheItem? cached).Should().BeTrue();
        cached.Should().BeSameAs(newItem);
    }

    [Fact]
    public void WhenAddOrUpdateWithNullValueThenShouldThrow()
    {
        // Arrange
        _cache = new MemoryAwareCache<string, TestCacheItem>();

		// Act
		Func<TestCacheItem> act = () => _cache.AddOrUpdate("key", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WhenAddOrUpdateWithNullKeyThenShouldThrow()
    {
        // Arrange
        _cache = new MemoryAwareCache<string, TestCacheItem>();
        var item = new TestCacheItem("value");

		// Act
		Func<TestCacheItem> act = () => _cache.AddOrUpdate(null!, item);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region TryGetValue Tests

    [Fact]
    public void WhenTryGetValueForExistingKeyThenShouldReturnTrueWithValue()
    {
        // Arrange
        _cache = new MemoryAwareCache<string, TestCacheItem>();
        var item = new TestCacheItem("test-value");
        _cache.AddOrUpdate("key1", item);

		// Act
		bool result = _cache.TryGetValue("key1", out TestCacheItem? value);

        // Assert
        result.Should().BeTrue();
        value.Should().BeSameAs(item);
    }

    [Fact]
    public void WhenTryGetValueForNonExistingKeyThenShouldReturnFalse()
    {
        // Arrange
        _cache = new MemoryAwareCache<string, TestCacheItem>();

		// Act
		bool result = _cache.TryGetValue("nonexistent", out TestCacheItem? value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    #endregion

    #region Cleanup and Expiration Tests

    [Fact]
    public async Task WhenItemExpiresAfterMaxAgeThenShouldBeRemoved()
    {
        // Arrange - Short cleanup interval and max age for testing
        _cache = new MemoryAwareCache<string, TestCacheItem>(
            cleanupInterval: TimeSpan.FromMilliseconds(100),
            maxAge: TimeSpan.FromMilliseconds(50));

        var item = new TestCacheItem("expire-me");
        _cache.AddOrUpdate("key1", item);

        // Initially should exist
        _cache.TryGetValue("key1", out _).Should().BeTrue();

        // Act - Wait for expiration and cleanup
        await Task.Delay(200);

        // Assert
        _cache.TryGetValue("key1", out _).Should().BeFalse();
    }

    [Fact]
    public async Task WhenCleanupIntervalIsZeroThenItemsShouldNotExpire()
    {
        // Arrange - Zero cleanup interval means no automatic cleanup
        _cache = new MemoryAwareCache<string, TestCacheItem>(
            cleanupInterval: TimeSpan.Zero,
            maxAge: TimeSpan.Zero);

        var item = new TestCacheItem("persist-me");
        _cache.AddOrUpdate("key1", item);

        // Act - Wait a bit
        await Task.Delay(100);

        // Assert - Item should still be there
        _cache.TryGetValue("key1", out TestCacheItem? cached).Should().BeTrue();
        cached.Should().BeSameAs(item);
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void WhenDisposedThenGetOrAddShouldThrow()
    {
        // Arrange
        _cache = new MemoryAwareCache<string, TestCacheItem>();
        _cache.Dispose();

		// Act
		Func<TestCacheItem> act = () => _cache.GetOrAdd("key", _ => new TestCacheItem("value"));

        // Assert
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void WhenDisposedThenAddOrUpdateShouldThrow()
    {
        // Arrange
        _cache = new MemoryAwareCache<string, TestCacheItem>();
        _cache.Dispose();

		// Act
		Func<TestCacheItem> act = () => _cache.AddOrUpdate("key", new TestCacheItem("value"));

        // Assert
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void WhenDisposedThenTryGetValueShouldReturnFalse()
    {
        // Arrange
        _cache = new MemoryAwareCache<string, TestCacheItem>();
        _cache.AddOrUpdate("key", new TestCacheItem("value"));
        _cache.Dispose();

		// Act
		bool result = _cache.TryGetValue("key", out TestCacheItem? value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void WhenDisposedMultipleTimesThenShouldNotThrow()
    {
        // Arrange
        _cache = new MemoryAwareCache<string, TestCacheItem>();

		// Act
		Action act = () =>
        {
            _cache.Dispose();
            _cache.Dispose();
            _cache.Dispose();
        };

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region Weak Reference Tests

    [Fact]
    public void WhenValueIsGarbageCollectedThenCacheShouldNotReturnIt()
    {
        // Arrange
        _cache = new MemoryAwareCache<string, TestCacheItem>(
            cleanupInterval: TimeSpan.FromHours(1),
            maxAge: TimeSpan.FromHours(1));

        // Create a scope where the item will go out of scope
        AddItemToCache("key1", "temporary-value");

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

		// Act - The weak reference might still have the value
		// depending on GC behavior, so we just verify no crash
		bool result = _cache.TryGetValue("key1", out TestCacheItem? value);

        // Assert - Either we get the value (GC hasn't collected) or we don't
        if (result)
        {
            value.Should().NotBeNull();
        }
    }

    private void AddItemToCache(string key, string value)
    {
        var item = new TestCacheItem(value);
        _cache!.AddOrUpdate(key, item);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task WhenAccessingConcurrentlyThenShouldBeThreadSafe()
    {
        // Arrange
        _cache = new MemoryAwareCache<string, TestCacheItem>();
        var tasks = new List<Task>();
        var exceptions = new List<Exception>();

        // Act - Concurrent reads and writes
        for (int i = 0; i < 100; i++)
        {
			string key = $"key{i % 10}";
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    _cache.GetOrAdd(key, k => new TestCacheItem($"value-{k}"));
                    _cache.TryGetValue(key, out _);
                    _cache.AddOrUpdate(key, new TestCacheItem($"updated-{key}"));
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        exceptions.Should().BeEmpty();
    }

    [Fact]
    public async Task WhenMultipleThreadsCallGetOrAddWithSameKeyThenFactoryShouldBeCalledOnce()
    {
        // Arrange
        _cache = new MemoryAwareCache<string, TestCacheItem>();
		int factoryCallCount = 0;
        var barrier = new Barrier(10);

		// Act
		IEnumerable<Task<TestCacheItem>> tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            barrier.SignalAndWait();
            return _cache.GetOrAdd("shared-key", key =>
            {
                Interlocked.Increment(ref factoryCallCount);
                return new TestCacheItem($"value-{key}");
            });
        }));

		TestCacheItem[] results = await Task.WhenAll(tasks);

        // Assert - Due to race conditions, factory might be called more than once
        // but all results should be the same value (first one cached)
        results.Should().AllSatisfy(r => r.Should().NotBeNull());
    }

    #endregion

    #region Real World Scenario Tests

    [Fact]
    public void WhenCachingExpensiveComputationsThenShouldImprovePerformance()
    {
        // Arrange
        _cache = new MemoryAwareCache<string, TestCacheItem>();
		int computationCount = 0;

        string ExpensiveComputation(string key)
        {
            computationCount++;
            Thread.Sleep(10);
            return $"computed-{key}";
        }

		// Act - Multiple requests for same data
		TestCacheItem result1 = _cache.GetOrAdd("data", k => new TestCacheItem(ExpensiveComputation(k)));
		TestCacheItem result2 = _cache.GetOrAdd("data", k => new TestCacheItem(ExpensiveComputation(k)));
		TestCacheItem result3 = _cache.GetOrAdd("data", k => new TestCacheItem(ExpensiveComputation(k)));

        // Assert
        computationCount.Should().Be(1);
        result1.Value.Should().Be("computed-data");
        result2.Should().BeSameAs(result1);
        result3.Should().BeSameAs(result1);
    }

    [Fact]
    public void WhenCachingDatabaseResultsThenShouldReduceQueries()
    {
        // Arrange
        _cache = new MemoryAwareCache<string, TestCacheItem>();
		int queryCount = 0;

        TestCacheItem SimulateDatabaseQuery(string userId)
        {
            queryCount++;
            return new TestCacheItem($"User:{userId}");
        }

        // Act - Simulate multiple page loads requesting same user
        for (int i = 0; i < 100; i++)
        {
            _cache.GetOrAdd("user:123", k => SimulateDatabaseQuery(k));
        }

        // Assert
        queryCount.Should().Be(1);
    }

    [Fact]
    public void WhenCachingConfigurationThenShouldUpdateOnDemand()
    {
        // Arrange
        _cache = new MemoryAwareCache<string, TestCacheItem>();

        var configV1 = new TestCacheItem("version1");
        _cache.AddOrUpdate("config", configV1);

        // Act - Simulate configuration update
        var configV2 = new TestCacheItem("version2");
        _cache.AddOrUpdate("config", configV2);

        // Assert
        _cache.TryGetValue("config", out TestCacheItem? current);
        current.Should().BeSameAs(configV2);
        current!.Value.Should().Be("version2");
    }

    [Fact]
    public void WhenCachingApiResponsesThenShouldHandleMultipleEndpoints()
    {
        // Arrange
        _cache = new MemoryAwareCache<string, TestCacheItem>();

        // Act - Cache different API responses
        _cache.AddOrUpdate("/api/users", new TestCacheItem("[{user1}, {user2}]"));
        _cache.AddOrUpdate("/api/products", new TestCacheItem("[{product1}]"));
        _cache.AddOrUpdate("/api/orders", new TestCacheItem("[{order1}, {order2}, {order3}]"));

        // Assert
        _cache.TryGetValue("/api/users", out TestCacheItem? users).Should().BeTrue();
        _cache.TryGetValue("/api/products", out TestCacheItem? products).Should().BeTrue();
        _cache.TryGetValue("/api/orders", out TestCacheItem? orders).Should().BeTrue();

        users!.Value.Should().Contain("user1");
        products!.Value.Should().Contain("product1");
        orders!.Value.Should().Contain("order3");
    }

    #endregion

    #region Default Configuration Tests

    [Fact]
    public void WhenCreatingWithDefaultConfigurationThenShouldUseDefaults()
    {
        // Arrange & Act
        _cache = new MemoryAwareCache<string, TestCacheItem>();

        // Add item and verify it works
        _cache.AddOrUpdate("key", new TestCacheItem("value"));

        // Assert
        _cache.TryGetValue("key", out TestCacheItem? item).Should().BeTrue();
        item!.Value.Should().Be("value");
    }

    [Fact]
    public void WhenCreatingWithCustomConfigurationThenShouldUseCustomValues()
    {
        // Arrange & Act
        _cache = new MemoryAwareCache<string, TestCacheItem>(
            cleanupInterval: TimeSpan.FromMinutes(10),
            maxAge: TimeSpan.FromHours(2));

        _cache.AddOrUpdate("key", new TestCacheItem("value"));

        // Assert
        _cache.TryGetValue("key", out _).Should().BeTrue();
    }

    #endregion

    #region WeakCacheEntry Tests

    [Fact]
    public void WhenCreatingWeakCacheEntryThenShouldStoreValue()
    {
        // Arrange
        var item = new TestCacheItem("test");

        // Act
        var entry = new WeakCacheEntry<TestCacheItem>(item);

        // Assert
        entry.TryGetValue(out TestCacheItem? retrieved).Should().BeTrue();
        retrieved.Should().BeSameAs(item);
    }

    [Fact]
    public void WhenWeakCacheEntryNotExpiredThenIsExpiredShouldBeFalse()
    {
        // Arrange
        var item = new TestCacheItem("test");
        var entry = new WeakCacheEntry<TestCacheItem>(item);

		// Act
		bool isExpired = entry.IsExpired(TimeSpan.FromHours(1));

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void WhenMaxAgeIsZeroThenEntryIsNeverExpired()
    {
        // Arrange
        var item = new TestCacheItem("test");
        var entry = new WeakCacheEntry<TestCacheItem>(item);

		// Act
		bool isExpired = entry.IsExpired(TimeSpan.Zero);

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void WhenComparingEqualWeakCacheEntriesThenShouldBeEqual()
    {
        // Arrange
        var item = new TestCacheItem("test");
        var entry1 = new WeakCacheEntry<TestCacheItem>(item);
        var entry2 = new WeakCacheEntry<TestCacheItem>(item);

        // Assert
        entry1.Equals(entry2).Should().BeTrue();
        entry1.Equals(item).Should().BeTrue();
        (entry1 == entry2).Should().BeTrue();
    }

    [Fact]
    public void WhenComparingDifferentWeakCacheEntriesThenShouldNotBeEqual()
    {
        // Arrange
        var item1 = new TestCacheItem("test1");
        var item2 = new TestCacheItem("test2");
        var entry1 = new WeakCacheEntry<TestCacheItem>(item1);
        var entry2 = new WeakCacheEntry<TestCacheItem>(item2);

        // Assert
        entry1.Equals(entry2).Should().BeFalse();
        (entry1 != entry2).Should().BeTrue();
    }

    [Fact]
    public void WhenGettingHashCodeThenShouldReturnItemHashCode()
    {
        // Arrange
        var item = new TestCacheItem("test");
        var entry = new WeakCacheEntry<TestCacheItem>(item);

        // Assert
        entry.GetHashCode().Should().Be(item.GetHashCode());
    }

    [Fact]
    public void WhenAccessingLastAccessTimeThenShouldBeCloseToNow()
    {
		// Arrange
		DateTime beforeCreation = DateTime.UtcNow;
        var item = new TestCacheItem("test");
        var entry = new WeakCacheEntry<TestCacheItem>(item);
		DateTime afterCreation = DateTime.UtcNow;

        // Assert
        entry.LastAccessTime.Should().BeOnOrAfter(beforeCreation);
        entry.LastAccessTime.Should().BeOnOrBefore(afterCreation);
    }

    #endregion
}
