using Godot;
using System;

public partial class Background : Sprite2D
{
	public Vector2 pos;
	public Vector2 cloudPos;
	public Vector2 cloudOPos;
	FastNoiseLite noise;

	Sprite2D clouds;
	FastNoiseLite cloudNoise;

	float speed = 0.01f;
	float cloudSpeed = 0.03f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		noise = (Texture as NoiseTexture2D).Noise as FastNoiseLite;
		clouds = GetNode<Sprite2D>("CloudsSprite");
		cloudNoise = (clouds.Texture as NoiseTexture2D).Noise as FastNoiseLite;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition = pos;
		noise.Offset = new Vector3(x: pos.X * speed, y: pos.Y * speed, z: 0);

		clouds.GlobalPosition = cloudPos;
		cloudNoise.Offset = new Vector3(x: cloudOPos.X * cloudSpeed, y: cloudOPos.Y * cloudSpeed, z: 0);
	}
}
