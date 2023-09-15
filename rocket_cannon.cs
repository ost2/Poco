using System;
using Godot;

public partial class rocket_cannon : RayCast2D
{
    public float startSpeed;
    public float speedMult;

    public float damage;
    public float explosionScale;

    [Export]
    public PackedScene rocketScene;

    public main main;

    public override void _Ready()
    {
        
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