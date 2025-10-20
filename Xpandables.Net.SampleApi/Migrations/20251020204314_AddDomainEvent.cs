﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xpandables.Net.SampleApi.Migrations;

/// <inheritdoc />
public partial class AddDomainEvent : Migration
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
                CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                EventType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                EventFullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                EventData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Sequence = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1")
            },
            constraints: table => table.PrimaryKey("PK_DomainEvents", x => x.KeyId));

        migrationBuilder.CreateTable(
            name: "SnapshotEvents",
            schema: "Events",
            columns: table => new
            {
                KeyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Status = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                EventType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                EventFullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
            name: "SnapshotEvents",
            schema: "Events");
    }
}
