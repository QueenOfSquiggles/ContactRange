using Godot;
using Squiggles.Core.Extension;
using Squiggles.Core.FSM;
using System;

public partial class AlienScout : CharacterBody3D {
  [Export(PropertyHint.File, "*.tscn")] private string _splatterVFXScene;
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


  public void HandleStatChange(string key, float value) {
    if (key == "Health") {
      if (value <= 0.0f) {
        // death
        var packed = GD.Load<PackedScene>(_splatterVFXScene);
        var node = packed.Instantiate() as Node3D;
        GetParent().AddChild(node);
        node.GlobalPosition = GlobalPosition;
        node.Rotation = new Vector3(0f, new Random().NextGuass() * 2f * Mathf.Pi, 0f);
        QueueFree();
      }
      else {
        _fsm.ChangeState(_stateRecover);
      }
    }
  }

}
