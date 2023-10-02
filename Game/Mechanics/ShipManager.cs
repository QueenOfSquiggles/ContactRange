using System;
using System.Collections.Generic;
using Godot;
using Squiggles.Core.Data;
using Squiggles.Core.Error;
using Squiggles.Core.Events;
using Squiggles.Core.Scenes.Utility;


public partial class ShipManager : Node {

  [Export] private PackedScene _gameWinCutscene;
  private static ShipManager _instance;

  public static event Action<string, int> OnRoomStatusUpdated;
  public static event Action<string, bool> OnFlagUpdated;

  public static string LastDeathMessage = "";

  private static readonly Dictionary<string, int> _statusRooms = new() {
    {"captain_quarters", 0},
    {"main_storage", 0},
    {"west_crew_quarters", 0},
    {"west_thruster", 0},
    {"engineering_workshop", 0},
    {"medical_bay", 0},
    {"east_thruster", 0},
    {"cafeteria", 0},
    {"east_crew_quarters", 1},
    {"sensor_array", 0},
  };

  private static readonly Dictionary<string, bool> _flags = new() {
    {"sensor_array_active", false},
    {"west_thruster_fire_extinguished", false},
    {"east_thruster_fire_extinguished", false},
    {"west_thruster_refueled", false},
    {"east_thruster_refueled", false},
    {"ship_systems_restored", false},
    {"aliens_exterminated", false},
  };

  public static int GetStatusFor(string key) => _statusRooms[key];
  public static bool GetFlagFor(string key) => _flags[key];
  public static void UpdateStatusFor(string key, int value) {
    _statusRooms[key] = value;
    Print.Debug($"[ShipManager] room status [{key}] = {value}");
    OnRoomStatusUpdated?.Invoke(key, value);
    var allInfected = true;
    var allCleared = true;
    foreach (var roomStatus in _statusRooms.Values) {
      if (roomStatus != 2) {
        allInfected = false;
      }
      if (roomStatus != 0) {
        allCleared = false;
      }
    }
    if (allCleared) {
      // TODO: do win condition
      Print.Error("Should do win screen/cutscene here!!!!!", _instance);
      var node = _instance._gameWinCutscene.Instantiate();
      _instance.AddChild(node);
    }
    else
    if (allInfected) {
      // all rooms infected
      LastDeathMessage = "Aliens overtook the ship! Make sure to push them back when you can!!!";
      SceneTransitions.LoadSceneAsync("res://Game/Menu/game_over_screen.tscn");
    }
  }

  public static void UpdateFlag(string key, bool value) {
    _flags[key] = value;
    Print.Debug($"[ShipManager] flag status [{key}] = {value}");
    if ((key != "ship_systems_restored") && _flags["sensor_array_active"] && _flags["west_thruster_fire_extinguished"] && _flags["east_thruster_fire_extinguished"] && _flags["west_thruster_refueled"] && _flags["east_thruster_refueled"]) {
      UpdateFlag("ship_systems_restored", true);
    }
    OnFlagUpdated?.Invoke(key, value);
  }



  public override void _Ready() {
    if (_instance is not null) {
      QueueFree();
      return;
    }
    _instance = this;
    LoadData();
    EventBus.Data.SerializeAll += SaveData;
    EventBus.Data.Reload += LoadData;
    foreach (var entry in _statusRooms) {
      Print.Debug($"{entry.Key} = {entry.Value}");
    }
    foreach (var entry in _flags) {
      Print.Debug($"{entry.Key} = {entry.Value}");
    }
  }

  public static void SaveData() {
    var sb = GetFile();
    foreach (var entry in _statusRooms) {
      sb.PutInt(entry.Key, entry.Value);
    }
    foreach (var entry in _flags) {
      sb.PutBool(entry.Key, entry.Value);
    }
    sb.SaveToFile();
  }

  public static void LoadData() {
    var sb = GetFile().LoadFromFile();
    foreach (var key in _statusRooms.Keys) {
      if (sb.GetInt(key, out var value)) {
        _statusRooms[key] = value;
      }
    }
    foreach (var key in _flags.Keys) {
      if (sb.GetBool(key, out var value)) {
        _flags[key] = value;
      }
    }
  }

  private static SaveDataBuilder GetFile() => new("ship_manifest.json");
}
