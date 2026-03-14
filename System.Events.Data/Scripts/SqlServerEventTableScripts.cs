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
/// SQL Server event table scripts based on AddEventContext migration.
/// </summary>
public sealed class SqlServerEventTableScripts : IEventTableScriptProvider
{
	/// <inheritdoc />
	public string GetCreateAllTablesScript(
		string schema = "Event",
		string? eventDomain = "DomainEvents",
		string? eventInbox = "InboxEvents",
		string? eventOutbox = "OutboxEvents",
		string? eventSnapshot = "SnapshotEvents") => $$"""
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = '{{schema}}')
    EXEC('CREATE SCHEMA [{{schema}}]');

IF OBJECT_ID('[{{schema}}].[{{eventDomain}}]', 'U') IS NULL
CREATE TABLE [{{schema}}].[{{eventDomain}}] (
    [KeyId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [StreamId] UNIQUEIDENTIFIER NOT NULL,
    [StreamVersion] BIGINT NOT NULL,
    [StreamName] NVARCHAR(450) NOT NULL,
    [Status] NVARCHAR(255) NOT NULL,
    [CausationId] NVARCHAR(64) NULL,
    [CorrelationId] NVARCHAR(64) NULL,
    [CreatedOn] DATETIME2 NOT NULL,
    [UpdatedOn] DATETIME2 NULL,
    [DeletedOn] DATETIME2 NULL,
    [EventName] NVARCHAR(255) NOT NULL,
    [EventData] NVARCHAR(MAX) NOT NULL,
    [Sequence] BIGINT IDENTITY(1,1) NOT NULL
);

IF OBJECT_ID('[{{schema}}].[{{eventInbox}}]', 'U') IS NULL
CREATE TABLE [{{schema}}].[{{eventInbox}}] (
    [KeyId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [ErrorMessage] NVARCHAR(MAX) NULL,
    [AttemptCount] INT NOT NULL CONSTRAINT DF_{{eventInbox}}_AttemptCount DEFAULT 0,
    [NextAttemptOn] DATETIME2 NULL,
    [ClaimId] UNIQUEIDENTIFIER NULL,
    [Consumer] NVARCHAR(256) NOT NULL,
    [Status] NVARCHAR(255) NOT NULL,
    [CausationId] NVARCHAR(64) NULL,
    [CorrelationId] NVARCHAR(64) NULL,
    [CreatedOn] DATETIME2 NOT NULL,
    [UpdatedOn] DATETIME2 NULL,
    [DeletedOn] DATETIME2 NULL,
	[EventName] NVARCHAR(255) NOT NULL,
    [Sequence] BIGINT IDENTITY(1,1) NOT NULL
);

IF OBJECT_ID('[{{schema}}].[{{eventOutbox}}]', 'U') IS NULL
CREATE TABLE [{{schema}}].[{{eventOutbox}}] (
    [KeyId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [ErrorMessage] NVARCHAR(MAX) NULL,
    [AttemptCount] INT NOT NULL CONSTRAINT DF_{{eventOutbox}}_AttemptCount DEFAULT 0,
    [NextAttemptOn] DATETIME2 NULL,
    [ClaimId] UNIQUEIDENTIFIER NULL,
    [Status] NVARCHAR(255) NOT NULL,
    [CausationId] NVARCHAR(64) NULL,
    [CorrelationId] NVARCHAR(64) NULL,
    [CreatedOn] DATETIME2 NOT NULL,
    [UpdatedOn] DATETIME2 NULL,
    [DeletedOn] DATETIME2 NULL,
    [EventName] NVARCHAR(255) NOT NULL,
    [EventData] NVARCHAR(MAX) NOT NULL,
    [Sequence] BIGINT IDENTITY(1,1) NOT NULL
);

IF OBJECT_ID('[{{schema}}].[{{eventSnapshot}}]', 'U') IS NULL
CREATE TABLE [{{schema}}].[{{eventSnapshot}}] (
    [KeyId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [OwnerId] UNIQUEIDENTIFIER NOT NULL,
    [Status] NVARCHAR(255) NOT NULL,
    [CausationId] NVARCHAR(64) NULL,
    [CorrelationId] NVARCHAR(64) NULL,
    [CreatedOn] DATETIME2 NOT NULL,
    [UpdatedOn] DATETIME2 NULL,
    [DeletedOn] DATETIME2 NULL,
    [EventName] NVARCHAR(255) NOT NULL,
    [EventData] NVARCHAR(MAX) NOT NULL,
    [Sequence] BIGINT IDENTITY(1,1) NOT NULL
);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_{{eventDomain}}_StreamId' AND object_id = OBJECT_ID('[{{schema}}].[{{eventDomain}}]'))
    CREATE INDEX [IX_{{eventDomain}}_StreamId] ON [{{schema}}].[{{eventDomain}}] ([StreamId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_{{eventDomain}}_StreamId_StreamVersion_Unique' AND object_id = OBJECT_ID('[{{schema}}].[{{eventDomain}}]'))
    CREATE UNIQUE INDEX [IX_{{eventDomain}}_StreamId_StreamVersion_Unique] ON [{{schema}}].[{{eventDomain}}] ([StreamId], [StreamVersion]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_{{eventDomain}}_StreamName' AND object_id = OBJECT_ID('[{{schema}}].[{{eventDomain}}]'))
    CREATE INDEX [IX_{{eventDomain}}_StreamName] ON [{{schema}}].[{{eventDomain}}] ([StreamName]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_{{eventDomain}}_Sequence' AND object_id = OBJECT_ID('[{{schema}}].[{{eventDomain}}]'))
    CREATE INDEX [IX_{{eventDomain}}_Sequence] ON [{{schema}}].[{{eventDomain}}] ([Sequence]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_{{eventInbox}}_ClaimId' AND object_id = OBJECT_ID('[{{schema}}].[{{eventInbox}}]'))
    CREATE INDEX [IX_{{eventInbox}}_ClaimId] ON [{{schema}}].[{{eventInbox}}] ([ClaimId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_{{eventInbox}}_EventId_Consumer_Unique' AND object_id = OBJECT_ID('[{{schema}}].[{{eventInbox}}]'))
    CREATE UNIQUE INDEX [IX_{{eventInbox}}_EventId_Consumer_Unique] ON [{{schema}}].[{{eventInbox}}] ([KeyId], [Consumer]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_{{eventInbox}}_Processing' AND object_id = OBJECT_ID('[{{schema}}].[{{eventInbox}}]'))
    CREATE INDEX [IX_{{eventInbox}}_Processing] ON [{{schema}}].[{{eventInbox}}] ([Status], [NextAttemptOn], [Sequence]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_{{eventInbox}}_Retry' AND object_id = OBJECT_ID('[{{schema}}].[{{eventInbox}}]'))
    CREATE INDEX [IX_{{eventInbox}}_Retry] ON [{{schema}}].[{{eventInbox}}] ([Status], [AttemptCount], [NextAttemptOn]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_{{eventInbox}}_Status_NextAttemptOn' AND object_id = OBJECT_ID('[{{schema}}].[{{eventInbox}}]'))
    CREATE INDEX [IX_{{eventInbox}}_Status_NextAttemptOn] ON [{{schema}}].[{{eventInbox}}] ([Status], [NextAttemptOn]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_{{eventInbox}}_Sequence' AND object_id = OBJECT_ID('[{{schema}}].[{{eventInbox}}]'))
    CREATE INDEX [IX_{{eventInbox}}_Sequence] ON [{{schema}}].[{{eventInbox}}] ([Sequence]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_{{eventOutbox}}_ClaimId' AND object_id = OBJECT_ID('[{{schema}}].[{{eventOutbox}}]'))
    CREATE INDEX [IX_{{eventOutbox}}_ClaimId] ON [{{schema}}].[{{eventOutbox}}] ([ClaimId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_{{eventOutbox}}_Processing' AND object_id = OBJECT_ID('[{{schema}}].[{{eventOutbox}}]'))
    CREATE INDEX [IX_{{eventOutbox}}_Processing] ON [{{schema}}].[{{eventOutbox}}] ([Status], [NextAttemptOn], [Sequence]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_{{eventOutbox}}_Retry' AND object_id = OBJECT_ID('[{{schema}}].[{{eventOutbox}}]'))
    CREATE INDEX [IX_{{eventOutbox}}_Retry] ON [{{schema}}].[{{eventOutbox}}] ([Status], [AttemptCount], [NextAttemptOn]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_{{eventOutbox}}_Status_NextAttemptOn' AND object_id = OBJECT_ID('[{{schema}}].[{{eventOutbox}}]'))
    CREATE INDEX [IX_{{eventOutbox}}_Status_NextAttemptOn] ON [{{schema}}].[{{eventOutbox}}] ([Status], [NextAttemptOn]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_{{eventOutbox}}_Sequence' AND object_id = OBJECT_ID('[{{schema}}].[{{eventOutbox}}]'))
    CREATE INDEX [IX_{{eventOutbox}}_Sequence] ON [{{schema}}].[{{eventOutbox}}] ([Sequence]);
""";

	/// <inheritdoc />
	public string GetDropAllTablesScript(
		string schema = "Event",
		string? eventDomain = "EventDomain",
		string? eventInbox = "EventInbox",
		string? eventOutbox = "EventOutbox",
		string? eventSnapshot = "EventSnapshot") => $$"""
DROP TABLE IF EXISTS [{{schema}}].[{{eventSnapshot}}];
DROP TABLE IF EXISTS [{{schema}}].[{{eventOutbox}}];
DROP TABLE IF EXISTS [{{schema}}].[{{eventInbox}}];
DROP TABLE IF EXISTS [{{schema}}].[{{eventDomain}}];
""";
}
