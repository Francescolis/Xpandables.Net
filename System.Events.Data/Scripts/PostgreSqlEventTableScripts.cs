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
/// PostgreSQL event table scripts based on AddEventContext migration.
/// </summary>
public sealed class PostgreSqlEventTableScripts : IEventTableScriptProvider
{
	/// <inheritdoc />
	public string GetCreateAllTablesScript(string schema = "events") => $$"""
CREATE SCHEMA IF NOT EXISTS "{{schema}}";

CREATE TABLE IF NOT EXISTS "{{schema}}"."DomainEvents" (
    "KeyId" UUID NOT NULL PRIMARY KEY,
    "StreamId" UUID NOT NULL,
    "StreamVersion" BIGINT NOT NULL,
    "StreamName" VARCHAR(450) NOT NULL,
    "Status" VARCHAR(255) NOT NULL,
    "CausationId" VARCHAR(64) NULL,
    "CorrelationId" VARCHAR(64) NULL,
    "CreatedOn" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    "UpdatedOn" TIMESTAMP WITHOUT TIME ZONE NULL,
    "DeletedOn" TIMESTAMP WITHOUT TIME ZONE NULL,
    "EventName" VARCHAR(255) NOT NULL,
    "EventData" TEXT NOT NULL,
    "Sequence" BIGSERIAL NOT NULL
);

CREATE TABLE IF NOT EXISTS "{{schema}}"."InboxEvents" (
    "KeyId" UUID NOT NULL PRIMARY KEY,
    "ErrorMessage" TEXT NULL,
    "AttemptCount" INTEGER NOT NULL DEFAULT 0,
    "NextAttemptOn" TIMESTAMP WITHOUT TIME ZONE NULL,
    "ClaimId" UUID NULL,
    "Consumer" VARCHAR(256) NOT NULL,
    "Status" VARCHAR(255) NOT NULL,
    "CausationId" VARCHAR(64) NULL,
    "CorrelationId" VARCHAR(64) NULL,
    "CreatedOn" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    "UpdatedOn" TIMESTAMP WITHOUT TIME ZONE NULL,
    "DeletedOn" TIMESTAMP WITHOUT TIME ZONE NULL,
	"EventName" VARCHAR(255) NOT NULL,
    "Sequence" BIGSERIAL NOT NULL
);

CREATE TABLE IF NOT EXISTS "{{schema}}"."OutboxEvents" (
    "KeyId" UUID NOT NULL PRIMARY KEY,
    "ErrorMessage" TEXT NULL,
    "AttemptCount" INTEGER NOT NULL DEFAULT 0,
    "NextAttemptOn" TIMESTAMP WITHOUT TIME ZONE NULL,
    "ClaimId" UUID NULL,
    "Status" VARCHAR(255) NOT NULL,
    "CausationId" VARCHAR(64) NULL,
    "CorrelationId" VARCHAR(64) NULL,
    "CreatedOn" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    "UpdatedOn" TIMESTAMP WITHOUT TIME ZONE NULL,
    "DeletedOn" TIMESTAMP WITHOUT TIME ZONE NULL,
    "EventName" VARCHAR(255) NOT NULL,
    "EventData" TEXT NOT NULL,
    "Sequence" BIGSERIAL NOT NULL
);

CREATE TABLE IF NOT EXISTS "{{schema}}"."SnapshotEvents" (
    "KeyId" UUID NOT NULL PRIMARY KEY,
    "OwnerId" UUID NOT NULL,
    "Status" VARCHAR(255) NOT NULL,
    "CausationId" VARCHAR(64) NULL,
    "CorrelationId" VARCHAR(64) NULL,
    "CreatedOn" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    "UpdatedOn" TIMESTAMP WITHOUT TIME ZONE NULL,
    "DeletedOn" TIMESTAMP WITHOUT TIME ZONE NULL,
    "EventName" VARCHAR(255) NOT NULL,
    "EventData" TEXT NOT NULL,
    "Sequence" BIGSERIAL NOT NULL
);

CREATE INDEX IF NOT EXISTS "IX_DomainEvent_StreamId" ON "{{schema}}"."DomainEvents" ("StreamId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_DomainEvent_StreamId_StreamVersion_Unique" ON "{{schema}}"."DomainEvents" ("StreamId", "StreamVersion");
CREATE INDEX IF NOT EXISTS "IX_DomainEvent_StreamName" ON "{{schema}}"."DomainEvents" ("StreamName");
CREATE INDEX IF NOT EXISTS "IX_DomainEvents_Sequence" ON "{{schema}}"."DomainEvents" ("Sequence");

CREATE INDEX IF NOT EXISTS "IX_InboxEvent_ClaimId" ON "{{schema}}"."InboxEvents" ("ClaimId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_InboxEvent_EventId_Consumer_Unique" ON "{{schema}}"."InboxEvents" ("KeyId", "Consumer");
CREATE INDEX IF NOT EXISTS "IX_InboxEvent_Processing" ON "{{schema}}"."InboxEvents" ("Status", "NextAttemptOn", "Sequence");
CREATE INDEX IF NOT EXISTS "IX_InboxEvent_Retry" ON "{{schema}}"."InboxEvents" ("Status", "AttemptCount", "NextAttemptOn");
CREATE INDEX IF NOT EXISTS "IX_InboxEvent_Status_NextAttemptOn" ON "{{schema}}"."InboxEvents" ("Status", "NextAttemptOn");
CREATE INDEX IF NOT EXISTS "IX_InboxEvents_Sequence" ON "{{schema}}"."InboxEvents" ("Sequence");

CREATE INDEX IF NOT EXISTS "IX_OutboxEvent_ClaimId" ON "{{schema}}"."OutboxEvents" ("ClaimId");
CREATE INDEX IF NOT EXISTS "IX_OutboxEvent_Processing" ON "{{schema}}"."OutboxEvents" ("Status", "NextAttemptOn", "Sequence");
CREATE INDEX IF NOT EXISTS "IX_OutboxEvent_Retry" ON "{{schema}}"."OutboxEvents" ("Status", "AttemptCount", "NextAttemptOn");
CREATE INDEX IF NOT EXISTS "IX_OutboxEvent_Status_NextAttemptOn" ON "{{schema}}"."OutboxEvents" ("Status", "NextAttemptOn");
CREATE INDEX IF NOT EXISTS "IX_OutboxEvents_Sequence" ON "{{schema}}"."OutboxEvents" ("Sequence");
""";

	/// <inheritdoc />
	public string GetDropAllTablesScript(string schema = "events") => $$"""
DROP TABLE IF EXISTS "{{schema}}"."SnapshotEvents";
DROP TABLE IF EXISTS "{{schema}}"."OutboxEvents";
DROP TABLE IF EXISTS "{{schema}}"."InboxEvents";
DROP TABLE IF EXISTS "{{schema}}"."DomainEvents";
""";
}
