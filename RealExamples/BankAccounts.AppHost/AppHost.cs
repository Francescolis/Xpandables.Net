var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ParameterResource> saPassword = builder.AddParameter("sa-password", "DevPass123!");
var sqlServer = builder.AddSqlServer("sqlserver", password: saPassword, port: 1433);

var accountDB = sqlServer.AddDatabase("accountDB", "AccountDB");
var eventDB = sqlServer.AddDatabase("eventDB", "EventDB");

builder.AddProject<Projects.BankAccounts_Api>("bankaccounts-api")
	.WithReference(accountDB)
	.WithReference(eventDB)
	.WaitFor(accountDB)
	.WaitFor(eventDB);

builder.Build().Run();
