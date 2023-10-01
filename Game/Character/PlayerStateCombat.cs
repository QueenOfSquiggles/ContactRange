using Godot;
using Squiggles.Core.CharStats;
using Squiggles.Core.Error;
using Squiggles.Core.Extension;
using Squiggles.Core.FSM;
using Squiggles.Core.Scenes.Character;
using Squiggles.Core.Scenes.Registration;
using Squiggles.Core.Scenes.Utility;
using Squiggles.Core.Scenes.Utility.Camera;
using System;

public partial class PlayerStateCombat : State {

  [Export(PropertyHint.File, "*.tscn")] private string _gameOverScene = "res://Game/Menu/game_over_screen.tscn";
  [Export] private CharStatManager _stats;
  [Export] private CharacterBody3D _actor;
  [Export] private AnimationPlayer _anim;
  [Export] private InventoryManager _inventory;
  [Export] private Area3D _killbox;
  [Export] private Node3D _heldItem;
  [Export] private Texture2D _playerFistTexture;
  [Export] private Sprite3D _heldWeaponSprite;

  [ExportGroup("Movement", "_movement")]
  [Export] private float _movementSpeed = 4.0f;
  [Export] private Sprite3D _combatSprite;

  private VirtualCamera _vcam;
  private Vector2 _inputVector;
  private bool _isOxygenAvailable = true;
  private int _damage = 1;

  public void ConsumeRoomData(RoomManager room) {
    _vcam = room.RoomCamera;
    _isOxygenAvailable = room.HasOxygen;
    _stats.SetStaticStat("AirRegen", _isOxygenAvailable ? 1.0f : 0.0f);
    if (!_isOxygenAvailable) {
      Print.Warn("Entering combat sans oxygen!");
    }
    void exitCombat() {
      _stats.SetStaticStat("AirRegen", 1.0f);
      room.OnCombatOver -= exitCombat;
    }
    room.OnCombatOver += exitCombat;
  }

  public override void _Ready() {
    base._Ready();
    _combatSprite.Visible = false;
    _killbox.BodyEntered += HandleKillboxBody;
  }

  public override void EnterState() {
    SetPhysicsProcess(true);
    _combatSprite.Visible = true;

    _damage = 1;
    var texture = _playerFistTexture;
    _inventory.DoForSlots((slot, id, qty) => {
      var item = RegistrationManager.GetResource<Item>(id);
      if (item is not null && item.IsWeapon && item.DamageValue > _damage) {
        _damage = item.DamageValue; // uses highest damage value weapon
        texture = item.Texture;
      }
    });
    _heldWeaponSprite.Texture = texture;
  }
  public override void ExitState() {
    SetPhysicsProcess(false);
    _combatSprite.Visible = false;
    if (_heldWeaponSprite.Texture == _playerFistTexture) {
      _heldWeaponSprite.Texture = null; // put those fists away outside of fights
    }
  }

  public override void _PhysicsProcess(double delta) {
    HandleMovement((float)delta);
    HandleAttacks();
    if (!_isOxygenAvailable && !_stats.ConsumeDynStat("air", (float)delta)) {
      // returns false when trying to consume more than available. Ergo, we suffocated
      ShipManager.LastDeathMessage = "You suffocated to death. Watch that O2 meter!";
      SceneTransitions.LoadSceneAsync(_gameOverScene, false, "DarkRed");
    }
  }

  private void HandleAttacks() {
    if (!Input.IsActionJustPressed("interact") || _anim.IsPlaying()) {
      return;
    }
    _anim.Play("SwingAttack");
  }


  private void HandleMovement(float delta) {
    var moveIntent = Input.GetVector("left", "right", "backwards", "forwards") // keyboard
        + Input.GetVector("left_joystick", "right_joystick", "backwards_joystick", "forwards_joystick"); // gamepad
    if (moveIntent.LengthSquared() < 0.1f && _actor.Velocity.LengthSquared() < 0.1f) {
      return; // do  no motion.
    }


    var moveDir = new Vector3();
    moveDir += _vcam.GlobalTransform.Up() * moveIntent.Y; // NOTE: altered to simulate 2D controls
    moveDir += _vcam.GlobalTransform.Right() * moveIntent.X;
    moveDir.Y = 0;
    if (moveDir.LengthSquared() > 1.0f) {
      moveDir = moveDir.Normalized();
    }

    var similarity = (moveDir.Dot(_actor.Velocity.Normalized()) * 0.5f) + 0.5f;
    _actor.Velocity = moveDir * _movementSpeed;
    _actor.MoveAndSlide();

    if (moveDir.LengthSquared() > 0.2f) {
      _heldItem.LookAt(_heldItem.GlobalPosition + moveDir, Vector3.Up);
    }
  }

  private void HandleKillboxBody(Node3D body) {
    if (body == _actor) { return; } // don't hurt yourself out there kiddo!
    var enemyStats = body.GetComponent<CharStatManager>();
    if (enemyStats is null || !enemyStats.HasStat("Health")) { return; }
    enemyStats.ModifyStaticStat("Health", -1);
  }
}
