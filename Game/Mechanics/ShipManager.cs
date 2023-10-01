using System;
using System.Collections.Generic;
using Godot;
using Squiggles.Core.Data;
using Squiggles.Core.Error;
using Squiggles.Core.Events;


public partial class ShipManager : Node {

  private ShipManager _instance;

  public static event Action<string, int> OnRoomStatusUpdated;
  public static event Action<string, bool> OnFlagUpdated;

  public static string LastDeathMessage = "";

  private static readonly Dictionary<string, int> _statusRooms = new() {
    {"captain_quarters", 0},
    {"biomass_room", 0},
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
    {"coffee_requested", false},
    {"coffee_returned", false},
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
    OnRoomStatusUpdated?.Invoke(key, value);
  }

  public static void UpdateFlag(string key, bool value) {
    _flags[key] = value;
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
