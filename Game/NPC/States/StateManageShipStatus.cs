using Godot;
using Squiggles.Core.Error;
using Squiggles.Core.Events;
using Squiggles.Core.FSM;
using Squiggles.Core.Scenes.Interactables;
using System;
using System.Threading.Tasks;

public partial class StateManageShipStatus : State {
  [Export] private string _trackFixSensors;
  [Export] private string _trackThrusterOnFire;
  [Export] private string _trackThrusterOutOfFuel;
  [Export] private string _trackExterminate;
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

    var track = GetTrack();
    if (track == "") {
      EmitSignal(nameof(OnStateFinished));
      return;
    }

    EventBus.Gameplay.TriggerRequestPlayerAbleToMove(false);
    _dialogic.Connect("timeline_ended", Callable.From(() => OnTrackEnd()), (uint)ConnectFlags.OneShot);
    _dialogic.Call("start", track);
    Print.Debug($"[{Name}] Captain dialog started: {track}");
  }

  private string GetTrack() {
    var track = "";
    if (!ShipManager.GetFlagFor("aliens_exterminated")) {
      track = _trackExterminate;
    }
    if (!ShipManager.GetFlagFor("east_thruster_refueled") || !ShipManager.GetFlagFor("west_thruster_refueled")) {
      track = _trackThrusterOutOfFuel;
    }
    if (!ShipManager.GetFlagFor("east_thruster_fire_extinguished") || !ShipManager.GetFlagFor("west_thruster_fire_extinguished")) {
      track = _trackThrusterOnFire;
    }
    if (!ShipManager.GetFlagFor("sensor_array_active")) {
      track = _trackFixSensors;
    }
    return track;
  }

  private void OnTrackEnd() {
    EventBus.Gameplay.TriggerRequestPlayerAbleToMove(true);
    if (GetTrack() == "") {
      EmitSignal(nameof(OnStateFinished));
    }
  }

  public override void ExitState() {
    Print.Debug($"[{Name}] Captain state exiting");
    _interaction.OnInteracted -= OnInteraction;
  }
}
