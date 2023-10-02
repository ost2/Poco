using Godot;
using System;

public partial class rocket : CharacterBody2D
{
	AnimatedSprite2D sprite;

	public float startSpeed;
	public float speedMult;

	public Vector2 propVector;
	Vector2 frontVector;
	public float curSpeed;

	public Plane target;
	public float tracking = 7.5f;

	public float damage;
	public float explosionScale;

	public main main;

	float time = 1;
	float freq = 1;
	float maxTime = 4.0f;

	bool exploded = false;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sprite = GetNode<AnimatedSprite2D>("Sprite");

		GlobalRotation = propVector.Angle();

		curSpeed = startSpeed;

		sprite.Play();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		time += (float)delta * freq;

		if (time / freq > maxTime)
		{
			explode();
		}

		if (IsInstanceValid(target) && target != null)
		{
			var dirVector = (target.GlobalPosition - GlobalPosition).Normalized();
			GlobalRotation = Vector2.FromAngle(Rotation).MoveToward(dirVector, tracking * (float)delta).Angle();
		}
		frontVector = Vector2.FromAngle(GlobalRotation);

		curSpeed = (startSpeed + time * 10) * speedMult * time;
		Velocity = frontVector * curSpeed;

		MoveAndSlide();
	}

	void explode(CharacterBody2D plane = null)
	{
		if (plane == null)
		{
			main.spawnExplosion(this, explosionScale, -12, null, 1, 0, damage, 1);
		}
		else
		{
			main.spawnExplosion(plane, explosionScale, -12, null, 1, 0, damage, 1);
		}
		QueueFree();
	}

	void _on_area_2d_area_entered(Area2D area)
	{
		if (area.Name == "Enemy" && !exploded)
		{
			exploded = true;
			var enemy = area.GetParent<CharacterBody2D>();
			explode(enemy);
		}
	}
}
