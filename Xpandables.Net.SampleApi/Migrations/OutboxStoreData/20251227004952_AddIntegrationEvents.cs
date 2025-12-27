using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xpandables.Net.SampleApi.Migrations.OutboxStoreData;

/// <inheritdoc />
public partial class AddIntegrationEvents : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "Events");

        migrationBuilder.CreateTable(
            name: "IntegrationEvents",
            schema: "Events",
            columns: table => new
            {
                KeyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                AttemptCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                NextAttemptOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                ClaimId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CausationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Status = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                EventName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                EventData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Sequence = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1")
            },
            constraints: table => table.PrimaryKey("PK_IntegrationEvents", x => x.KeyId));

        migrationBuilder.CreateIndex(
            name: "IX_IntegrationEvent_ClaimId",
            schema: "Events",
            table: "IntegrationEvents",
            column: "ClaimId");

        migrationBuilder.CreateIndex(
            name: "IX_IntegrationEvent_Processing",
            schema: "Events",
            table: "IntegrationEvents",
            columns: ["Status", "NextAttemptOn", "Sequence"]);

        migrationBuilder.CreateIndex(
            name: "IX_IntegrationEvent_Retry",
            schema: "Events",
            table: "IntegrationEvents",
            columns: ["Status", "AttemptCount", "NextAttemptOn"]);

        migrationBuilder.CreateIndex(
            name: "IX_IntegrationEvent_Status_NextAttemptOn",
            schema: "Events",
            table: "IntegrationEvents",
            columns: ["Status", "NextAttemptOn"]);

        migrationBuilder.CreateIndex(
            name: "IX_IntegrationEvents_Sequence",
            schema: "Events",
            table: "IntegrationEvents",
            column: "Sequence");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "IntegrationEvents",
            schema: "Events");
    }
}
