using System;
using Godot;

public partial class rocket_cannon : RayCast2D
{
    public float startSpeed;
    public float speedMult;

    public float damage;
    public float explosionScale;

    public bool loaded = false;

    AnimatedSprite2D sprite;
    public float spriteScale;
    public float baseScale;

    [Export]
    public PackedScene rocketScene;

    public main main;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite2D>("Sprite");
        spriteScale = sprite.Scale.X;
        baseScale = spriteScale;
    }

    public override void _Process(double delta)
    {
        sprite.Scale = new Vector2(x: spriteScale, y: spriteScale);
        if (loaded)
        {
            sprite.Show();
        }
        else
        {
            sprite.Hide();
        }
    }
    public void shootRocket(Plane target = null)
    {
        var rock = rocketScene.Instantiate<rocket>();

        rock.startSpeed = startSpeed;
        rock.speedMult = speedMult;

        rock.GlobalPosition = GlobalPosition;

        rock.propVector = TargetPosition.Normalized();
        rock.target = target;

        rock.damage = damage;
        rock.explosionScale = explosionScale;

        rock.main = main;

        AddChild(rock);
    }
}