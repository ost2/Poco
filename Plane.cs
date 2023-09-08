using Godot;
using System;

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
	
	public float MinSpeed { get { return baseSpeed - maxSlowDown; } }
	public float MaxSpeed { get { return baseSpeed + maxBoost; } }

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

	// §READY
	public override void _Ready()
	{
		
	}
	
	// §PROCESS
	public override void _PhysicsProcess(double delta)
	{
	}
	public void handleTiltAnim(AnimatedSprite2D mainSprite)
	{
		if (turnVal < -maxTurn * 0.5f)
		{
			mainSprite.Animation = "lean";
			mainSprite.Frame = 1;
			mainSprite.FlipV = true;
			Skew = turnVal * 0.1f;
		}
		else if (turnVal < -maxTurn * 0.15f)
		{
			mainSprite.Animation = "lean";
			mainSprite.Frame = 0;
			mainSprite.FlipV = true;
			Skew = turnVal * 0.2f;
		}
		else if (turnVal < maxTurn * 0.15f)
		{
			mainSprite.Animation = "default";
			mainSprite.Frame = 0;
		}
		else if (turnVal < maxTurn * 0.5f)
		{
			mainSprite.Animation = "lean";
			mainSprite.Frame = 0;
			mainSprite.FlipV = false;
			Skew = turnVal * 0.2f;
		}
		else if (turnVal < maxTurn * 1.0f)
		{
			mainSprite.Animation = "lean";
			mainSprite.Frame = 1;
			mainSprite.FlipV = false;
			Skew = turnVal * 0.1f;
		}
		Skew = Mathf.Abs(Skew) * 0;
	}
	public void hideStuff(cannon cannon, AnimatedSprite2D propeller, AnimatedSprite2D propeller2 = null, AnimatedSprite2D propeller3 = null, Node2D node = null)
	{
		cannon?.Hide();
		propeller?.Hide();

		propeller2?.Hide();
		propeller3?.Hide();

		node?.Hide();
	}
	public void spinPropeller(AnimatedSprite2D propellerSprite, AudioStreamPlayer2D engineSound = null)
	{
		var playSpeed = 1 + Mathf.InverseLerp(MinSpeed, MaxSpeed, curSpeed);
		propellerSprite.SpeedScale = 1 + playSpeed;
		if (!propellerSprite.IsPlaying())
		{
			propellerSprite.Play();
		}

		if (engineSound != null)
		{
			engineSound.PitchScale = 0.75f + Mathf.InverseLerp(MinSpeed, MaxSpeed, curSpeed) * 0.4f;
			engineSound.VolumeDb = -5 + Mathf.InverseLerp(MinSpeed, MaxSpeed, curSpeed);
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
