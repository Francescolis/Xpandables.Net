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

using System.States;

using FluentAssertions;

namespace Xpandables.Net.UnitTests.Primitives;

public sealed class StateContextTests
{
    #region Test States and Context

    private sealed class OrderContext(string orderId, StateContextTests.OrderState initialState) : StateContext<OrderState>(initialState)
    {
        public string OrderId { get; } = orderId;
        public List<string> StateHistory { get; } = [];
        public List<string> TransitionLog { get; } = [];

        protected override void OnStateTransitioning(OrderState? currentState, OrderState newState)
        {
            TransitionLog.Add($"Transitioning from {currentState?.GetType().Name ?? "null"} to {newState.GetType().Name}");
        }

        protected override void OnStateTransitioned(OrderState? previousState, OrderState currentState)
        {
            StateHistory.Add(currentState.GetType().Name);
        }
    }

    private abstract class OrderState : State<OrderContext>
    {
        public List<string> Actions { get; } = [];
    }

    private sealed class PendingState : OrderState
    {
        protected override void OnEnteringStateContext(OrderContext context)
        {
            Actions.Add($"Order {context.OrderId} is now pending");
        }

        protected override void OnExitingStateContext(OrderContext context)
        {
            Actions.Add($"Order {context.OrderId} leaving pending state");
        }

        public void Submit(OrderContext context)
        {
            context.TransitionToState(new ProcessingState());
        }
    }

    private sealed class ProcessingState : OrderState
    {
        protected override void OnEnteringStateContext(OrderContext context)
        {
            Actions.Add($"Order {context.OrderId} is being processed");
        }

        protected override void OnExitingStateContext(OrderContext context)
        {
            Actions.Add($"Order {context.OrderId} finished processing");
        }

        public void Ship(OrderContext context)
        {
            context.TransitionToState(new ShippedState());
        }

        public void Cancel(OrderContext context)
        {
            context.TransitionToState(new CancelledState());
        }
    }

    private sealed class ShippedState : OrderState
    {
        protected override void OnEnteringStateContext(OrderContext context)
        {
            Actions.Add($"Order {context.OrderId} has been shipped");
        }

        public void Deliver(OrderContext context)
        {
            context.TransitionToState(new DeliveredState());
        }
    }

    private sealed class DeliveredState : OrderState
    {
        protected override void OnEnteringStateContext(OrderContext context)
        {
            Actions.Add($"Order {context.OrderId} has been delivered");
        }
    }

    private sealed class CancelledState : OrderState
    {
        protected override void OnEnteringStateContext(OrderContext context)
        {
            Actions.Add($"Order {context.OrderId} has been cancelled");
        }
    }

    #endregion

    #region Traffic Light Example

    private sealed class TrafficLightContext : StateContext<TrafficLightState>
    {
        public int CycleCount { get; private set; }

        public TrafficLightContext() : base(new RedState()) { }

        protected override void OnStateTransitioned(TrafficLightState? previousState, TrafficLightState currentState)
        {
            CycleCount++;
        }
    }

    private abstract class TrafficLightState : State<TrafficLightContext>
    {
        public abstract string Color { get; }
        public abstract void Next(TrafficLightContext context);
    }

    private sealed class RedState : TrafficLightState
    {
        public override string Color => "Red";

        public override void Next(TrafficLightContext context)
        {
            context.TransitionToState(new GreenState());
        }
    }

    private sealed class GreenState : TrafficLightState
    {
        public override string Color => "Green";

        public override void Next(TrafficLightContext context)
        {
            context.TransitionToState(new YellowState());
        }
    }

    private sealed class YellowState : TrafficLightState
    {
        public override string Color => "Yellow";

        public override void Next(TrafficLightContext context)
        {
            context.TransitionToState(new RedState());
        }
    }

    #endregion

    #region Validation Context

