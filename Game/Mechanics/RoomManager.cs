using Godot;
using Squiggles.Core.Data;
using Squiggles.Core.Error;
using Squiggles.Core.Extension;
using Squiggles.Core.Scenes.Utility.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class RoomManager : Area3D {

  [Export(PropertyHint.Enum, "captain_quarters,biomass_room,main_storage,west_crew_quarters,west_thruster,engineering_workshop,medical_bay,east_thruster,cafeteria,east_crew_quarters,sensor_array")]
  private string _roomName = "captain_quarters";

  public string RoomName => _roomName;

  [Export] private RoomManager[] _adjacentRooms;
  [Export] private VirtualCamera _combatCamera;
  [Export] private Marker3D _spawnPoint;
  [Export] private float _timeInfectAdjacent = 30.0f;
  [Export] private float _timeKillLifeSupport = 60.0f;
  [Export] private float _chanceInfectAdjacent = 0.25f;
  [Export] private int _minAliens = 1;
  [Export] private int _maxAliens = 2;
  private readonly Dictionary<string, float> _spawnTable = new() {
    {"res://Game/Enemy/alien_scout.tscn", 1.0f},
  };

  public VirtualCamera RoomCamera => _combatCamera;
  public bool HasAliens { get; private set; }
  public bool HasOxygen { get; private set; }

  public event Action OnCombatOver;

  private bool _isCombatActive;
  private int _alienCount;
  private SceneTreeTimer _timer;
  private readonly Random _rand = new();

  public override void _Ready() {
    ShipManager.OnRoomStatusUpdated += HandleRoomStatusUpdate;
    HandleRoomStatusUpdate(_roomName, ShipManager.GetStatusFor(_roomName));
    BodyEntered += (body) => {
      if (body is Player player) {
        HandlePlayerEnter(player);
      }
      else if (body.IsInGroup("alien")) {
        _alienCount++;
        Print.Debug($"[{_roomName}] aliens: {_alienCount}");
      }
    };
  }

  private void HandlePlayerEnter(Player player) {
    if (!HasAliens) {
      // room is safe
      return;
    }
    _timer?.Dispose();
    _isCombatActive = true;
    player.OnEnterRoom(this);
    _combatCamera.PushVCam();
    // spawn aliens
    var count = _rand.NextRange(_maxAliens, _minAliens);
    count = 1;
    _alienCount = 0;
    Print.Debug($"Spawning {count} aliens in {_roomName}. Generating from {_minAliens}-{_maxAliens}");
    for (var i = 0; i < count; i++) {
      var scene = GD.Load<PackedScene>("res://Game/Enemy/alien_scout.tscn");
      var node = scene.Instantiate() as Node3D;
      AddChild(node);
      node.GlobalPosition = _spawnPoint.GlobalPosition;
      node.TreeExited += HandleAlienDie;
      _alienCount++;
    }
    var foundAliens = GetChildren().Where((node) => node.IsInGroup("alien")).ToList();
    _alienCount = foundAliens.Count;
  }

  private void HandleAlienDie() {
    var foundAliens = GetChildren().Where((node) => node.IsInGroup("alien")).ToList();
    _alienCount = foundAliens.Count;
    Print.Debug($"[{_roomName}] aliens: {_alienCount}");
    if (_alienCount <= 0) {
      if (_roomName != "east_crew_quarters" || ShipManager.GetFlagFor("ship_systems_restored")) {
        // until all ship systems are resotored, disallow clearing the east crew quarters
        ShipManager.UpdateStatusFor(_roomName, 0); // cleared rooms reset to zero
      }
      HandleCombatEnd();
    }
  }

  private void HandleCombatEnd() {
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
    switch (status) {
      case 0: // do nothing
        break;
      case 1: // aliens are added
        _minAliens = 1;
        _minAliens = 2;
        GetNewTimer(_timeInfectAdjacent).Timeout += DoInfection;

        break;
      case 2: // life support removed
        _minAliens = 3;
        _minAliens = 6;
        break;
      default:
        Print.Warn("Unhandled room status!!!!!", this);
        break;
    }
    Print.Debug($"[{_roomName}] {(HasAliens ? "aliens" : "")} {(HasOxygen ? "" : "no_o2")}");
  }

  private void DoInfection() {
    // get random adjacent uninfected
    var rand = new Random();
    var available = _adjacentRooms.Where((room) => ShipManager.GetStatusFor(room.RoomName) == 0).ToList();
    if (available.Count > 0) {
      if (rand.PercentChance(_chanceInfectAdjacent)) {
        var target = available[rand.Next() % available.Count];
        ShipManager.UpdateStatusFor(target.RoomName, 1);
      }
      else {
        // try again. Provides some variance in rate of infection. Aslo creates another slider for difficulty
        GetNewTimer(_timeInfectAdjacent).Timeout += DoInfection;

      }
    }
    // prep for next stage
    GetNewTimer(_timeKillLifeSupport).Timeout += () => ShipManager.UpdateStatusFor(_roomName, 2);
  }

  private SceneTreeTimer GetNewTimer(float time) {
    _timer?.Dispose();
    _timer = GetTree().CreateTimer(time, false);
    return _timer;
  }

}
