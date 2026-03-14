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
/// PostgreSQL event table scripts.
/// </summary>
public sealed class PostgreSqlEventTableScripts : IEventTableScriptProvider
{
	/// <inheritdoc />
	public string GetCreateAllTablesScript(
		string schema = "Event",
		string? eventDomain = "EventDomain",
		string? eventInbox = "EventInbox",
		string? eventOutbox = "EventOutbox",
		string? eventSnapshot = "EventSnapshot") => $$"""
CREATE SCHEMA IF NOT EXISTS "{{schema}}";

CREATE TABLE IF NOT EXISTS "{{schema}}"."{{eventDomain}}" (
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

CREATE TABLE IF NOT EXISTS "{{schema}}"."{{eventInbox}}" (
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

CREATE TABLE IF NOT EXISTS "{{schema}}"."{{eventOutbox}}" (
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

CREATE TABLE IF NOT EXISTS "{{schema}}"."{{eventSnapshot}}" (
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

CREATE INDEX IF NOT EXISTS "IX_{{eventDomain}}_StreamId" ON "{{schema}}"."{{eventDomain}}" ("StreamId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_{{eventDomain}}_StreamId_StreamVersion_Unique" ON "{{schema}}"."{{eventDomain}}" ("StreamId", "StreamVersion");
CREATE INDEX IF NOT EXISTS "IX_{{eventDomain}}_StreamName" ON "{{schema}}"."{{eventDomain}}" ("StreamName");
CREATE INDEX IF NOT EXISTS "IX_{{eventDomain}}_Sequence" ON "{{schema}}"."{{eventDomain}}" ("Sequence");

CREATE INDEX IF NOT EXISTS "IX_{{eventInbox}}_ClaimId" ON "{{schema}}"."{{eventInbox}}" ("ClaimId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_{{eventInbox}}_EventId_Consumer_Unique" ON "{{schema}}"."{{eventInbox}}" ("KeyId", "Consumer");
CREATE INDEX IF NOT EXISTS "IX_{{eventInbox}}_Processing" ON "{{schema}}"."{{eventInbox}}" ("Status", "NextAttemptOn", "Sequence");
CREATE INDEX IF NOT EXISTS "IX_{{eventInbox}}_Retry" ON "{{schema}}"."{{eventInbox}}" ("Status", "AttemptCount", "NextAttemptOn");
CREATE INDEX IF NOT EXISTS "IX_{{eventInbox}}_Status_NextAttemptOn" ON "{{schema}}"."{{eventInbox}}" ("Status", "NextAttemptOn");
CREATE INDEX IF NOT EXISTS "IX_{{eventInbox}}_Sequence" ON "{{schema}}"."{{eventInbox}}" ("Sequence");

CREATE INDEX IF NOT EXISTS "IX_{{eventOutbox}}_ClaimId" ON "{{schema}}"."{{eventOutbox}}" ("ClaimId");
CREATE INDEX IF NOT EXISTS "IX_{{eventOutbox}}_Processing" ON "{{schema}}"."{{eventOutbox}}" ("Status", "NextAttemptOn", "Sequence");
CREATE INDEX IF NOT EXISTS "IX_{{eventOutbox}}_Retry" ON "{{schema}}"."{{eventOutbox}}" ("Status", "AttemptCount", "NextAttemptOn");
CREATE INDEX IF NOT EXISTS "IX_{{eventOutbox}}_Status_NextAttemptOn" ON "{{schema}}"."{{eventOutbox}}" ("Status", "NextAttemptOn");
CREATE INDEX IF NOT EXISTS "IX_{{eventOutbox}}_Sequence" ON "{{schema}}"."{{eventOutbox}}" ("Sequence");
""";

	/// <inheritdoc />
	public string GetDropAllTablesScript(
		string schema = "Event",
		string? eventDomain = "EventDomain",
		string? eventInbox = "EventInbox",
		string? eventOutbox = "EventOutbox",
		string? eventSnapshot = "EventSnapshot") => $$"""
DROP TABLE IF EXISTS "{{schema}}"."{{eventSnapshot}}";
DROP TABLE IF EXISTS "{{schema}}"."{{eventOutbox}}";
DROP TABLE IF EXISTS "{{schema}}"."{{eventInbox}}";
DROP TABLE IF EXISTS "{{schema}}"."{{eventDomain}}";
""";
}
