/*******************************************************************************
 * Copyright (C) 2025 Francis-Black EWANE
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
using System.Net.Abstractions.States;

using FluentAssertions;

namespace System.Net.UnitTests.States;

public class StateContextTests
{
    [Fact]
    public void Constructor_SetsInitialState_AndCallsEnter()
    {
        // Arrange
        var initial = new TestState("Init");

        // Act
        var ctx = new TestStateContext(initial);

        // Assert
        ctx.CurrentState.Should().BeSameAs(initial);
        initial.EnterCount.Should().Be(1);
        initial.ExitCount.Should().Be(0);
        initial.EnteredWithContext.Should().BeSameAs(ctx);
    }

    [Fact]
    public void Constructor_NullInitial_Throws()
    {
        // Act
        var act = () => new TestStateContext(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Transition_RaisesEvents_InOrder_AndCallsExitThenEnter()
    {
        // Arrange
        var order = new List<string>();
        var initial = new TestState("Init", order);
        var next = new TestState("Next", order);
        var ctx = new TestStateContext(initial, order);

        StateTransitionEventArgs? transitioningArgs = null;
        StateTransitionEventArgs? transitionedArgs = null;
        ctx.StateTransitioning += (_, e) => { order.Add("transitioning"); transitioningArgs = e; };
        ctx.StateTransitioned += (_, e) => { order.Add("transitioned"); transitionedArgs = e; };

        // Act
        ctx.TransitionToState(next);

        // Assert ordering including virtual hooks
        order.Should().ContainInOrder("onTransitioning", "transitioning", "exit:Init", "enter:Next", "onTransitioned", "transitioned");

        transitioningArgs.Should().NotBeNull();
        transitioningArgs!.Value.PreviousState.Should().Be(initial);
        transitioningArgs!.Value.NewState.Should().Be(next);
        transitioningArgs!.Value.TransitionTime.Should().BeAfter(DateTimeOffset.MinValue);
        transitioningArgs!.Value.TransitionTime.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

        transitionedArgs.Should().NotBeNull();
        transitionedArgs!.Value.PreviousState.Should().Be(initial);
        transitionedArgs!.Value.NewState.Should().Be(next);
        transitionedArgs!.Value.TransitionTime.Should().BeAfter(DateTimeOffset.MinValue);
        transitionedArgs!.Value.TransitionTime.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

        initial.ExitCount.Should().Be(1);
        next.EnterCount.Should().Be(1);
        ctx.CurrentState.Should().BeSameAs(next);
    }

    [Fact]
    public void Transition_NullState_Throws()
    {
        var ctx = new TestStateContext(new TestState("Init"));
        var act = () => ctx.TransitionToState((TestState)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Transition_Disallowed_ByCanTransitionTo_Throws_AndDoesNotChangeState()
    {
        // Arrange
        var initial = new TestState("Init");
        var next = new TestState("Next");
        var ctx = new TestStateContext(initial)
        {
            CanTransitionPredicate = _ => false
        };

        var transitioningCalled = false;
        var transitionedCalled = false;
        ctx.StateTransitioning += (_, _) => transitioningCalled = true;
        ctx.StateTransitioned += (_, _) => transitionedCalled = true;

        // Act
        var act = () => ctx.TransitionToState(next);

        // Assert
        act.Should().Throw<InvalidOperationException>();
        ctx.CurrentState.Should().BeSameAs(initial);
        next.EnterCount.Should().Be(0);
        initial.ExitCount.Should().Be(0);
        transitioningCalled.Should().BeFalse();
        transitionedCalled.Should().BeFalse();
    }

    [Fact]
    public void IStateContext_ExplicitMembers_Work_AsExpected()
    {
        // Arrange
        var initial = new TestState("Init");
        var next = new TestState("Next");
        var ctx = new TestStateContext(initial);
        IStateContext iface = ctx;

        // Assert CurrentState via non-generic
        iface.CurrentState.Should().BeSameAs(initial);

        // Act via explicit TransitionToState(IState)
        iface.TransitionToState(next);

        // Assert
        ctx.CurrentState.Should().BeSameAs(next);
        iface.CurrentState.Should().BeSameAs(next);
        next.EnterCount.Should().Be(1);
        initial.ExitCount.Should().Be(1);
    }

    [Fact]
    public void IState_NonGenericInterface_Delegates_To_Generic_EnterExit()
    {
        // Arrange
        var state = new TestState("S");
        var ctx = new TestStateContext(new TestState("Init"));
        IState asNonGeneric = state;

        // Act
        asNonGeneric.EnterStateContext(ctx);
        asNonGeneric.ExitStateContext(ctx);

        // Assert
        state.EnterCount.Should().Be(1);
        state.ExitCount.Should().Be(1);
        state.EnteredWithContext.Should().BeSameAs(ctx);
        state.ExitedWithContext.Should().BeSameAs(ctx);
    }

    [Fact]
    public void IState_NonGenericInterface_WrongContextType_Throws_InvalidCast()
    {
        // Arrange
        var state = new TestState("S");
        var wrongCtx = new AnotherStateContext(new AnotherState());
        IState asNonGeneric = state;

        // Act
        var actEnter = () => asNonGeneric.EnterStateContext(wrongCtx);
        var actExit = () => asNonGeneric.ExitStateContext(wrongCtx);

        // Assert
        actEnter.Should().Throw<InvalidCastException>();
        actExit.Should().Throw<InvalidCastException>();
        state.EnterCount.Should().Be(0);
        state.ExitCount.Should().Be(0);
    }

    private sealed class TestStateContext(StateContextTests.TestState initial, List<string>? order = null) : StateContext<TestState>(initial)
    {
        private readonly List<string>? _order = order;
        public Func<TestState, bool>? CanTransitionPredicate { get; set; }

        protected override bool CanTransitionTo(TestState newState)
        {
            return CanTransitionPredicate?.Invoke(newState) ?? true;
        }

        protected override void OnStateTransitioning(TestState? currentState, TestState newState)
        {
            _order?.Add("onTransitioning");
        }

        protected override void OnStateTransitioned(TestState? previousState, TestState currentState)
        {
            _order?.Add("onTransitioned");
        }

        // Promote events visibility for tests
        public new event EventHandler<StateTransitionEventArgs>? StateTransitioning
        {
            add => base.StateTransitioning += value;
            remove => base.StateTransitioning -= value;
        }

        public new event EventHandler<StateTransitionEventArgs>? StateTransitioned
        {
            add => base.StateTransitioned += value;
            remove => base.StateTransitioned -= value;
        }
    }

    private sealed class AnotherStateContext(StateContextTests.AnotherState initial) : StateContext<AnotherState>(initial)
    {
    }

    private sealed class TestState(string name, List<string>? order = null) : State<TestStateContext>
    {
        private readonly List<string>? _order = order;
        public string Name { get; } = name;
        public int EnterCount { get; private set; }
        public int ExitCount { get; private set; }
        public IStateContext? EnteredWithContext { get; private set; }
        public IStateContext? ExitedWithContext { get; private set; }

        protected override void OnEnteringStateContext(TestStateContext context)
        {
            EnterCount++;
            EnteredWithContext = context;
            _order?.Add($"enter:{Name}");
        }

        protected override void OnExitingStateContext(TestStateContext context)
        {
            ExitCount++;
            ExitedWithContext = context;
            _order?.Add($"exit:{Name}");
        }

        public override string ToString() => Name;
    }

    private sealed class AnotherState : State<AnotherStateContext>
    {
        protected override void OnEnteringStateContext(AnotherStateContext context) { }
        protected override void OnExitingStateContext(AnotherStateContext context) { }
    }
}
