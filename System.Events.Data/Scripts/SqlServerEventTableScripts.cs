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
/// SQL Server event table scripts based on AddEventContext migration.
/// </summary>
public sealed class SqlServerEventTableScripts : IEventTableScriptProvider
{
    /// <inheritdoc />
    public string GetCreateAllTablesScript(string schema = "Events") => $$"""
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = '{{schema}}')
    EXEC('CREATE SCHEMA [{{schema}}]');

CREATE TABLE [{{schema}}].[DomainEvents] (
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

CREATE TABLE [{{schema}}].[InboxEvents] (
    [KeyId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [ErrorMessage] NVARCHAR(MAX) NULL,
    [AttemptCount] INT NOT NULL CONSTRAINT DF_InboxEvents_AttemptCount DEFAULT 0,
    [NextAttemptOn] DATETIME2 NULL,
    [ClaimId] UNIQUEIDENTIFIER NULL,
    [Consumer] NVARCHAR(256) NOT NULL,
    [Status] NVARCHAR(255) NOT NULL,
    [CausationId] NVARCHAR(64) NULL,
    [CorrelationId] NVARCHAR(64) NULL,
    [CreatedOn] DATETIME2 NOT NULL,
    [UpdatedOn] DATETIME2 NULL,
    [DeletedOn] DATETIME2 NULL,
    [Sequence] BIGINT IDENTITY(1,1) NOT NULL
);

CREATE TABLE [{{schema}}].[OutboxEvents] (
    [KeyId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [ErrorMessage] NVARCHAR(MAX) NULL,
    [AttemptCount] INT NOT NULL CONSTRAINT DF_OutboxEvents_AttemptCount DEFAULT 0,
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

CREATE TABLE [{{schema}}].[SnapshotEvents] (
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

CREATE INDEX [IX_DomainEvent_StreamId] ON [{{schema}}].[DomainEvents] ([StreamId]);
CREATE UNIQUE INDEX [IX_DomainEvent_StreamId_StreamVersion_Unique] ON [{{schema}}].[DomainEvents] ([StreamId], [StreamVersion]);
CREATE INDEX [IX_DomainEvent_StreamName] ON [{{schema}}].[DomainEvents] ([StreamName]);
CREATE INDEX [IX_DomainEvents_Sequence] ON [{{schema}}].[DomainEvents] ([Sequence]);

CREATE INDEX [IX_InboxEvent_ClaimId] ON [{{schema}}].[InboxEvents] ([ClaimId]);
CREATE UNIQUE INDEX [IX_InboxEvent_EventId_Consumer_Unique] ON [{{schema}}].[InboxEvents] ([KeyId], [Consumer]);
CREATE INDEX [IX_InboxEvent_Processing] ON [{{schema}}].[InboxEvents] ([Status], [NextAttemptOn], [Sequence]);
CREATE INDEX [IX_InboxEvent_Retry] ON [{{schema}}].[InboxEvents] ([Status], [AttemptCount], [NextAttemptOn]);
CREATE INDEX [IX_InboxEvent_Status_NextAttemptOn] ON [{{schema}}].[InboxEvents] ([Status], [NextAttemptOn]);
CREATE INDEX [IX_InboxEvents_Sequence] ON [{{schema}}].[InboxEvents] ([Sequence]);

CREATE INDEX [IX_OutboxEvent_ClaimId] ON [{{schema}}].[OutboxEvents] ([ClaimId]);
CREATE INDEX [IX_OutboxEvent_Processing] ON [{{schema}}].[OutboxEvents] ([Status], [NextAttemptOn], [Sequence]);
CREATE INDEX [IX_OutboxEvent_Retry] ON [{{schema}}].[OutboxEvents] ([Status], [AttemptCount], [NextAttemptOn]);
CREATE INDEX [IX_OutboxEvent_Status_NextAttemptOn] ON [{{schema}}].[OutboxEvents] ([Status], [NextAttemptOn]);
CREATE INDEX [IX_OutboxEvents_Sequence] ON [{{schema}}].[OutboxEvents] ([Sequence]);
""";

    /// <inheritdoc />
    public string GetDropAllTablesScript(string schema = "Events") => $$"""
DROP TABLE IF EXISTS [{{schema}}].[SnapshotEvents];
DROP TABLE IF EXISTS [{{schema}}].[OutboxEvents];
DROP TABLE IF EXISTS [{{schema}}].[InboxEvents];
DROP TABLE IF EXISTS [{{schema}}].[DomainEvents];
""";
}
