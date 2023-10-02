using Godot;
using System;

public partial class value_changed_label : Label
{
	float time;
	public float timeLimit;
	public float maxSize;

	public bool left;

	fancy_bar bar;
	public Node2D followNode = null;
	Vector2 startPos;

	float freq = 28;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Hide();
		
		bar = GetParent<fancy_bar>();

		startPos = Position;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		time += (float)delta;

		RotationDegrees = -90 + Mathf.Cos(time * 5) * 10;

		var scaleVal = Mathf.Sin(time * 3) * 0.6f;
		Scale = new Vector2(x: scaleVal, y: scaleVal);

		SelfModulate = bar.TintProgress;

		var posVal = Mathf.Cos(time * freq / 3);

		freq -= (float)delta * 28;

		var sign = left ? -1 : 1;
		if (followNode != null)
		{
			var relPos = followNode.GetGlobalTransformWithCanvas().Origin + new Vector2(x: 100, y: 0);
			GlobalPosition = relPos + new Vector2(x: 40 * sign, y: -300) + new Vector2(x: posVal, y: posVal) * freq * 0.3f;

			Scale *= 0.5f;
		}
		else
		{
			Position = startPos + new Vector2(x: posVal, y: posVal) * freq * 0.05f;
		}
		
		if (time >= 1)
		{
			QueueFree();
		}
		Show();
	}
}
