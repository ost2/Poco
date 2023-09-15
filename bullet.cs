using Godot;
using System;

public partial class bullet : Area2D
{
	public Vector2 angle;
	public Vector2 velocity;
	public float speed;
	public float damage;

	public string firedId;

	public float volume;
	public float pitch;

	Sprite2D trail;
	float trailLength;
	float trailAlpha = 0;

	float existTime = 0.0f;

	AudioStreamPlayer2D fireSound;

	const float bulletVolumeConstant = -10;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var main = GetParent<cannon>().main;

		fireSound = GetNode<AudioStreamPlayer2D>("BulletFiredSound");
		fireSound.VolumeDb = volume + bulletVolumeConstant;
		fireSound.PitchScale = pitch;

		trail = GetNode<Sprite2D>("TrailSprite");
		trail.SelfModulate = new Color(1, 1, 1, trailAlpha);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		var frontVector = Vector2.FromAngle(angle.Angle());

		velocity /= 1 + (float)delta;
		speed *= 1 + (float)delta;

		existTime += (float)delta;
		Position += ((angle * speed) + velocity) * (float)delta;
		
		Rotation = frontVector.Angle();

		if (trailAlpha < 1)
		{
			trailAlpha += (float)delta;
			trail.SelfModulate = new Color(1, 1, 1, trailAlpha);
		}

		if (existTime > 1.5f)
		{
			QueueFree();
		}
	}
}
