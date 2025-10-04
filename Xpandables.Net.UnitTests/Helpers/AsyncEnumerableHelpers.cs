using System.Runtime.CompilerServices;

namespace Xpandables.Net.UnitTests.Helpers;

internal static class AsyncEnumerableHelpers
{
    public static async IAsyncEnumerable<T> ToAsync<T>(this IEnumerable<T> source, [EnumeratorCancellation] CancellationToken ct = default)
    {
        foreach (var item in source)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Yield();
            yield return item;
        }
    }

    public static IAsyncEnumerable<T> RangeAsync<T>(int start, int count, Func<int, T> projector)
        => Enumerable.Range(start, count).Select(projector).ToAsync();
}