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

namespace Xpandables.Net.SampleApi.Migrations;

/// <inheritdoc />
public partial class RemoveEventFullName : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "EventFullName",
            schema: "Events",
            table: "SnapshotEvents");

        migrationBuilder.DropColumn(
            name: "EventFullName",
            schema: "Events",
            table: "DomainEvents");

        migrationBuilder.RenameColumn(
            name: "EventType",
            schema: "Events",
            table: "SnapshotEvents",
            newName: "EventName");

        migrationBuilder.RenameColumn(
            name: "EventType",
            schema: "Events",
            table: "DomainEvents",
            newName: "EventName");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "EventName",
            schema: "Events",
            table: "SnapshotEvents",
            newName: "EventType");

        migrationBuilder.RenameColumn(
            name: "EventName",
            schema: "Events",
            table: "DomainEvents",
            newName: "EventType");

        migrationBuilder.AddColumn<string>(
            name: "EventFullName",
            schema: "Events",
            table: "SnapshotEvents",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "EventFullName",
            schema: "Events",
            table: "DomainEvents",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");
    }
}
