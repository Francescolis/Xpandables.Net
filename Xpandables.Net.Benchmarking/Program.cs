using BenchmarkDotNet.Running;

namespace Xpandables.Net.Benchmarking;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run(
        [
            //typeof(PaginationBenchmarks),
            //typeof(MapperBenchmarks),
            //typeof(StrategyOverheadBenchmarks),
            //typeof(JsonStreamingBenchmarks),
            typeof(AspNetCoreStreamingBenchmarks),
            //typeof(AsyncPagedSerializationBenchmarks)
        ]);
    }
}
