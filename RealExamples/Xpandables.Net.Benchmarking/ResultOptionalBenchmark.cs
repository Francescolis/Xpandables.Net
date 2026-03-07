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
using System.Net;
using System.Optionals;
using System.Results;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Xpandables.Net.Benchmarking;

/// <summary>
/// Benchmarks measuring Result and Optional creation, access patterns,
/// and value-type boxing characteristics.
/// </summary>
/// <remarks>
/// <para><strong>Result benchmarks</strong> compare the cost of building success/failure
/// results with and without generic value types, measuring builder overhead and
/// <see cref="Result{TValue}"/> typed-value access vs. the base <c>InternalValue</c> path.</para>
/// <para><strong>Optional benchmarks</strong> compare Map/Bind chains on <see cref="Optional{T}"/>
/// (a readonly record struct) to verify zero-allocation behavior for value-type payloads.</para>
/// </remarks>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
[RankColumn]
public class ResultOptionalBenchmark
{
	// ── Result benchmarks ───────────────────────────────────────────────

	/// <summary>
	/// Baseline: create a non-generic <see cref="SuccessResult"/>.
	/// </summary>
	[Benchmark(Baseline = true, Description = "Result.Success() — non-generic")]
	public Result CreateSuccessResult()
	{
		return Result.Success().Build();
	}

	/// <summary>
	/// Create a generic <see cref="SuccessResult{TValue}"/> with an int value.
	/// Measures the typed-value storage path that avoids boxing.
	/// </summary>
	[Benchmark(Description = "Result.Success<int>(42) — value-type")]
	public Result<int> CreateSuccessResultInt()
	{
		return Result.Success(42).Build();
	}

	/// <summary>
	/// Create a generic <see cref="SuccessResult{TValue}"/> with a string value.
	/// Reference-type baseline for comparison with value types.
	/// </summary>
	[Benchmark(Description = "Result.Success<string>(\"hello\") — ref-type")]
	public Result<string> CreateSuccessResultString()
	{
		return Result.Success("hello").Build();
	}

	/// <summary>
	/// Create a <see cref="FailureResult"/> with status code, title, and detail.
	/// Measures builder-chain overhead.
	/// </summary>
	[Benchmark(Description = "Result.Failure() — with details")]
	public Result CreateFailureResultWithDetails()
	{
		return Result.Failure()
			.WithStatusCode(HttpStatusCode.BadRequest)
			.WithTitle("Validation failed")
			.WithDetail("Name is required.")
			.Build();
	}

	/// <summary>
	/// Access <see cref="Result{TValue}.Value"/> on a pre-built result.
	/// Ensures the typed path does not box.
	/// </summary>
	[Benchmark(Description = "Result<int>.Value access")]
	public int AccessResultValue()
	{
		SuccessResult<int> result = Result.Success(42).Build();
		return result.Value;
	}

	// ── Optional benchmarks ─────────────────────────────────────────────

	/// <summary>
	/// Create an <see cref="Optional{T}"/> with a value-type payload.
	/// Should be zero-allocation (readonly record struct).
	/// </summary>
	[Benchmark(Description = "Optional.Some(42) — create")]
	public Optional<int> CreateOptionalInt()
	{
		return Optional.Some(42);
	}

	/// <summary>
	/// Create an empty <see cref="Optional{T}"/>.
	/// </summary>
	[Benchmark(Description = "Optional.Empty<int>() — create")]
	public Optional<int> CreateOptionalEmpty()
	{
		return Optional.Empty<int>();
	}

	/// <summary>
	/// Chain Map operations on <see cref="Optional{T}"/> with value-type.
	/// Measures struct copy overhead with no heap allocation.
	/// </summary>
	[Benchmark(Description = "Optional<int> Map chain (x3)")]
	public Optional<int> OptionalMapChain()
	{
		return Optional.Some(10)
			.Map(x => x + 1)
			.Map(x => x * 2)
			.Map(x => x - 3);
	}

	/// <summary>
	/// Bind from Optional&lt;int&gt; to Optional&lt;string&gt;.
	/// Measures type-conversion path.
	/// </summary>
	[Benchmark(Description = "Optional<int>.Bind<string>")]
	public Optional<string> OptionalBind()
	{
		return Optional.Some(42)
			.Bind(x => x.ToString());
	}

	/// <summary>
	/// Empty-fallback chain: Optional is empty, calls Empty() supplier.
	/// </summary>
	[Benchmark(Description = "Optional<int> Empty fallback")]
	public Optional<int> OptionalEmptyFallback()
	{
		return Optional.Empty<int>()
			.Empty(() => 99);
	}

	/// <summary>
	/// Map on empty Optional — should short-circuit without invoking the delegate.
	/// </summary>
	[Benchmark(Description = "Optional<int> Map on empty (short-circuit)")]
	public Optional<int> OptionalMapOnEmpty()
	{
		return Optional.Empty<int>()
			.Map(x => x + 1);
	}

	/// <summary>
	/// Full pipeline: create, map, bind, access value.
	/// Representative of real-world usage patterns.
	/// </summary>
	[Benchmark(Description = "Optional full pipeline (Some→Map→Bind→Value)")]
	public string OptionalFullPipeline()
	{
		return Optional.Some(42)
			.Map(x => x * 2)
			.Bind(x => $"Result: {x}")
			.Value;
	}
}
