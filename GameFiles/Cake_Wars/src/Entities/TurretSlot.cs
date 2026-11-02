using System.Numerics;

namespace CakeWars;

public class TurretSlot
{
    public Vector2 RelativeOffset;
    public bool Unlocked;
    public bool HasTurret;
    public TurretType Type;
    public string Name = "";
    public float Range;
    public float Damage;
    public float Cooldown;
    public float CooldownTimer;
    public bool IsFlamethrower;
}