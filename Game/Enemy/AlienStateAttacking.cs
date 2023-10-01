using Godot;
using Squiggles.Core.CharStats;
using Squiggles.Core.Error;
using Squiggles.Core.Extension;
using Squiggles.Core.FSM;

public partial class AlienStateAttacking : State {

  [Export] private CharacterBody3D _actor;
  [Export] private float _speed = 1.5f;
  [Export] private float _damage = 0.5f;
  [Export] private float _attackSpeed = 1.5f;

  private Node3D _player;

  public override void EnterState() {
    SetPhysicsProcess(true);
    _player = GetTree().GetFirstNodeInGroup("player") as Node3D;
  }
  public override void ExitState() => SetPhysicsProcess(false);

  public override void _PhysicsProcess(double delta) {
    var dir = _player.GlobalPosition - _actor.GlobalPosition;
    dir.Y = 0;
    _actor.Velocity = dir.Normalized() * _speed;
    var data = _actor.MoveAndCollide(_actor.Velocity * (float)delta);
    if (data is null) { return; }

    if (data.GetCollider() is Player player) {
      var stats = player.GetComponent<CharStatManager>();
      stats?.ModifyStaticStat("Health", -_damage); // delta must be negative to reduce bc that's how math works.
      Print.Debug($"Hit player: HP: {stats?.GetStat("Health")}");
      EmitSignal(nameof(OnStateFinished));
    }
  }


}
