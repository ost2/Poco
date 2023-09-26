using Godot;
using System;
using System.Collections.Generic;

public partial class Plane : CharacterBody2D
{
	// MOVEMENT
	public float baseSpeed = 600.0f;
	public float curSpeed;
	public float boostAccel = 100.0f;
	public float maxBoost = 400.0f;
	public float slowDownAccel = 100.0f;
	public float maxSlowDown = 400.0f;
	public float turnAccel = 1.0f;
	public float turnVal;
	public float turnDecay = 0.4f;
	public float maxTurn = 3.0f;
	public Vector2 frontVector;
	
	public float MinSpeed { get { return Mathf.Clamp(baseSpeed - maxSlowDown, 0, baseSpeed); } }
	public float MaxSpeed { get { return baseSpeed + maxBoost; } }
	public float TurnSpeedLimit { get { return maxTurn / (1 + Mathf.Clamp(Mathf.InverseLerp(MinSpeed, MaxSpeed, curSpeed), 0.01f, 1)); } }

	public bool isPlayer = false;

	// COMBAT
	public float health = 100.0f;
	public float maxHealth = 100.0f;
	public bool canAim = true;
	public float fireTime;
	public float bulletSpeed;
	public float damage;
	public float inaccuracyRad;
	public float heatUp;
	public float coolDown;

	public float fireRange = 900.0f;
	public float aimSkill = 0.4f;

	public bool isDead = false;

	[Export]
	public PackedScene hitParticlesScene;
	public List<hit_particles> hitParticlesArr = new List<hit_particles>();

	// §READY
	public override void _Ready()
	{
		
	}
	
	// §PROCESS
	public override void _PhysicsProcess(double delta)
	{
	}
	public int handleTiltAnim(AnimatedSprite2D mainSprite)
	{
		if (turnVal < -TurnSpeedLimit * 0.5f)
		{
			mainSprite.Animation = "lean";
			mainSprite.Frame = 1;
			mainSprite.FlipV = true;
			return -2;
		}
		else if (turnVal < -TurnSpeedLimit * 0.15f)
		{
			mainSprite.Animation = "lean";
			mainSprite.Frame = 0;
			mainSprite.FlipV = true;
			return -1;
		}
		else if (turnVal <= TurnSpeedLimit * 0.15f)
		{
			mainSprite.Animation = "default";
			mainSprite.Frame = 0;
			return 0;
		}
		else if (turnVal <= TurnSpeedLimit * 0.5f)
		{
			mainSprite.Animation = "lean";
			mainSprite.Frame = 0;
			mainSprite.FlipV = false;
			return 1;
		}
		else if (turnVal <= TurnSpeedLimit * 1.0f)
		{
			mainSprite.Animation = "lean";
			mainSprite.Frame = 1;
			mainSprite.FlipV = false;
			return 2;
		}
		return 0;
	}
	public void hideStuff(cannon cannon, AnimatedSprite2D propeller, AnimatedSprite2D propeller2 = null, AnimatedSprite2D propeller3 = null, Node2D node = null)
	{
		cannon?.Hide();
		propeller?.Hide();

		propeller2?.Hide();
		propeller3?.Hide();

		node?.Hide();
	}
	public hit_particles getHitParticles(Vector2 angle, PackedScene particlesScene)
	{
		var rnd = new RandomNumberGenerator();

		var hitParticles = particlesScene.Instantiate<hit_particles>();

		var x = rnd.RandfRange(0, 8 * Scale.X * 0.5f);
		var y = rnd.RandfRange(0, 8 * Scale.Y * 0.8f - x);

		hitParticles.Scale /= Scale * 0.8f;
		hitParticles.Position = new Vector2(x: x, y: y);
		hitParticles.Direction = Vector2.FromAngle(angle.Angle() - frontVector.Angle());

		hitParticlesArr.Add(hitParticles);
		return hitParticles;
	}

	// public void doHitParticles(hit_particles)
	// {
	// 	hitP
	// }

	public void spinPropeller(AnimatedSprite2D propellerSprite, AudioStreamPlayer2D engineSound = null)
	{
		var playSpeed = 1 + Mathf.Clamp(Mathf.InverseLerp(MinSpeed, MaxSpeed, curSpeed), 0.05f, 1);
		propellerSprite.SpeedScale = 1 + playSpeed;
		if (!propellerSprite.IsPlaying())
		{
			propellerSprite.Play();
		}

		if (engineSound != null)
		{
			engineSound.PitchScale = 0.75f + Mathf.Clamp(Mathf.InverseLerp(MinSpeed, MaxSpeed, curSpeed), 0.05f, 1) * 0.4f;
			engineSound.VolumeDb = -5 + Mathf.Clamp(Mathf.InverseLerp(MinSpeed, MaxSpeed, curSpeed), 0.05f, 1);
			if (!engineSound.Playing)
			{
				engineSound.Play();
			}
		}
	}
	public void showStuff(cannon cannon, AnimatedSprite2D propeller, AnimatedSprite2D propeller2 = null, AnimatedSprite2D propeller3 = null)
	{
		cannon?.Show();
		propeller?.Show();

		propeller2?.Show();
		propeller3?.Show();
	}
}
