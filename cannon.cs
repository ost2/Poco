using Godot;
using System;

public partial class cannon : RayCast2D
{
	// STATS
	float fireTime;
	float bulletSpeed;
	float damage;

	float heatUp;
	float coolDown;

	[Export]
	public PackedScene bulletScene;

	// CHILD NODES
	public AnimatedSprite2D crossSprite;
	AnimatedSprite2D cannonSprite;
	public Marker2D tip;
	PointLight2D muzzleFlash;
	AnimatedSprite2D muzzleFlashSprite;

	public Vector2 CrosshairPos { get { return crossSprite.GlobalPosition; } }

	// PARENT NODES
	public main main;
	Plane plane;

	// §READY
	public override void _Ready()
	{
		crossSprite = GetNode<AnimatedSprite2D>("CrosshairSprite");
		cannonSprite = GetNode<AnimatedSprite2D>("CannonSprite");
		tip = cannonSprite.GetNode<Marker2D>("CannonTip");
		muzzleFlashSprite = tip.GetNode<AnimatedSprite2D>("MuzzleFlashSprite");
		plane = GetParent<Plane>();
		main = plane.GetParent<main>();

		if (!plane.isPlayer)
		{
			assignStats();
		}

		muzzleFlashSprite.GetNode<Light2D>("MuzzleFlash").Scale /= muzzleFlashSprite.Scale;
	}
	public void assignStats()
	{
		fireTime = plane.fireTime;
		bulletSpeed = plane.bulletSpeed;
		damage = plane.damage;

		heatUp = plane.heatUp;
		coolDown = plane.coolDown;
	}

	float lastFired = 0.0f;
	public float realFireTime = 0.0f;
	// §PROCESS
	public override void _PhysicsProcess(double delta)
	{
		if (realFireTime == 0)
		{
			realFireTime = fireTime;
		}

		assignStats();
		crossSprite.Visible = plane.isPlayer;

		TargetPosition = plane.isPlayer ? main.MousePos : main.PlayerPos + main.player.frontVector * (main.player.curSpeed * plane.aimSkill);

		var scaleFactor =  0.5f + Mathf.InverseLerp(0, 1920, (TargetPosition - GlobalPosition).Length()) * 6;
		crossSprite.Scale = new Vector2(x: scaleFactor, y: scaleFactor);

		lastFired += (float)delta;

		if (lastFired < 0.05)
		{
			muzzleFlashSprite.Visible = true;
			muzzleFlashSprite.Play();
		}
		else muzzleFlashSprite.Visible = false;

		var angle = plane.frontVector;
		if (plane.canAim)
		{
			angle = (TargetPosition - GlobalPosition).Normalized();
		}

		cannonSprite.GlobalRotation = angle.Angle();

		var rand = new RandomNumberGenerator();
		var sway = (plane.inaccuracyRad / 2) - (realFireTime - fireTime);
		var curSway = rand.RandfRange(-sway, sway);
		angle = Vector2.FromAngle(angle.Angle() + curSway);

		var shouldShoot = false;
		if (plane.isPlayer)
		{
			crossSprite.Position = TargetPosition;
			crossSprite.Rotation += (float)delta;
			
			if (Input.IsActionPressed("shoot"))
			{
				if (realFireTime < fireTime * 2.0f)
				{
					realFireTime += heatUp;
				}
				shouldShoot = true;
				crossSprite.Animation = "in";
				crossSprite.SpeedScale = 1 / (realFireTime * 2);
				crossSprite.Play();
			}
			else if (Input.IsActionJustReleased("shoot"))
			{
				crossSprite.Animation = "out";
				crossSprite.Play();
			}
			else 
			{
				if (realFireTime > fireTime * 0.5f)
				{
					realFireTime -= coolDown * (float)delta;
				}
			}
			realFireTime = Mathf.Clamp(realFireTime, fireTime * 0.5f, fireTime * 2);
		}
		else 
		{
			realFireTime = fireTime;
		}
		if (!plane.isPlayer && (plane.Position - main.PlayerPos).Length() < plane.fireRange)
		{
			shouldShoot = true;
		}

		if (lastFired >= realFireTime && shouldShoot && main.gameRunning)
		{
			fireCannon();
		}

		void fireCannon()
		{
			lastFired = 0.0f;

			cannonSprite.GlobalRotation = angle.Angle();

			var bul = bulletScene.Instantiate<bullet>();
			bul.Position = tip.GlobalPosition;
			bul.Rotation = angle.Angle();
			bul.angle = angle;
			bul.speed = bulletSpeed;
			bul.velocity = plane.Velocity;
			bul.damage = damage;
			bul.volume = loudness;
			bul.pitch = pitch;

			bul.firedId = plane.isPlayer ? "Player" : "Enemy";

			AddChild(bul);
		}
	}

	[Export]
	float loudness = 0;
	[Export]
	float pitch = 1;
	
	public void setLoudness(float db)
	{
		loudness = db;
	}
}
