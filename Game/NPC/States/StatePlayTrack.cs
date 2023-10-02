using System.Threading.Tasks;
using Godot;
using Squiggles.Core.Error;
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

  public override void EnterState() => ConnectInteraction();

  private async void ConnectInteraction() {
    await Task.Delay(500);
    _interaction.OnInteracted += OnInteraction;
  }


  private void OnInteraction() {
    EventBus.Gameplay.TriggerRequestPlayerAbleToMove(false);
    _dialogic.Connect("timeline_ended", Callable.From(() => OnTrackEnd()), (uint)ConnectFlags.OneShot);
    _dialogic.Call("start", _trackName);
    Print.Debug($"[{Name}] Captain dialog started: {_trackName}");
  }

  private void OnTrackEnd() {
    Print.Debug($"[{Name}] Captain dialog ended: {_trackName}");
    EventBus.Gameplay.TriggerRequestPlayerAbleToMove(true);
    EmitSignal(nameof(OnStateFinished));
  }

  public override void ExitState() {
    Print.Debug($"[{Name}] Captain state exiting");
    _interaction.OnInteracted -= OnInteraction;
  }
}
