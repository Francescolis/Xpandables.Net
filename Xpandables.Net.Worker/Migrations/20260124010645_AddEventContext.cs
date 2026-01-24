/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xpandables.Net.Worker.Migrations;

/// <inheritdoc />
public partial class AddEventContext : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "Events");

        migrationBuilder.CreateTable(
            name: "DomainEvents",
            schema: "Events",
            columns: table => new
            {
                KeyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                StreamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                StreamVersion = table.Column<long>(type: "bigint", nullable: false),
                StreamName = table.Column<string>(type: "nvarchar(450)", nullable: false),
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
            constraints: table => table.PrimaryKey("PK_DomainEvents", x => x.KeyId));

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

        migrationBuilder.CreateTable(
            name: "SnapshotEvents",
            schema: "Events",
            columns: table => new
            {
                KeyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
            constraints: table => table.PrimaryKey("PK_SnapshotEvents", x => x.KeyId));

        migrationBuilder.CreateIndex(
            name: "IX_DomainEvent_StreamId",
            schema: "Events",
            table: "DomainEvents",
            column: "StreamId");

        migrationBuilder.CreateIndex(
            name: "IX_DomainEvent_StreamId_StreamVersion_Unique",
            schema: "Events",
            table: "DomainEvents",
            columns: ["StreamId", "StreamVersion"],
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_DomainEvent_StreamName",
            schema: "Events",
            table: "DomainEvents",
            column: "StreamName");

        migrationBuilder.CreateIndex(
            name: "IX_DomainEvents_Sequence",
            schema: "Events",
            table: "DomainEvents",
            column: "Sequence");

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

        migrationBuilder.CreateIndex(
            name: "IX_SnapshotEvent_OwnerId",
            schema: "Events",
            table: "SnapshotEvents",
            column: "OwnerId");

        migrationBuilder.CreateIndex(
            name: "IX_SnapshotEvent_OwnerId_Sequence_Unique",
            schema: "Events",
            table: "SnapshotEvents",
            columns: ["OwnerId", "Sequence"],
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_SnapshotEvents_Sequence",
            schema: "Events",
            table: "SnapshotEvents",
            column: "Sequence");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DomainEvents",
            schema: "Events");

        migrationBuilder.DropTable(
            name: "InboxEvents",
            schema: "Events");

        migrationBuilder.DropTable(
            name: "OutboxEvents",
            schema: "Events");

        migrationBuilder.DropTable(
            name: "SnapshotEvents",
            schema: "Events");
    }
}
