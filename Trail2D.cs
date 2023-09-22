using Godot;
using System;

public partial class Trail2D : Line2D
{
	[Export]
	float length = 10;
	Vector2 point;
	public override void _PhysicsProcess(double delta)
	{
		var plane = GetParent<Node2D>().GetParent<Node2D>().GetParent<Plane>();

		length = plane.curSpeed / 40;

		GlobalPosition = Vector2.Zero;
		GlobalRotation = 0;

		

		point = GetParent<Node2D>().GlobalPosition / plane.Scale;

		var color = new Color(255, 255, 255, Mathf.Lerp(plane.MinSpeed, plane.MaxSpeed, plane.curSpeed) * 0.000005f);
		Modulate = color;
		SelfModulate = color;

		AddPoint(point);
		while (GetPointCount() > length)
		{
			RemovePoint(0);
		}
	}
}
