using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xpandables.Net.Worker.Migrations.InboxStoreData;

/// <inheritdoc />
public partial class AddInboxEvents : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "Events");

        migrationBuilder.CreateTable(
            name: "InboxEvents",
            schema: "Events",
            columns: table => new
            {
                KeyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                AttemptCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                NextAttemptOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                ClaimId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Consumer = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Status = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                CausationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                CorrelationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                Sequence = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1")
            },
            constraints: table => table.PrimaryKey("PK_InboxEvents", x => x.KeyId));

        migrationBuilder.CreateIndex(
            name: "IX_InboxEvent_ClaimId",
            schema: "Events",
            table: "InboxEvents",
            column: "ClaimId");

        migrationBuilder.CreateIndex(
            name: "IX_InboxEvent_EventId_Consumer_Unique",
            schema: "Events",
            table: "InboxEvents",
            columns: ["KeyId", "Consumer"],
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_InboxEvent_Processing",
            schema: "Events",
            table: "InboxEvents",
            columns: ["Status", "NextAttemptOn", "Sequence"]);

        migrationBuilder.CreateIndex(
            name: "IX_InboxEvent_Retry",
            schema: "Events",
            table: "InboxEvents",
            columns: ["Status", "AttemptCount", "NextAttemptOn"]);

        migrationBuilder.CreateIndex(
            name: "IX_InboxEvent_Status_NextAttemptOn",
            schema: "Events",
            table: "InboxEvents",
            columns: ["Status", "NextAttemptOn"]);

        migrationBuilder.CreateIndex(
            name: "IX_InboxEvents_Sequence",
            schema: "Events",
            table: "InboxEvents",
            column: "Sequence");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "InboxEvents",
            schema: "Events");
    }
}
