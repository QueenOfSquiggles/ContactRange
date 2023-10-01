using Godot;
using System;

public partial class PlayerInventorySlotDisplay : Button
{

  [Signal] public delegate void OnRequestDrop(int slot, string item);

  [Export] private void _buttonsRoot;

  private int _slot = 0;
  private string _item = "";

  public void SetItem(string id)
  {
    _item = id;


  }




}
