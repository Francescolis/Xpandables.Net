using BenchmarkDotNet.Running;

namespace System.Net.Benchmarking;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run(
        [
            typeof(PaginationBenchmarks),
            typeof(MapperBenchmarks),
            typeof(StrategyOverheadBenchmarks)
        ]);
    }
}
