using Godot;
using System;

public partial class Main : Node2D
{
	public override void _Ready(){
		Summator s = new Summator();
		s.Add(10);
		s.Add(10);
		s.Add(10);
		s.Add(10);
		GD.Print(s.GetTotal());
	}
}
