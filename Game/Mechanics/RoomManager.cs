using Godot;
using Squiggles.Core.Error;
using Squiggles.Core.Scenes.Utility.Camera;
using System;
using System.Threading.Tasks;

public partial class RoomManager : Area3D {

  [Export(PropertyHint.Enum, "captain_quarters,biomass_room,main_storage,west_crew_quarters,west_thruster,engineering_workshop,medical_bay,east_thruster,cafeteria,east_crew_quarters,sensor_array")]
  private string _roomName = "captain_quarters";

  public string RoomName => _roomName;

  [Export] private RoomManager[] _adjacentRooms;
  [Export] private VirtualCamera _combatCamera;
  [Export] private Timer _timer;

  public VirtualCamera RoomCamera => _combatCamera;
  public bool HasAliens { get; private set; }
  public bool HasOxygen { get; private set; }

  public event Action OnCombatOver;

  private bool _isCombatActive;

  public override void _Ready() {
    ShipManager.OnRoomStatusUpdated += HandleRoomStatusUpdate;
    HandleRoomStatusUpdate(_roomName, ShipManager.GetStatusFor(_roomName));
    BodyEntered += (body) => {
      if (body is Player player) {
        HandlePlayerEnter(player);
      }
    };
    BodyExited += (body) => {
      if (body is Player player) {
        HandleCombatEnd(player);
      }
    };
  }

  private async void HandlePlayerEnter(Player player) {
    if (!HasAliens) {
      // room is safe
      return;
    }
    _isCombatActive = true;
    player.OnEnterRoom(this);
    _combatCamera.PushVCam();
    await Task.Delay(15000);
    HandleCombatEnd(player);
  }

  private void HandleCombatEnd(Player player) {
    if (!_isCombatActive) { return; } // avoids duplicate calls
    _combatCamera.PopVCam();
    OnCombatOver?.Invoke();
    _isCombatActive = false;
  }

  private void HandleRoomStatusUpdate(string room, int status) {
    if (room != _roomName) {
      // not for us
      return;
    }
    HasAliens = status != 0;
    HasOxygen = status != 2;
    Print.Debug($"Room Status [{_roomName}]: HasAliens={HasAliens}, HasOxygen={HasOxygen}");
  }

}
