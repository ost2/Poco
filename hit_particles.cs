using Godot;
using System;

public partial class hit_particles : CpuParticles2D
{
	CharacterBody2D parent;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		parent = GetParent<CharacterBody2D>();
		Emitting = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
