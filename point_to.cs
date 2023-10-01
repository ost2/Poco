using Godot;
using System;

public partial class point_to : RayCast2D
{
	Node2D parent;
	main main;

	public Vector2 startPos = Vector2.Zero;

	Sprite2D point1;
	Sprite2D point2;
	Sprite2D point3;

	AnimatedSprite2D preview;
	public AnimatedSprite2D previewSprite;

	public bool isActive = true;
	public bool showLine = false;
	public bool showPreview = true;
	public bool inversePointScale = false;

	public float maxRange = 20000;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		parent = GetParent<Node2D>();
		main = parent.GetParent<main>();
		
		point1 = GetNode<Sprite2D>("Point1");
		point2 = GetNode<Sprite2D>("Point2");
		point3 = GetNode<Sprite2D>("Point3");

		preview = GetNode<AnimatedSprite2D>("Preview");
		previewSprite = preview.GetNode<AnimatedSprite2D>("PreviewSprite");

		Scale /= parent.Scale;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if ((parent.Position - main.mainCam.Position).Length() < maxRange && isActive == true)
		{
			Show();
		}
		else 
		{
			Hide();
		}
		if (showLine)
		{
			point1.Show();
			point2.Show();
			point3.Show();
		}
		else
		{
			point1.Hide();
			point2.Hide();
			point3.Hide();
		}
		if (showPreview && !isOnScreen())
		{
			preview.Show();
		}
		else 
		{
			preview.Hide();
		}

		TargetPosition = startPos == Vector2.Zero ? (main.PlayerPos - parent.Position) / (Scale * parent.Scale) : -(main.player.cannon.tip.GlobalPosition - startPos) / Scale;

		point1.Position = TargetPosition * 0.3f;
		point2.Position = TargetPosition * 0.5f;
		point3.Position = TargetPosition * 0.7f;

		preview.Position = TargetPosition - (TargetPosition.Normalized() * 30);
		preview.Rotation = TargetPosition.Angle() - Mathf.Pi;
		previewSprite.GlobalRotation = 0;

		var prevScaleVal = Mathf.InverseLerp(0, 500, Mathf.Clamp(howFarOffScreen(), 0, 500)) * 0.35f;
		preview.Scale = new Vector2(x: prevScaleVal, y: prevScaleVal);

		var scaleVal = Mathf.Clamp(TargetPosition.Length() / 100, 0, 10);
		var scaleVec = new Vector2(x: scaleVal, y: scaleVal);
		
		if (inversePointScale)
		{
			point1.Scale = new Vector2(x: 0.0075f, y: 0.0075f) * scaleVec * 23;
			point2.Scale = new Vector2(x: 0.015f, y: 0.015f) * scaleVec * 23;
			point3.Scale = new Vector2(x: 0.02f, y: 0.02f) * scaleVec * 23;
		}
		else 
		{
			point1.Scale = new Vector2(x: 0.15f, y: 0.15f) / scaleVec;
			point2.Scale = new Vector2(x: 0.3f, y: 0.3f) / scaleVec;
			point3.Scale = new Vector2(x: 0.5f, y: 0.5f) / scaleVec;
		}

		GlobalRotation = 0;
	}
	public bool isOnScreen()
	{
		if (Mathf.Abs(parent.Position.X - main.mainCam.Position.X) > 1920 / 2 || Mathf.Abs(parent.Position.Y - main.mainCam.Position.Y) > 1080 / 2)
		{
			return false;
		}
		else 
		{
			return true;
		}
	}

	public float howFarOffScreen()
	{
		var x = Mathf.Abs(parent.Position.X - main.mainCam.Position.X) - (1920 / main.mainCam.Zoom.X) / 2;
		var y = Mathf.Abs(parent.Position.Y - main.mainCam.Position.Y) - (1080 / main.mainCam.Zoom.Y) / 2;

		return x + y;
	}
}
