using Godot;
using Squiggles.Core.Events;
using Squiggles.Core.FSM;
using Squiggles.Core.Scenes.Interactables;

public partial class StatePlayTrack : State {

  [Export] private string _trackName;
  [Export] private InteractiveTrigger _interaction;
  private Node _dialogic;

  public override void _Ready() {
    base._Ready();
    _dialogic = GetNode("/root/Dialogic");
  }

  public override void EnterState() => _interaction.OnInteracted += OnInteraction;

  private void OnInteraction() {
    EventBus.Gameplay.TriggerRequestPlayerAbleToMove(false);
    _dialogic.Call("start", _trackName);
    var call = Callable.From(OnTrackEnd);
    if (!_dialogic.IsConnected("timeline_ended", call)) {
      _dialogic.Connect("timeline_ended", call, (uint)ConnectFlags.OneShot);
      // call OnTrackEnd when "timeline_ended"
    }
  }

  private void OnTrackEnd() => EmitSignal(nameof(OnStateFinished));

  public override void ExitState() {
    EventBus.Gameplay.TriggerRequestPlayerAbleToMove(true);
    _interaction.OnInteracted -= OnInteraction;
  }
}