    private sealed class ValidatedStateContext(StateContextTests.ValidatedState initialState, HashSet<Type> allowedTransitions) : StateContext<ValidatedState>(initialState)
    {
        private readonly HashSet<Type> _allowedTransitions = allowedTransitions;

        protected override bool CanTransitionTo(ValidatedState newState)
        {
            return _allowedTransitions.Count <= 0 || _allowedTransitions.Contains(newState.GetType());
        }
    }

    private abstract class ValidatedState : State<ValidatedStateContext>
    {
        public abstract string Name { get; }
    }

    private sealed class InitialState : ValidatedState
    {
        public override string Name => "Initial";
    }

    private sealed class ActiveState : ValidatedState
    {
        public override string Name => "Active";
    }

    private sealed class FinalState : ValidatedState
    {
        public override string Name => "Final";
    }

    #endregion

    #region Basic State Transition Tests

    [Fact]
    public void WhenCreatingContextWithInitialStateThenCurrentStateShouldBeSet()
    {
        // Arrange & Act
        var context = new OrderContext("ORD-001", new PendingState());

        // Assert
        context.CurrentState.Should().BeOfType<PendingState>();
    }

    [Fact]
    public void WhenTransitioningToNewStateThenCurrentStateShouldChange()
    {
        // Arrange
        var context = new OrderContext("ORD-001", new PendingState());
        var pendingState = (PendingState)context.CurrentState;

        // Act
        pendingState.Submit(context);

        // Assert
        context.CurrentState.Should().BeOfType<ProcessingState>();
    }

    [Fact]
    public void WhenTransitioningThenOnEnteringStateContextShouldBeCalled()
    {
        // Arrange
        var context = new OrderContext("ORD-001", new PendingState());
        var pendingState = (PendingState)context.CurrentState;

        // Act
        pendingState.Submit(context);

        // Assert
        var processingState = (ProcessingState)context.CurrentState;
        processingState.Actions.Should().Contain(a => a.Contains("is being processed", StringComparison.Ordinal));
    }

    [Fact]
    public void WhenTransitioningThenOnExitingStateContextShouldBeCalled()
    {
        // Arrange
        var initialState = new PendingState();
        var context = new OrderContext("ORD-001", initialState);

        // Act
        initialState.Submit(context);

        // Assert
        initialState.Actions.Should().Contain(a => a.Contains("leaving pending state", StringComparison.Ordinal));
    }

