using Godot;
using System;

public partial class Hazard : Area2D
{
	public override void _Ready()
	{
		// Connect body_entered signal
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			// Reload start/end screen
			GetTree().ChangeSceneToFile("res://start_end_screen.tscn");
		}
	}
}
