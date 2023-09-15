using Godot;
using System;

public partial class rocket_trail : Line2D
{
	[Export]
	float length = 10;
	Vector2 point;
	public override void _PhysicsProcess(double delta)
	{
		var rocket = GetParent<Node2D>().GetParent<rocket>();

		length = rocket.curSpeed / 80;

		GlobalPosition = Vector2.Zero;
		GlobalRotation = 0;

		point = GetParent<Node2D>().GlobalPosition / rocket.Scale;

		AddPoint(point);
		while (GetPointCount() > length)
		{
			RemovePoint(0);
		}
		//Skew = GetParent<plane>().Skew;
	}
}