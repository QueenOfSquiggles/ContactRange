using Godot;
using Squiggles.Core.Error;
using Squiggles.Core.Events;
using Squiggles.Core.Extension;
using Squiggles.Core.FSM;
using Squiggles.Core.Interaction;
using Squiggles.Core.Scenes.Character;
using Squiggles.Core.Scenes.Utility.Camera;

public partial class PlayerStateExplore : State {

  [Export] private VirtualCamera _vcam;
  [Export] private CharacterBody3D _actor;
  [Export] private InteractionSensor _interactionSensor;
  [Export] private Node3D _heldItem;
  [Export] private PlayerInventoryInteractive _inventoryDisplay;
  [Export] private InventoryManager _inventoryData;

  [ExportGroup("Movement", "_movement")]
  [Export] private float _movementSpeed = 4.0f;
  [Export] private float _movementAcceleration = 2.0f;
  [Export] private float _movementDeceleration = 1.0f;

  private bool _isInventoryOpen;

  private Vector2 _inputVector;
  public override void _Ready() {
    base._Ready();
    _interactionSensor.OnCurrentInteractionChange += HandleInteractionChanged;
    _inventoryDisplay.Visible = false;
  }
  public override void _ExitTree() => _interactionSensor.OnCurrentInteractionChange -= HandleInteractionChanged;

  public override void EnterState() => SetPhysicsProcess(true);
  public override void ExitState() => SetPhysicsProcess(false);

  public override void _PhysicsProcess(double delta) {
    if (!_isInventoryOpen) {
      HandleMovement((float)delta);
      HandleInteraction();
    }
  }

  private void HandleInteraction() {
    if (!Input.IsActionJustPressed("interact") || _interactionSensor.CurrentInteraction is not IInteractable interact) {
      return;
    }
    interact.Interact();
    _interactionSensor.RefreshCurrent();
  }

  private void HandleMovement(float delta) {
    var moveIntent = Input.GetVector("left", "right", "backwards", "forwards") // keyboard
        + Input.GetVector("left_joystick", "right_joystick", "backwards_joystick", "forwards_joystick"); // gamepad
    if (moveIntent.LengthSquared() < 0.1f && _actor.Velocity.LengthSquared() < 0.1f) {
      return; // do  no motion.
    }


    var moveDir = new Vector3();
    moveDir += _vcam.GlobalTransform.Forward() * moveIntent.Y;
    moveDir += _vcam.GlobalTransform.Right() * moveIntent.X;
    moveDir.Y = 0;
    if (moveDir.LengthSquared() > 1.0f) {
      moveDir = moveDir.Normalized();
    }

    var similarity = (moveDir.Dot(_actor.Velocity.Normalized()) * 0.5f) + 0.5f;
    var lerpFactor = Mathf.Lerp(_movementDeceleration, _movementAcceleration, similarity);
    _actor.Velocity = _actor.Velocity.Lerp(moveDir * _movementSpeed, lerpFactor * (float)delta);
    _actor.MoveAndSlide();

    if (moveDir.LengthSquared() > 0.2f) {
      _heldItem.LookAt(_heldItem.GlobalPosition + moveDir, Vector3.Up);
    }
  }

  private void HandleInteractionChanged() {
    if (_interactionSensor.CurrentInteraction is IInteractable interact) {
      EventBus.GUI.TriggerAbleToInteract(interact.GetActiveName());
    }
    else {
      EventBus.GUI.TriggerUnableToInteract();
    }
  }

  public override void _UnhandledInput(InputEvent @event) {
    if (!IsActive) { return; }
    if (@event.IsActionPressed("open_inventory")) {
      if (_inventoryDisplay.Visible) {
        _inventoryDisplay.Visible = false;
        Input.MouseMode = Input.MouseModeEnum.Captured;
        _isInventoryOpen = false;
      }
      else {
        _inventoryDisplay.ReadInventory(_inventoryData);
        _inventoryDisplay.Visible = true;
        _isInventoryOpen = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
      }
      this.HandleInput();
    }
    if (_isInventoryOpen && @event.IsAction("ui_cancel")) {
      Print.Debug($"{Name} handled 'ui_cancel'");
      this.HandleInput(); // prevents pause menu
    }
  }

}
