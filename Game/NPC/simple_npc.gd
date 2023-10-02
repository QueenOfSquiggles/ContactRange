@tool
extends Node3D

@export var dialog_track := ""
@export var sprite : Texture2D :
	set(value):
		$SimpleNPC.texture = value
	get:
		return $SimpleNPC.texture

func _ready() -> void:
	$TalkIcon.visible = false


func _on_interactive_trigger_on_interacted() -> void:
	print("interacting with NPC " + name + ", playing dialogic track: " + dialog_track)
	Dialogic.start(dialog_track)


func _on_interactive_trigger_on_deselected() -> void:
	$TalkIcon.visible = false


func _on_interactive_trigger_on_selected() -> void:
	$TalkIcon.visible = true
