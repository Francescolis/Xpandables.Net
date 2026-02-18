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
namespace System.Events.Data.Scripts;

/// <summary>
/// SQLite event table scripts based on AddEventContext migration.
/// </summary>
public sealed class SqliteEventTableScripts : IEventTableScriptProvider
{
	/// <inheritdoc />
	public string GetCreateAllTablesScript(string schema = "Events") => """
CREATE TABLE IF NOT EXISTS "DomainEvents" (
    "KeyId" TEXT NOT NULL PRIMARY KEY,
    "StreamId" TEXT NOT NULL,
    "StreamVersion" INTEGER NOT NULL,
    "StreamName" TEXT NOT NULL,
    "Status" TEXT NOT NULL,
    "CausationId" TEXT NULL,
    "CorrelationId" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    "UpdatedOn" TEXT NULL,
    "DeletedOn" TEXT NULL,
    "EventName" TEXT NOT NULL,
    "EventData" TEXT NOT NULL,
    "Sequence" INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS "InboxEvents" (
    "KeyId" TEXT NOT NULL PRIMARY KEY,
    "ErrorMessage" TEXT NULL,
    "AttemptCount" INTEGER NOT NULL DEFAULT 0,
    "NextAttemptOn" TEXT NULL,
    "ClaimId" TEXT NULL,
    "Consumer" TEXT NOT NULL,
    "Status" TEXT NOT NULL,
    "CausationId" TEXT NULL,
    "CorrelationId" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    "UpdatedOn" TEXT NULL,
    "DeletedOn" TEXT NULL,
	"EventData" TEXT NOT NULL,
    "Sequence" INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS "OutboxEvents" (
    "KeyId" TEXT NOT NULL PRIMARY KEY,
    "ErrorMessage" TEXT NULL,
    "AttemptCount" INTEGER NOT NULL DEFAULT 0,
    "NextAttemptOn" TEXT NULL,
    "ClaimId" TEXT NULL,
    "Status" TEXT NOT NULL,
    "CausationId" TEXT NULL,
    "CorrelationId" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    "UpdatedOn" TEXT NULL,
    "DeletedOn" TEXT NULL,
    "EventName" TEXT NOT NULL,
    "EventData" TEXT NOT NULL,
    "Sequence" INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS "SnapshotEvents" (
    "KeyId" TEXT NOT NULL PRIMARY KEY,
    "OwnerId" TEXT NOT NULL,
    "Status" TEXT NOT NULL,
    "CausationId" TEXT NULL,
    "CorrelationId" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    "UpdatedOn" TEXT NULL,
    "DeletedOn" TEXT NULL,
    "EventName" TEXT NOT NULL,
    "EventData" TEXT NOT NULL,
    "Sequence" INTEGER NOT NULL
);

-- Auto-increment triggers for Sequence columns
CREATE TRIGGER IF NOT EXISTS "TR_DomainEvents_Sequence"
AFTER INSERT ON "DomainEvents"
FOR EACH ROW WHEN NEW."Sequence" = 0
BEGIN
    UPDATE "DomainEvents"
    SET "Sequence" = (SELECT COALESCE(MAX("Sequence"), 0) + 1 FROM "DomainEvents")
    WHERE "KeyId" = NEW."KeyId";
END;

CREATE TRIGGER IF NOT EXISTS "TR_InboxEvents_Sequence"
AFTER INSERT ON "InboxEvents"
FOR EACH ROW WHEN NEW."Sequence" = 0
BEGIN
    UPDATE "InboxEvents"
    SET "Sequence" = (SELECT COALESCE(MAX("Sequence"), 0) + 1 FROM "InboxEvents")
    WHERE "KeyId" = NEW."KeyId";
END;

CREATE TRIGGER IF NOT EXISTS "TR_OutboxEvents_Sequence"
AFTER INSERT ON "OutboxEvents"
FOR EACH ROW WHEN NEW."Sequence" = 0
BEGIN
    UPDATE "OutboxEvents"
    SET "Sequence" = (SELECT COALESCE(MAX("Sequence"), 0) + 1 FROM "OutboxEvents")
    WHERE "KeyId" = NEW."KeyId";
END;

CREATE TRIGGER IF NOT EXISTS "TR_SnapshotEvents_Sequence"
AFTER INSERT ON "SnapshotEvents"
FOR EACH ROW WHEN NEW."Sequence" = 0
BEGIN
    UPDATE "SnapshotEvents"
    SET "Sequence" = (SELECT COALESCE(MAX("Sequence"), 0) + 1 FROM "SnapshotEvents")
    WHERE "KeyId" = NEW."KeyId";
END;

CREATE INDEX IF NOT EXISTS "IX_DomainEvent_StreamId" ON "DomainEvents" ("StreamId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_DomainEvent_StreamId_StreamVersion_Unique" ON "DomainEvents" ("StreamId", "StreamVersion");
CREATE INDEX IF NOT EXISTS "IX_DomainEvent_StreamName" ON "DomainEvents" ("StreamName");
CREATE INDEX IF NOT EXISTS "IX_DomainEvents_Sequence" ON "DomainEvents" ("Sequence");

CREATE INDEX IF NOT EXISTS "IX_InboxEvent_ClaimId" ON "InboxEvents" ("ClaimId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_InboxEvent_EventId_Consumer_Unique" ON "InboxEvents" ("KeyId", "Consumer");
CREATE INDEX IF NOT EXISTS "IX_InboxEvent_Processing" ON "InboxEvents" ("Status", "NextAttemptOn", "Sequence");
CREATE INDEX IF NOT EXISTS "IX_InboxEvent_Retry" ON "InboxEvents" ("Status", "AttemptCount", "NextAttemptOn");
CREATE INDEX IF NOT EXISTS "IX_InboxEvent_Status_NextAttemptOn" ON "InboxEvents" ("Status", "NextAttemptOn");
CREATE INDEX IF NOT EXISTS "IX_InboxEvents_Sequence" ON "InboxEvents" ("Sequence");

CREATE INDEX IF NOT EXISTS "IX_OutboxEvent_ClaimId" ON "OutboxEvents" ("ClaimId");
CREATE INDEX IF NOT EXISTS "IX_OutboxEvent_Processing" ON "OutboxEvents" ("Status", "NextAttemptOn", "Sequence");
CREATE INDEX IF NOT EXISTS "IX_OutboxEvent_Retry" ON "OutboxEvents" ("Status", "AttemptCount", "NextAttemptOn");
CREATE INDEX IF NOT EXISTS "IX_OutboxEvent_Status_NextAttemptOn" ON "OutboxEvents" ("Status", "NextAttemptOn");
CREATE INDEX IF NOT EXISTS "IX_OutboxEvents_Sequence" ON "OutboxEvents" ("Sequence");
""";

	/// <inheritdoc />
	public string GetDropAllTablesScript(string schema = "Events") => """
DROP TRIGGER IF EXISTS "TR_SnapshotEvents_Sequence";
DROP TRIGGER IF EXISTS "TR_OutboxEvents_Sequence";
DROP TRIGGER IF EXISTS "TR_InboxEvents_Sequence";
DROP TRIGGER IF EXISTS "TR_DomainEvents_Sequence";
DROP TABLE IF EXISTS "SnapshotEvents";
DROP TABLE IF EXISTS "OutboxEvents";
DROP TABLE IF EXISTS "InboxEvents";
DROP TABLE IF EXISTS "DomainEvents";
""";
}
