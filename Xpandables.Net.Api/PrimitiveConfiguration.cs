namespace Xpandables.Net.Api;
public static class PrimitiveConfiguration
{
    static readonly IConfiguration _configuration;
    static PrimitiveConfiguration() => _configuration = GetConfiguration();
    public static IConfiguration Configuration => _configuration;
    public static TOptions GetOptions<TOptions>(
        this IConfiguration configuration)
        where TOptions : notnull
        => configuration
            .GetSection(typeof(TOptions).Name)
            .Get<TOptions>() ?? throw new InvalidOperationException(
                $"The configuration section {typeof(TOptions).Name} is missing.");
    public static string GetCurrentDirectory()
        => Directory.GetCurrentDirectory();

    static IConfiguration GetConfiguration()
    {
        string environment = Environment
            .GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(
                "appsettings.json",
                optional: true,
                reloadOnChange: true)
            .AddJsonFile(
                $"appsettings.{environment}.json",
                optional: true,
                reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
