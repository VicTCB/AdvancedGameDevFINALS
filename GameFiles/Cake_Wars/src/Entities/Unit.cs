using System.Numerics;

namespace CakeWars;

public class Unit
{
    public string Name = "";
    public Faction Faction;
    public UnitType Type;
    public Vector2 Position;
    public Vector2 Size;
    public float Speed;
    public float BaseSpeed;
    public float Hp;
    public float MaxHp;
    public float Damage;
    public float AttackRange;
    public float AttackCooldown;
    public float AttackTimer;
    public int CrumbReward;
    public float ExpReward;
    public float StunTimer;
}

public class BakingOrder
{
    public string Name = "";
    public UnitType Type;
    public float Hp;
    public float Speed;
    public float Damage;
    public float AttackRange;
    public float AttackCooldown;
    public float BakeTimeTotal;
    public float BakeTimeRemaining;
}