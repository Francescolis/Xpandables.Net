/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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
/// SQLite event table scripts.
/// </summary>
public sealed class SqliteEventTableScripts : IEventTableScriptProvider
{
	/// <inheritdoc />
	public string GetCreateAllTablesScript(
		string schema = "Event",
		string? eventDomain = "EventDomain",
		string? eventInbox = "EventInbox",
		string? eventOutbox = "EventOutbox",
		string? eventSnapshot = "EventSnapshot") => $$"""
CREATE TABLE IF NOT EXISTS "{{eventDomain}}" (
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

CREATE TABLE IF NOT EXISTS "{{eventInbox}}" (
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

CREATE TABLE IF NOT EXISTS "{{eventOutbox}}" (
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

CREATE TABLE IF NOT EXISTS "{{eventSnapshot}}" (
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
CREATE TRIGGER IF NOT EXISTS "TR_{{eventDomain}}_Sequence"
AFTER INSERT ON "{{eventDomain}}"
FOR EACH ROW WHEN NEW."Sequence" = 0
BEGIN
    UPDATE "{{eventDomain}}"
    SET "Sequence" = (SELECT COALESCE(MAX("Sequence"), 0) + 1 FROM "{{eventDomain}}")
    WHERE "KeyId" = NEW."KeyId";
END;

CREATE TRIGGER IF NOT EXISTS "TR_{{eventInbox}}_Sequence"
AFTER INSERT ON "{{eventInbox}}"
FOR EACH ROW WHEN NEW."Sequence" = 0
BEGIN
    UPDATE "{{eventInbox}}"
    SET "Sequence" = (SELECT COALESCE(MAX("Sequence"), 0) + 1 FROM "{{eventInbox}}")
    WHERE "KeyId" = NEW."KeyId";
END;

CREATE TRIGGER IF NOT EXISTS "TR_{{eventOutbox}}_Sequence"
AFTER INSERT ON "{{eventOutbox}}"
FOR EACH ROW WHEN NEW."Sequence" = 0
BEGIN
    UPDATE "{{eventOutbox}}"
    SET "Sequence" = (SELECT COALESCE(MAX("Sequence"), 0) + 1 FROM "{{eventOutbox}}")
    WHERE "KeyId" = NEW."KeyId";
END;

CREATE TRIGGER IF NOT EXISTS "TR_{{eventSnapshot}}_Sequence"
AFTER INSERT ON "{{eventSnapshot}}"
FOR EACH ROW WHEN NEW."Sequence" = 0
BEGIN
    UPDATE "{{eventSnapshot}}"
    SET "Sequence" = (SELECT COALESCE(MAX("Sequence"), 0) + 1 FROM "{{eventSnapshot}}")
    WHERE "KeyId" = NEW."KeyId";
END;

CREATE INDEX IF NOT EXISTS "IX_{{eventDomain}}_StreamId" ON "{{eventDomain}}" ("StreamId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_{{eventDomain}}_StreamId_StreamVersion_Unique" ON "{{eventDomain}}" ("StreamId", "StreamVersion");
CREATE INDEX IF NOT EXISTS "IX_{{eventDomain}}_StreamName" ON "{{eventDomain}}" ("StreamName");
CREATE INDEX IF NOT EXISTS "IX_{{eventDomain}}_Sequence" ON "{{eventDomain}}" ("Sequence");

CREATE INDEX IF NOT EXISTS "IX_{{eventInbox}}_ClaimId" ON "{{eventInbox}}" ("ClaimId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_{{eventInbox}}_EventId_Consumer_Unique" ON "{{eventInbox}}" ("KeyId", "Consumer");
CREATE INDEX IF NOT EXISTS "IX_{{eventInbox}}_Processing" ON "{{eventInbox}}" ("Status", "NextAttemptOn", "Sequence");
CREATE INDEX IF NOT EXISTS "IX_{{eventInbox}}_Retry" ON "{{eventInbox}}" ("Status", "AttemptCount", "NextAttemptOn");
CREATE INDEX IF NOT EXISTS "IX_{{eventInbox}}_Status_NextAttemptOn" ON "{{eventInbox}}" ("Status", "NextAttemptOn");
CREATE INDEX IF NOT EXISTS "IX_{{eventInbox}}_Sequence" ON "{{eventInbox}}" ("Sequence");

CREATE INDEX IF NOT EXISTS "IX_{{eventOutbox}}_ClaimId" ON "{{eventOutbox}}" ("ClaimId");
CREATE INDEX IF NOT EXISTS "IX_{{eventOutbox}}_Processing" ON "{{eventOutbox}}" ("Status", "NextAttemptOn", "Sequence");
CREATE INDEX IF NOT EXISTS "IX_{{eventOutbox}}_Retry" ON "{{eventOutbox}}" ("Status", "AttemptCount", "NextAttemptOn");
CREATE INDEX IF NOT EXISTS "IX_{{eventOutbox}}_Status_NextAttemptOn" ON "{{eventOutbox}}" ("Status", "NextAttemptOn");
CREATE INDEX IF NOT EXISTS "IX_{{eventOutbox}}_Sequence" ON "{{eventOutbox}}" ("Sequence");
""";

	/// <inheritdoc />
	public string GetDropAllTablesScript(
		string schema = "Event",
		string? eventDomain = "EventDomain",
		string? eventInbox = "EventInbox",
		string? eventOutbox = "EventOutbox",
		string? eventSnapshot = "EventSnapshot") => """
DROP TRIGGER IF EXISTS "TR_{{eventSnapshot}}_Sequence";
DROP TRIGGER IF EXISTS "TR_{{eventOutbox}}_Sequence";
DROP TRIGGER IF EXISTS "TR_{{eventInbox}}_Sequence";
DROP TRIGGER IF EXISTS "TR_{{eventDomain}}_Sequence";
DROP TABLE IF EXISTS "{{eventSnapshot}}";
DROP TABLE IF EXISTS "{{eventOutbox}}";
DROP TABLE IF EXISTS "{{eventInbox}}";
DROP TABLE IF EXISTS "{{eventDomain}}";
""";
}
