using BenchmarkDotNet.Running;

namespace Xpandables.Net.Benchmarking;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(AsyncPagedEnumerableBenchmark).Assembly).Run(args);
    }
}
