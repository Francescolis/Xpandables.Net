using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xpandables.Net.Worker.Migrations;

/// <inheritdoc />
public partial class AddOutboxEvents : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "Events");

        migrationBuilder.CreateTable(
            name: "OutboxEvents",
            schema: "Events",
            columns: table => new
            {
                KeyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                AttemptCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                NextAttemptOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                ClaimId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Status = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                CausationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                CorrelationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                EventName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                EventData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Sequence = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1")
            },
            constraints: table => table.PrimaryKey("PK_OutboxEvents", x => x.KeyId));

        migrationBuilder.CreateIndex(
            name: "IX_OutboxEvent_ClaimId",
            schema: "Events",
            table: "OutboxEvents",
            column: "ClaimId");

        migrationBuilder.CreateIndex(
            name: "IX_OutboxEvent_Processing",
            schema: "Events",
            table: "OutboxEvents",
            columns: ["Status", "NextAttemptOn", "Sequence"]);

        migrationBuilder.CreateIndex(
            name: "IX_OutboxEvent_Retry",
            schema: "Events",
            table: "OutboxEvents",
            columns: ["Status", "AttemptCount", "NextAttemptOn"]);

        migrationBuilder.CreateIndex(
            name: "IX_OutboxEvent_Status_NextAttemptOn",
            schema: "Events",
            table: "OutboxEvents",
            columns: ["Status", "NextAttemptOn"]);

        migrationBuilder.CreateIndex(
            name: "IX_OutboxEvents_Sequence",
            schema: "Events",
            table: "OutboxEvents",
            column: "Sequence");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "OutboxEvents",
            schema: "Events");
    }
}
