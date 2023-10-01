using Godot;
using Squiggles.Core.Error;
using Squiggles.Core.Extension;
using Squiggles.Core.FSM;
using System;
using System.Threading.Tasks;

public partial class AlienScout : CharacterBody3D {
  [Export(PropertyHint.File, "*.tscn")] private string _splatterVFXScene;
  [Export(PropertyHint.File, "*.tscn")] private string _hitSprayVFXScene;
  [Export] private FiniteStateMachine _fsm;
  [Export] private AlienStateAttacking _stateAttacking;
  [Export] private AlienStateHitRecover _stateRecover;

  public override void _Ready() {
    _stateRecover.OnStateFinished += () => _fsm.ChangeState(_stateAttacking);
    _stateAttacking.OnStateFinished += () => _fsm.ChangeState(_stateRecover);
  }

  public void ConsumeRoomData(RoomManager room) {
    // TODO:
  }


  public async void HandleStatChange(string key, float value) {
    if (key == "Health") {
      if (value <= 0.0f) {
        // death
        var packed = await GetVFX(_splatterVFXScene);
        var node = packed.Instantiate() as Node3D;
        GetParent().AddChild(node);
        node.GlobalPosition = GlobalPosition;
        node.Rotation = new Vector3(0f, new Random().NextGuass() * 2f * Mathf.Pi, 0f);
        QueueFree();
      }
      else {
        _fsm.ChangeState(_stateRecover);
        var packed = await GetVFX(_hitSprayVFXScene);
        var node = packed.Instantiate() as Node3D;
        GetParent().AddChild(node);
        node.GlobalPosition = GlobalPosition;
      }
    }
  }

  private async Task<PackedScene> GetVFX(string path) {
    if (ResourceLoader.HasCached(path)) {
      return GD.Load(path) as PackedScene;
    }
    var err = ResourceLoader.LoadThreadedRequest(path);
    if (err != Error.Ok) {
      Print.Error($"{err}", this);
      return null;
    }
    var arr = new Godot.Collections.Array();
    do {
      ResourceLoader.LoadThreadedGetStatus(path, arr);
      await Task.Delay(50); // check every 0.05 seconds. effectively 20 fps
    }
    while (arr[0].AsSingle() < 1.0f);
    return ResourceLoader.LoadThreadedGet(path) as PackedScene;
  }

}
