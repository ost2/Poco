using Godot;
using System;

public partial class menu_button : Button
{
	AnimatedSprite2D sprite;
	Label text;

	AudioStreamPlayer clickSound;
	AudioStreamPlayer hoverSound;

	bool playShine = false;

	Color baseColor;
	bool colorSet = false;

	public string buttonID;

	menu menu;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sprite = GetNode<AnimatedSprite2D>("Sprite");
		text = GetNode<Label>("Text");

		menu = GetParent<menu>();

		hoverSound = GetNode<AudioStreamPlayer>("HoverSound");

		baseColor = Modulate;
		colorSet = true;

		sprite.Animation = "default";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		text.Text = Text;
	}

	public void setButtonID(string id, bool back = false)
	{
		buttonID = id;
		
		if (back)
		{
			clickSound = GetNode<AudioStreamPlayer>("BackSound");
		}
		else
		{
			clickSound = GetNode<AudioStreamPlayer>("ClickSound");
		}
	}

	void _on_pressed()
	{
		sprite.Animation = "pressed";
		sprite.Play();

		clickSound.Play();
	}

	void _on_mouse_entered()
	{
		playShine = true;
		sprite.Animation = "shine";
		sprite.Play();

		Modulate = new Color(baseColor.R * 1.4f, baseColor.G * 1.4f, baseColor.B * 1.4f, 1);

		hoverSound.Play();
	}
	void _on_mouse_exited()
	{
		playShine = false;

		Modulate = baseColor;
	}

	void _on_sprite_animation_looped()
	{
		if (!playShine)
		{
			sprite.Animation = "default";
		}
		if (sprite.Animation == "pressed")
		{
			menu.doButtonAction(buttonID);
			sprite.Animation = "default";
		}
	}

	void _on_visibility_changed()
	{
		if (colorSet)
		{
			Modulate = baseColor;
		}
		playShine = false;
	}
}
