using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 200f;
	[Export] public float JumpForce = 350f;
	[Export] public float Gravity = 900f;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// apply gravity if not on floor
		if (!IsOnFloor())
			velocity.Y += Gravity * (float)delta;
		else
			velocity.Y = 0; // reset vertical velocity when grounded

		// horizontal movement with a/d or left/right
		float direction = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
		velocity.X = direction * Speed;

		// jump with space or up key
		if (IsOnFloor() && (Input.IsActionJustPressed("ui_accept") || Input.IsActionJustPressed("ui_up")))
		{
			velocity.Y = -JumpForce;
		}

		// prevent manual downward movement
		if (velocity.Y > 0 && Input.IsActionPressed("ui_down"))
		{
			velocity.Y = 0;
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	// call when player dies
	public void Die()
	{
		GD.Print("player died!");

		// reset hud timer if exists
		if (GetTree().CurrentScene.HasNode("HUD"))
		{
			var hud = GetTree().CurrentScene.GetNode<HUD>("HUD");
			hud?.ResetTimer();
		}

		// go back to start/end screen
		GetTree().ChangeSceneToFile("res://start_end_screen.tscn");
	}
}