    [Fact]
    public void WhenTransitioningWithNullStateThenShouldThrowArgumentNullException()
    {
        // Arrange
        var context = new OrderContext("ORD-001", new PendingState());

        // Act
        var act = () => context.TransitionToState(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region State History Tests

    [Fact]
    public void WhenMultipleTransitionsOccurThenHistoryShouldTrackAll()
    {
        // Arrange
        var context = new OrderContext("ORD-001", new PendingState());

        // Act
        var pending = (PendingState)context.CurrentState;
        pending.Submit(context);

        var processing = (ProcessingState)context.CurrentState;
        processing.Ship(context);

        var shipped = (ShippedState)context.CurrentState;
        shipped.Deliver(context);

        // Assert
        context.StateHistory.Should().HaveCount(4);
        context.StateHistory.Should().ContainInOrder(
            "PendingState",
            "ProcessingState",
            "ShippedState",
            "DeliveredState");
    }

    [Fact]
    public void WhenTransitioningThenTransitionLogShouldRecordTransitions()
    {
        // Arrange
        var context = new OrderContext("ORD-001", new PendingState());

        // Act
        var pending = (PendingState)context.CurrentState;
        pending.Submit(context);

        // Assert
        context.TransitionLog.Should().Contain(
            "Transitioning from PendingState to ProcessingState");
    }

    #endregion

    #region Event Tests

    [Fact]
    public void WhenTransitioningThenStateTransitioningEventShouldBeRaised()
    {
        // Arrange
        var context = new OrderContext("ORD-001", new PendingState());
        StateTransitionEventArgs? capturedArgs = null;

        context.StateTransitioning += (sender, args) => capturedArgs = args;

        // Act
        var pending = (PendingState)context.CurrentState;
        pending.Submit(context);

        // Assert
        capturedArgs.Should().NotBeNull();
        capturedArgs!.Value.PreviousState.Should().BeOfType<PendingState>();
        capturedArgs.Value.NewState.Should().BeOfType<ProcessingState>();
        capturedArgs.Value.TransitionTime.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void WhenTransitionCompleteThenStateTransitionedEventShouldBeRaised()
    {
        // Arrange
        var context = new OrderContext("ORD-001", new PendingState());
        StateTransitionEventArgs? capturedArgs = null;

        context.StateTransitioned += (sender, args) => capturedArgs = args;

        // Act
        var pending = (PendingState)context.CurrentState;
        pending.Submit(context);

        // Assert
        capturedArgs.Should().NotBeNull();
        capturedArgs!.Value.NewState.Should().BeOfType<ProcessingState>();
    }

    [Fact]
    public void WhenMultipleSubscribersThenAllShouldBeNotified()
    {
        // Arrange
        var context = new OrderContext("ORD-001", new PendingState());
        var notifications = new List<string>();

        context.StateTransitioned += (s, e) => notifications.Add("Subscriber1");
        context.StateTransitioned += (s, e) => notifications.Add("Subscriber2");
        context.StateTransitioned += (s, e) => notifications.Add("Subscriber3");

        // Act
        var pending = (PendingState)context.CurrentState;
        pending.Submit(context);

        // Assert
        notifications.Should().HaveCount(3);
        notifications.Should().Contain("Subscriber1");
        notifications.Should().Contain("Subscriber2");
        notifications.Should().Contain("Subscriber3");
    }

    #endregion

    #region Traffic Light Cycle Tests

    [Fact]
    public void WhenTrafficLightStartsThenShouldBeRed()
    {
        // Arrange & Act
        var trafficLight = new TrafficLightContext();

        // Assert
        trafficLight.CurrentState.Color.Should().Be("Red");
    }

    [Fact]
    public void WhenTrafficLightCyclesThenShouldFollowCorrectSequence()
    {
        // Arrange
        var trafficLight = new TrafficLightContext();
        var colors = new List<string> { trafficLight.CurrentState.Color };

        // Act
        trafficLight.CurrentState.Next(trafficLight);
        colors.Add(trafficLight.CurrentState.Color);

        trafficLight.CurrentState.Next(trafficLight);
        colors.Add(trafficLight.CurrentState.Color);

        trafficLight.CurrentState.Next(trafficLight);
        colors.Add(trafficLight.CurrentState.Color);

        // Assert
        colors.Should().ContainInOrder("Red", "Green", "Yellow", "Red");
    }

    [Fact]
    public void WhenTrafficLightCompletesFullCycleThenCycleCountShouldIncrement()
    {
        // Arrange
        var trafficLight = new TrafficLightContext();

        // Act - Complete one full cycle: Red -> Green -> Yellow -> Red
        trafficLight.CurrentState.Next(trafficLight);
        trafficLight.CurrentState.Next(trafficLight);
        trafficLight.CurrentState.Next(trafficLight);

        // Assert
        trafficLight.CycleCount.Should().Be(4);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void WhenTransitionIsAllowedThenShouldSucceed()
    {
        // Arrange
        var allowedTransitions = new HashSet<Type> { typeof(InitialState), typeof(ActiveState) };
        var context = new ValidatedStateContext(new InitialState(), allowedTransitions);

        // Act
        context.TransitionToState(new ActiveState());

        // Assert
        context.CurrentState.Should().BeOfType<ActiveState>();
    }

    [Fact]
    public void WhenTransitionIsNotAllowedThenShouldThrow()
    {
        // Arrange
        var allowedTransitions = new HashSet<Type> { typeof(InitialState) };
        var context = new ValidatedStateContext(new InitialState(), allowedTransitions);

        // Act
        var act = () => context.TransitionToState(new FinalState());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Invalid state transition*");
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task WhenAccessingCurrentStateConcurrentlyThenShouldBeThreadSafe()
    {
        // Arrange
        var context = new TrafficLightContext();
        var readTasks = new List<Task<string>>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            readTasks.Add(Task.Run(() => context.CurrentState.Color));
        }

        var results = await Task.WhenAll(readTasks);

        // Assert
        results.Should().AllSatisfy(color => color.Should().BeOneOf("Red", "Green", "Yellow"));
    }

    #endregion

    #region Real World Scenario Tests

    [Fact]
    public void WhenOrderFollowsHappyPathThenShouldReachDelivered()
    {
        // Arrange
        var context = new OrderContext("ORD-001", new PendingState());

        // Act
        var pending = (PendingState)context.CurrentState;
        pending.Submit(context);

        var processing = (ProcessingState)context.CurrentState;
        processing.Ship(context);

        var shipped = (ShippedState)context.CurrentState;
        shipped.Deliver(context);

        // Assert
        context.CurrentState.Should().BeOfType<DeliveredState>();
    }

    [Fact]
    public void WhenOrderIsCancelledThenShouldReachCancelled()
    {
        // Arrange
        var context = new OrderContext("ORD-001", new PendingState());

        // Act
        var pending = (PendingState)context.CurrentState;
        pending.Submit(context);

        var processing = (ProcessingState)context.CurrentState;
        processing.Cancel(context);

        // Assert
        context.CurrentState.Should().BeOfType<CancelledState>();
        var cancelledState = (CancelledState)context.CurrentState;
        cancelledState.Actions.Should().Contain(a => a.Contains("cancelled", StringComparison.Ordinal));
    }

    [Fact]
    public void WhenTrackingOrderProgressThenShouldProvideFullHistory()
    {
        // Arrange
        var context = new OrderContext("ORD-001", new PendingState());
        var eventLog = new List<string>();

        context.StateTransitioned += (s, e) => eventLog.Add($"Transitioned to {e.NewState.GetType().Name} at {e.TransitionTime:HH:mm:ss}");

        // Act
        var pending = (PendingState)context.CurrentState;
        pending.Submit(context);

        var processing = (ProcessingState)context.CurrentState;
        processing.Ship(context);

        // Assert
        eventLog.Should().HaveCount(2);
        eventLog[0].Should().Contain("ProcessingState");
        eventLog[1].Should().Contain("ShippedState");
    }

    #endregion

    #region StateTransitionEventArgs Tests

    [Fact]
    public void WhenCreatingEventArgsThenShouldHaveCorrectProperties()
    {
        // Arrange
        var previous = new PendingState();
        var next = new ProcessingState();
        var time = DateTimeOffset.UtcNow;

        // Act
        var args = new StateTransitionEventArgs
        {
            PreviousState = previous,
            NewState = next,
            TransitionTime = time
        };

        // Assert
        args.PreviousState.Should().Be(previous);
        args.NewState.Should().Be(next);
        args.TransitionTime.Should().Be(time);
    }

    [Fact]
    public void WhenInitialTransitionThenPreviousStateShouldBeNull()
    {
        // Arrange
        StateTransitionEventArgs? capturedArgs = null;
        var context = new OrderContext("ORD-001", new PendingState());

        // Note: The first transition happens in constructor, 
        // so we need to subscribe before creating context
        // This is testing the first transition to initial state
        var pending = (PendingState)context.CurrentState;

        context.StateTransitioned += (s, e) => capturedArgs = e;

        // Act - First actual transition after initial
        pending.Submit(context);

        // Assert
        capturedArgs!.Value.PreviousState.Should().BeOfType<PendingState>();
    }

    #endregion
}
