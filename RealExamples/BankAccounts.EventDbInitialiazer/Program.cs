using System.Events.Data.Scripts;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddSqlServerClient("eventDB");

var connectionString = builder.Configuration.GetConnectionString("eventDB");

using var connection = new SqlConnection(connectionString);
await connection.OpenAsync().ConfigureAwait(false);

IEventTableScriptProvider scripts = EventTableScriptProviders.SqlServer;
string sql = scripts.GetCreateAllTablesScript("dbo");

#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
using var command = new SqlCommand(sql, connection);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
await command.ExecuteNonQueryAsync().ConfigureAwait(false);

#pragma warning disable CA1303 // Do not pass literals as localized parameters
Console.WriteLine("EventDB initialized.");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
