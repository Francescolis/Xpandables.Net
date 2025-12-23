using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xpandables.Net.SampleApi.Migrations.BankAccountData;

/// <inheritdoc />
public partial class AddBankAccount : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "Bank");

        migrationBuilder.CreateTable(
            name: "Accounts",
            schema: "Bank",
            columns: table => new
            {
                KeyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                AccountType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Owner = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Accounts", x => x.KeyId));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Accounts",
            schema: "Bank");
    }
}
