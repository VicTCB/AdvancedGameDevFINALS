using System.Numerics;
using Raylib_cs;

namespace CakeWars;

public class Projectile
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Damage;
    public float Radius;
    public Faction Faction;
    public bool IsAoe;
    public float AoeRadius;
    public bool AppliesStun;
    public float StunDuration;
    public bool AffectedByGravity;
    public Color VisualColor;
}