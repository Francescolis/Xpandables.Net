using System.Collections;
using System.Net;

namespace System.Results;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed record ResultInfo
{
	public required HttpStatusCode StatusCode { get; init; }
	public string? Title { get; init; }
	public string? Detail { get; init; }
	public Uri? Location { get; init; }
	public object? Value { get; init; }
	public Exception? Exception { get; init; }
	public ElementCollection Headers { get; init; } = [];
	public ElementCollection Extensions { get; init; } = [];

	public static ResultInfo FromResult(Result result)
	{
		ArgumentNullException.ThrowIfNull(result);

		return result switch
		{
			SuccessResult success => new ResultInfo
			{
				StatusCode = success.StatusCode,
				Title = success.Title,
				Detail = success.Detail,
				Location = success.Location,
				Value = success.Value,
				Headers = success.Headers,
				Extensions = success.Extensions
			},
			FailureResult failure => new ResultInfo
			{
				StatusCode = failure.StatusCode,
				Title = failure.Title,
				Detail = failure.Detail,
				Exception = failure.Exception,
				Headers = failure.Headers,
				Extensions = failure.Extensions
			},
			_ => throw new ArgumentException("Unknown result type.", nameof(result))
		};
	}
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
