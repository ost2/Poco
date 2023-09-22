using Godot;
using System;

public partial class rocket_target : Node2D
{
	public float radius = 45;
	public Plane target = null;

	AnimatedSprite2D left;
	AnimatedSprite2D up;
	AnimatedSprite2D right;
	AnimatedSprite2D down;

	AnimatedSprite2D rocketSprite;
	Label countLabel;

	float curTargetTime;
	float lostTargetTime;
	bool hasTarget = false;
	bool showRocketSprite = false;
	Vector2 rocketBaseScale;
	float rocketScaleStep = 20;

	public float rocketCount;

	float baseScale;

	float time;
	float freq = 2.5f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		baseScale = Scale.X;

		left = GetNode<AnimatedSprite2D>("Left");
		up = GetNode<AnimatedSprite2D>("Up");
		right = GetNode<AnimatedSprite2D>("Right");
		down = GetNode<AnimatedSprite2D>("Down");

		rocketSprite = GetNode<AnimatedSprite2D>("RocketSprite");
		countLabel = rocketSprite.GetNode<Label>("CountLabel");

		rocketBaseScale = rocketSprite.Scale;

		rocketSprite.Hide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		time += (float)delta * freq;

		rocketSprite.GlobalPosition = GlobalPosition + new Vector2(x: 70, y: -70);
		countLabel.Text = "X " + rocketCount;

		if (showRocketSprite)
		{
			rocketSprite.Show();
			if (rocketSprite.Scale < rocketBaseScale)
			{
				rocketSprite.Scale *= 1 + (float)delta * rocketScaleStep;
			}
			else
			{
				rocketSprite.Scale = rocketBaseScale;
			}
		}
		else
		{
			rocketSprite.Scale = new Vector2(x: 0.1f, y: 0.1f);
			// if (rocketSprite.Scale > Vector2.Zero)
			// {
			// 	rocketSprite.Scale *= 1 - (float)delta * rocketScaleStep;
			// }
			// else
			// {
			// 	rocketSprite.Scale = Vector2.Zero;
			// 	rocketSprite.Hide();
			// }
		}

		if (hasTarget)
		{
			if (curTargetTime > 0.01f)
			{
				Show();
				showRocketSprite = true;
			}
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

		if (IsInstanceValid(target) && rocketCount > 0)
		{
			if (target != null && !target.isDead)
			{
				GlobalPosition = target.GlobalPosition + target.frontVector * 15.5f;
			}
			else
			{
				showRocketSprite = false;
				Hide();
			}
		}
		else
		{
			Hide();
			target = null;
		}

		var rad = radius + (radius * Mathf.Sin(time) * 0.15f);
		setRadius(rad);

		Rotation += (float)delta * 2;
	}

	public void getNewTarget()
	{
		hasTarget = true;
		curTargetTime = 0;
		showRocketSprite = false;

		if (rocketCount > 0)
		{
			GetNode<AudioStreamPlayer>("TargetSound").Play();
		}
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
