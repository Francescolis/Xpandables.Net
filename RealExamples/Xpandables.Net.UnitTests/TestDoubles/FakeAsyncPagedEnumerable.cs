namespace Xpandables.Net.UnitTests.TestDoubles;

internal sealed class FakeAsyncPagedEnumerable<T>(IEnumerable<T> source, Pagination? pagination = null) : IAsyncPagedEnumerable<T>
{
    private readonly List<T> _items = [.. source];
    private readonly Pagination _pagination = pagination ?? Pagination.Create(pageSize: 10, currentPage: 1, totalCount: source.Count(), continuationToken: null);

    public Pagination Pagination => _pagination;

    public Task<Pagination> GetPaginationAsync(CancellationToken cancellationToken = default) => Task.FromResult(_pagination);

    public IAsyncPagedEnumerable<T> WithStrategy(PaginationStrategy strategy) => this;

    public IAsyncPagedEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new Enumerator(_items, _pagination, cancellationToken);

    private sealed class Enumerator : IAsyncPagedEnumerator<T>
    {
        private readonly List<T> _items;
        private readonly CancellationToken _cancellationToken;
        private int _index = -1;
        private readonly Pagination _pagination;

        public Enumerator(List<T> items, Pagination pagination, CancellationToken cancellationToken)
        {
            _items = items;
            _pagination = pagination;
            _cancellationToken = cancellationToken;
        }

        public T Current => _items[_index];

        public PaginationStrategy Strategy => PaginationStrategy.PerPage;

        public ref readonly Pagination Pagination => ref _pagination;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public ValueTask<bool> MoveNextAsync()
        {
            _cancellationToken.ThrowIfCancellationRequested();
            _index++;
            return new ValueTask<bool>(_index < _items.Count);
        }
    }
}
