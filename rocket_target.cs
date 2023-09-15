using Godot;
using System;

public partial class rocket_target : Node2D
{
	public float radius = 40;
	public Plane target = null;

	AnimatedSprite2D left;
	AnimatedSprite2D up;
	AnimatedSprite2D right;
	AnimatedSprite2D down;

	float curTargetTime;
	float lostTargetTime;
	bool hasTarget = false;

	float baseScale;

	float time;
	float freq = 4;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		baseScale = Scale.X;

		left = GetNode<AnimatedSprite2D>("Left");
		up = GetNode<AnimatedSprite2D>("Up");
		right = GetNode<AnimatedSprite2D>("Right");
		down = GetNode<AnimatedSprite2D>("Down");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		time += (float)delta * freq;

		if (hasTarget)
		{
			if (curTargetTime <= 0.5f)
			{
				curTargetTime += (float)delta;
				var scaleFac = baseScale * 0.4f + Mathf.Clamp(curTargetTime * 3 * baseScale, 0, baseScale * 0.6f);
				Scale = new Vector2(x: scaleFac, y: scaleFac);
			}
			Rotation += (float)delta * (4 * Mathf.Clamp(1 - Mathf.InverseLerp(0, 1.5f, curTargetTime), 0, 1));
		}
		else
		{
			if (lostTargetTime <= 0.2f)
			{
				lostTargetTime += (float)delta;
				Rotation += (float)delta * (lostTargetTime * 10 + 1);
				var scaleFac = baseScale - baseScale * Mathf.InverseLerp(0, 0.2f, lostTargetTime);
				Scale = new Vector2(x: scaleFac, y: scaleFac);
			}
		}

		if (IsInstanceValid(target))
		{
			if (target != null && !target.isDead)
			{
				Show();
				GlobalPosition = target.GlobalPosition;
			}
			else
			{
				Hide();
			}
		}
		else
		{
			target = null;
		}

		var rad = radius + (radius * Mathf.Sin(time) * 0.3f);
		setRadius(rad);

		Rotation += (float)delta * 2;
	}

	public void getNewTarget()
	{
		hasTarget = true;
		curTargetTime = 0;
	}
	public void loseTarget()
	{
		hasTarget = false;
		lostTargetTime = 0;
	}

	void setRadius(float r)
	{
		left.Position = new Vector2(x: -r, y: 0);
		up.Position = new Vector2(x: 0, y: -r);
		right.Position = new Vector2(x: r, y: 0);
		down.Position = new Vector2(x: 0, y: r);
	}
}
