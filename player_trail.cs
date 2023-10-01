using Godot;
using System;

public partial class player_trail : Line2D
{
	[Export]
	float scaleThing = 1;

	[Export]
	public float length = 10;

	float time = 0;

	bool draw;

	public Vector2 pos = Vector2.Zero;
	public override void _PhysicsProcess(double delta)
	{
		time += (float)delta;

		var mark = GetParent<Node2D>();
		var plane = mark.GetParent<Node2D>().GetParent<Plane>();

		var fpiBalls = 0.99996f - (0.0000001f * time);

		length = plane.curSpeed / 40;

		// GlobalPosition = ; //plane.GlobalPosition; //GetParent<Node2D>().GlobalPosition;
		// GlobalRotation = 0;

		var point = mark.GlobalPosition * fpiBalls / mark.GlobalScale; //GetParent<Node2D>().GlobalPosition / plane.Scale / scaleThing;

		var color = new Color(1, 1, 1, Mathf.InverseLerp(plane.MinSpeed, plane.MaxSlides, plane.curSpeed));
		// Modulate = color;
		// SelfModulate = color;

		if (draw)
		{
			AddPoint(point);
		}

		while (GetPointCount() > length)
		{
			RemovePoint(0);
		}
	}

	public void stopDrawing()
	{
		draw = false;
		ClearPoints();
	}
	public void startDrawing()
	{
		draw = true;
	}
}
