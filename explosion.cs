using Godot;
using System;

public partial class explosion : AnimatedSprite2D
{
	PointLight2D light;

	public Vector2 vel;
	public Vector2 pos;

	Area2D hitBox;
	CollisionShape2D circle;
	CpuParticles2D particles;

	public float pitch;

	public bool playSound = true;
	public float damage = 0;
	public Vector2 size = new Vector2(x: 1, y: 1);
	public float speed = 1;

	public float volume;
	
	bool animOver = false;
	bool soundOver = false;
	bool particlesStarted = false;

	float time;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var sound = GetNode<AudioStreamPlayer2D>("ExplosionSound");
		sound.VolumeDb = volume;
		sound.PitchScale = pitch;
		sound.Play();

		particles = GetNode<CpuParticles2D>("Particles");

		hitBox = GetNode<Area2D>("HitBox");
		circle = hitBox.GetNode<CollisionShape2D>("Circle");

		hitBox.Name = "Explosion";

		Scale = size;
		var rand = new Random();
		Rotation = rand.Next(0, 181);
		light = GetNode<PointLight2D>("PointLight2D");
		Position = pos;
		Play();

		SpeedScale = speed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (soundOver && animOver && time > 1.0f)
		{
			QueueFree();
		}

		vel *= 0.995f;
		time += (float)delta;

		particles.GlobalPosition = GlobalPosition;

		//RotationDegrees = Mathf.Sin(time * 200) * 360;

		if (!particlesStarted && time > 0.0f)
		{
			particlesStarted = true;
			particles.Emitting = true;
		}

		if (time < 0.2f)
		{

			var c = circle.Shape as CircleShape2D;
			c.Radius = Mathf.Clamp(time * 140 * speed * 8, 0, 110) * size.X;
		}
		else
		{
			hitBox.Monitorable = false;
		}


		if (size == new Vector2(x: 1, y: 1))
		{
			Scale += new Vector2(x: (float)delta, y: (float)delta) * 2.75f;
		}
		Position += vel * (float)delta;
		
		if (!animOver)
		{
			light.Energy = 0.5f + Mathf.Cos(time * 2) * 4; 
		}
		light.TextureScale += (float)delta * 5;

		if (size.X != 1)
		{
			light.Enabled = false;
		}
	}

	void _on_animation_finished()
	{
		SelfModulate = new Color(0, 0, 0, 0);
		light.Hide();
		animOver = true;
	}
	void _on_audio_stream_player_2d_finished()
	{
		soundOver = true;
	}
}
