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

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:System.Events.HostedScheduler.ExecuteAsync(System.Threading.CancellationToken)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>", Scope = "member", Target = "~M:System.Events.IEventPublisher.PublishAsync``1(``0,System.Threading.CancellationToken)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>", Scope = "member", Target = "~M:System.Events.IEventHandlerWrapper.HandleAsync(System.Object,System.Threading.CancellationToken)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "<Pending>", Scope = "type", Target = "~T:System.Events.IEventHandler`1")]
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>", Scope = "member", Target = "~M:System.Events.IEventHandler`1.HandleAsync(`0,System.Threading.CancellationToken)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>", Scope = "member", Target = "~M:System.Events.IEventBus.PublishAsync(System.Events.Integration.IIntegrationEvent,System.Threading.CancellationToken)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>", Scope = "member", Target = "~M:System.Events.Integration.IPendingIntegrationEventsBuffer.Add(System.Events.Integration.IIntegrationEvent)")]
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>", Scope = "member", Target = "~M:System.Events.Domain.IEventSourcing.LoadFromHistory(System.Events.Domain.IDomainEvent)")]
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>", Scope = "member", Target = "~M:System.Events.Domain.IEventSourcing.AppendEvent(System.Events.Domain.IDomainEvent)")]
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>", Scope = "member", Target = "~M:System.Events.Domain.IEventSourcing.AppendVersioningEvent(System.Events.Domain.IDomainEvent)")]
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>", Scope = "member", Target = "~M:System.Events.Domain.ISnapshotStore.AppendSnapshotAsync(System.Events.Domain.ISnapshotEvent,System.Threading.CancellationToken)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>", Scope = "member", Target = "~M:System.Events.Aggregates.Aggregate.UpdateBusinessVersionFromEvent(System.Events.Domain.IDomainEvent)")]
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>", Scope = "member", Target = "~M:System.Events.Aggregates.Aggregate.IsSignificantBusinessEvent(System.Events.Domain.IDomainEvent)~System.Boolean")]
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>", Scope = "member", Target = "~M:System.Events.Integration.IInboxStore.ReceiveAsync(System.Events.Integration.IIntegrationEvent,System.String,System.Nullable{System.TimeSpan},System.Threading.CancellationToken)~System.Threading.Tasks.Task{System.Events.Integration.InboxReceiveResult}")]
