using Godot;
using Squiggles.Core.Error;
using Squiggles.Core.Events;
using Squiggles.Core.Extension;
using Squiggles.Core.FSM;
using Squiggles.Core.Scenes.Character;
using Squiggles.Core.Scenes.Interactables;
using System;
using System.Threading.Tasks;

public partial class StateConsumeItem : State {
  [Export] private string _trackWaiting;
  [Export] private string _trackSuccessful;
  [Export] private string _itemNeeded;
  [Export] private InteractiveTrigger _interaction;
  private Node _dialogic;
  private bool _hasItem;

  public override void _Ready() {
    base._Ready();
    _dialogic = GetNode("/root/Dialogic");
  }

  public override void EnterState() => ConnectInteraction();

  private async void ConnectInteraction() {
    Print.Debug($"[{Name}] entered state. Connection interaction");
    _interaction.IsActive = false;
    await Task.Delay(500);
    _interaction.IsActive = true;
    _interaction.OnInteracted += OnInteraction;
  }

  private void OnInteraction() {
    var track = _trackWaiting;
    if (TryConsumeItem(_itemNeeded) || _hasItem) {
      track = _trackSuccessful;
      _hasItem = true;
    }
    Print.Debug($"[{Name}] Interaction detected. Starting track: {track}");

    EventBus.Gameplay.TriggerRequestPlayerAbleToMove(false);
    _dialogic.Connect("timeline_ended", Callable.From(OnTrackEnd), (uint)ConnectFlags.OneShot);
    _dialogic.Call("start", track);
  }

  private bool TryConsumeItem(string itemName) {
    if (GetTree().GetFirstNodeInGroup("player")?.GetComponent<InventoryManager>() is not InventoryManager items) {
      Print.Warn("failed to get player inventory manager");
      return false;
    }
    var slot = -1;
    items.DoForSlots((index, item, qty) => {
      if (item == itemName) {
        slot = index;
      }
    });
    if (slot < 0) {
      Print.Warn("Sensor array failed to find sensor array disk");
      return false;
    }
    items.SetSelection(slot);
    var result = items.RemoveItem();
    Print.Debug($"Tried to consume item {itemName} from player. Was successful? {result}");
    return result;
  }

  private void OnTrackEnd() {
    EventBus.Gameplay.TriggerRequestPlayerAbleToMove(true);
    if (_hasItem) {
      Print.Debug($"[{Name}] calling state exit");
      EmitSignal(nameof(OnStateFinished));
    }
  }

  public override void ExitState() {
    Print.Debug($"[{Name}] exiting state");
    _interaction.OnInteracted -= OnInteraction;
  }
}
