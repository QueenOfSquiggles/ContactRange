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
  private float _timeInfectAdjacent = 30.0f;
  private float _timeKillLifeSupport = 60.0f;
  private float _chanceInfectAdjacent = 0.25f;
  private int _minAliens = 1;
  private int _maxAliens = 2;
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
      if (body is Player player && !_isCombatActive) {
        HandlePlayerEnter(player);
      }
    };
    var diff = GameplaySettings.GetFloat("difficulty") / 10f;
    _chanceInfectAdjacent = diff + 0.2f;
    _timeKillLifeSupport = Mathf.Lerp(120, 30, diff);
    _timeInfectAdjacent = Mathf.Lerp(60, 15, diff);
    Print.Debug($"Difficulty Settings: {diff:N2} --> Chance Infect = {_chanceInfectAdjacent:N2}; Time Kill O2 = {_timeKillLifeSupport:N0}; Time Infect Adjacent = {_timeInfectAdjacent:N0}");
  }

  private void HandlePlayerEnter(Player player) {
    if (!HasAliens) {
      // room is safe
      return;
    }
    _isCombatActive = true;
    player.OnEnterRoom(this);
    _combatCamera.PushVCam();
    // spawn aliens
    var count = _rand.NextRange(_maxAliens, _minAliens);
    _alienCount = 0;
    SpawnWave(count);
    // safety check to avoid hard lock bugs
    var foundAliens = GetChildren().Where((node) => node.IsInGroup("alien")).ToList();
    _alienCount = foundAliens.Count;
  }

  private void SpawnWave(int count) {

    Print.Debug($"Spawning {count} aliens in {_roomName}. Generating from {_minAliens}-{_maxAliens}");
    var circle = Mathf.Pi * 2f;
    var angle = circle / ((float)count); // is it really redundant?????
    var distBetween = 2.5f;
    var adjacentAngles = (circle - angle) / 2f;
    var distanceOut = 1.5f; // eddge case where  triangle of infinite length is made when spawning few aliens
    if (angle < Mathf.DegToRad(90f)) {
      distanceOut = distBetween * Mathf.Sin(adjacentAngles) / Mathf.Sin(angle);
    }

    Print.Debug($"Spawning conditions: angle={Mathf.RadToDeg(angle):N2}d, ring radius={distanceOut}m");
    var currentAngle = 0f;
    for (var i = 0; i < count; i++) {
      var path = _spawnTable.Keys.ElementAt(_rand.Next() % _spawnTable.Keys.Count);
      var scene = GD.Load<PackedScene>(path);
      var node = scene.Instantiate() as Node3D;
      AddChild(node);
      var offset = new Vector3(Mathf.Cos(currentAngle) * distanceOut, 0f, Mathf.Sin(currentAngle) * distanceOut);
      node.GlobalPosition = _spawnPoint.GlobalPosition + offset;
      node.TreeExited += HandleAlienDie;
      _alienCount++;
      currentAngle += angle;
    }
  }

  private void CheckCombatStatus() {
    if (!_isCombatActive) { return; }
    var foundAliens = GetChildren().Where((node) => node is not null && IsInstanceValid(node) && node.IsInGroup("alien")).ToList();
    _alienCount = foundAliens.Count;
    Print.Debug($"[{_roomName}] Combat - {_alienCount} aliens left");
    if (_alienCount <= 0) {
      // until all ship systems are resotored, disallow clearing the east crew quarters
      var nextStatus = ShipManager.GetStatusFor(_roomName) - 1;
      _timer.TimeLeft = 500000f; // force to not work.
      if (_roomName == "east_crew_quarters" && !ShipManager.GetFlagFor("ship_systems_restored")) {
        nextStatus = Math.Max(nextStatus, 1); // east crew quarters cannot go lower than 1 until systems restored
      }
      ShipManager.UpdateStatusFor(_roomName, nextStatus); // cleared rooms reduce by one
      Print.Debug($"[{_roomName}] handling combat end");
      HandleCombatEnd();
    }
    else {
      Print.Debug($"[{_roomName}] checked, combat still needed");
    }
  }

  private async void HandleAlienDie() {
    await Task.Delay(100); // give .1 seconds for alien count to update
    CheckCombatStatus();
  }

  private void HandleCombatEnd() {
    if (!_isCombatActive) { return; }

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
        _maxAliens = 2;
        GetNewTimer(_timeInfectAdjacent).Timeout += DoInfection;

        break;
      case 2: // life support removed
        _minAliens = 3;
        _maxAliens = 7;
        break;
      default:
        Print.Warn("Unhandled room status!!!!!", this);
        break;
    }
    Print.Debug($"[{_roomName}] {(HasAliens ? "aliens" : "[s]~~aliens~~[/s]")} {(HasOxygen ? "O2" : "[s]~~O2~~[/s]")}");
  }

  private void DoInfection() {
    // get random adjacent uninfected
    var rand = new Random();
    var available = _adjacentRooms.Where((room) => ShipManager.GetStatusFor(room.RoomName) == 0).ToList();
    if (available.Count > 0) {
      var target = available[rand.Next() % available.Count];
      if (rand.PercentChance(_chanceInfectAdjacent)) {
        ShipManager.UpdateStatusFor(target.RoomName, 1);
        // try to do another infection.
        Print.Debug($"[{_roomName}] infected {target.RoomName}");
        GetNewTimer(_timeInfectAdjacent).Timeout += DoInfection;
        return;
      }
      // try again. Provides some variance in rate of infection. Aslo creates another slider for difficulty
      Print.Debug($"[{_roomName}] failed to spread infection to {target.RoomName}");
      GetNewTimer(_timeInfectAdjacent).Timeout += DoInfection;
      return;
    }
    else {
      // move to next stage once all adjacent are infected.
      // prep for next stage
      Print.Debug($"[{_roomName}] all available rooms infected. cutting off O2 supply");
      GetNewTimer(_timeKillLifeSupport).Timeout += () => ShipManager.UpdateStatusFor(_roomName, 2);
    }
  }

  private SceneTreeTimer GetNewTimer(float time) {
    _timer = GetTree().CreateTimer(time, false);
    return _timer;
  }

}
