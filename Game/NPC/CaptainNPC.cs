using System.Linq;
using Godot;
using Squiggles.Core.Error;
using Squiggles.Core.FSM;

public partial class CaptainNPC : Node3D {

  [Signal] public delegate void OnStartRequestedCoffeeEventHandler();

  [Export] private FiniteStateMachine _fsm;
  public override void _Ready() {

    // connect states in sequence. Each state determines the conditions for leaving
    State lastState = null;
    var states = _fsm.GetChildren().Cast<State>().Where((state) => state is not null);
    foreach (var state in states) {
      if (lastState is not null) {
        Print.Debug($"Captain connecting [{lastState.Name}].end -> [{state.Name}]");
        lastState.OnStateFinished += () => _fsm.ChangeState(state);
      }
      lastState = state;
    }
  }

  private void RelaySignal_RequestedCoffee() => EmitSignal(nameof(OnStartRequestedCoffee));
}
