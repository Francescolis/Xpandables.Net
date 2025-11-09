using BenchmarkDotNet.Running;

using Xpandables.Net.Benchmarking.Benchmarks;

namespace Xpandables.Net.Benchmarking;

public static class Program
{
    public static void Main(string[] args)
    {
        //BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

        BenchmarkRunner.Run(new[]
        {
            //BenchmarkConverter.TypeToBenchmarks(typeof(SerializationBenchmarks)),
            BenchmarkConverter.TypeToBenchmarks(typeof(DeserializationBenchmarks))
        });
    }
}
