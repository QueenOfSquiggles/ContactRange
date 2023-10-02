using Godot;
using Squiggles.Core.FSM;

public partial class CaptainNPC : Node3D {

  [Export] private FiniteStateMachine _fsm;
  public override void _Ready() {

    // connect states in sequence. Each state determines the conditions for leaving
    State lastState = null;
    foreach (var state in _fsm.GetChildren()) {
      if (lastState is not null) {
        lastState.OnStateFinished += () => _fsm.ChangeState(state as State);
      }
      lastState = state as State;
    }
  }
  public void OnInteract() {
  }

}
