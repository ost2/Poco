using Godot;
using System;

public partial class Trail2D : Line2D
{
	[Export]
	float scaleThing = 1;

	[Export]
	float length = 10;
	public override void _PhysicsProcess(double delta)
	{
		var mark = GetParent<Node2D>();
		var plane = mark.GetParent<Node2D>().GetParent<Plane>();

		length = plane.curSpeed / 40;

		GlobalPosition = Vector2.Zero; //plane.GlobalPosition; //GetParent<Node2D>().GlobalPosition;
		GlobalRotation = 0;

		var point = mark.GlobalPosition / mark.GlobalScale; //GetParent<Node2D>().GlobalPosition / plane.Scale / scaleThing;

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
