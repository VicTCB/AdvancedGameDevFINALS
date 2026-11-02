using System.Numerics;

namespace CakeWars;

public static class SpawnerSystem
{
    public static void SpawnUnit(GameState state, string name, Faction faction, UnitType type, float startX, float hp, float speed, float dmg, float range, float cooldown, int reward, float exp)
    {
        Vector2 size = type switch
        {
            UnitType.Mount => new Vector2(56, 62),
            UnitType.Flying => new Vector2(30, 30),
            UnitType.Ranged => new Vector2(32, 40),
            _ => new Vector2(36, 46)
        };

        float startY = (type == UnitType.Flying) ? GameConfig.AirY : (GameConfig.GroundY - size.Y);

        state.Units.Add(new Unit
        {
            Name = name,
            Faction = faction,
            Type = type,
            Position = new Vector2(startX, startY),
            Size = size,
            Hp = hp,
            MaxHp = hp,
            Speed = speed,
            BaseSpeed = speed,
            Damage = dmg,
            AttackRange = range,
            AttackCooldown = cooldown,
            AttackTimer = 0f,
            CrumbReward = reward,
            ExpReward = exp
        });
    }

    public static void UpdateOven(GameState state, float dt)
    {
        if (state.OvenQueue.Count == 0) return;

        var currentBake = state.OvenQueue.Peek();
        if (currentBake.BakeTimeRemaining > 0)
        {
            currentBake.BakeTimeRemaining -= dt;
        }
        else
        {
            bool isAir = currentBake.Type == UnitType.Flying;
            bool spawnBlocked = false;

            foreach (var u in state.Units)
            {
                if (u.Faction == Faction.Ally)
                {
                    bool friendIsAir = u.Type == UnitType.Flying;
                    if (isAir != friendIsAir) continue;

                    if (u.Position.X < GameConfig.AllySpawnX + 55f)
                    {
                        spawnBlocked = true;
                        break;
                    }
                }
            }

            if (!spawnBlocked)
            {
                state.OvenQueue.Dequeue();
                SpawnUnit(state, currentBake.Name, Faction.Ally, currentBake.Type, GameConfig.AllySpawnX, currentBake.Hp, currentBake.Speed, currentBake.Damage, currentBake.AttackRange, currentBake.AttackCooldown, 0, 0);
            }
        }
    }

    public static void SpawnEnemyWave(GameState state)
    {
        Random rnd = new Random();
        int categoryRoll = rnd.Next(0, 100);

        if (categoryRoll < 40)
        {
            if (state.CakeTier == 1 || rnd.Next(0, 2) == 0)
                SpawnUnit(state, "Small Red Ant", Faction.Enemy, UnitType.Ground, GameConfig.TableWidth - 140f, 80f, 80f, 12f, 35f, 0.9f, 15, 25f);
            else if (state.CakeTier == 2 || rnd.Next(0, 2) == 0)
                SpawnUnit(state, "Big Red Ant", Faction.Enemy, UnitType.Ground, GameConfig.TableWidth - 140f, 160f, 70f, 22f, 40f, 1.1f, 25, 40f);
            else
                SpawnUnit(state, "Beetle", Faction.Enemy, UnitType.Ground, GameConfig.TableWidth - 140f, 280f, 55f, 32f, 40f, 1.2f, 40, 60f);
        }
        else if (categoryRoll < 68)
        {
            if (state.CakeTier == 1)
                SpawnUnit(state, "Bombardier", Faction.Enemy, UnitType.Ranged, GameConfig.TableWidth - 140f, 65f, 60f, 16f, 220f, 1.5f, 25, 40f);
            else if (state.CakeTier == 2)
                SpawnUnit(state, "Spitter Spider", Faction.Enemy, UnitType.Ranged, GameConfig.TableWidth - 140f, 110f, 65f, 25f, 250f, 1.3f, 35, 55f);
            else
                SpawnUnit(state, "Wasp Shooter", Faction.Enemy, UnitType.Ranged, GameConfig.TableWidth - 140f, 170f, 75f, 38f, 290f, 1.1f, 50, 75f);
        }
        else if (categoryRoll < 88)
        {
            if (state.CakeTier == 1)
                SpawnUnit(state, "Fruit Fly", Faction.Enemy, UnitType.Flying, GameConfig.TableWidth - 140f, 70f, 95f, 14f, 140f, 1.0f, 30, 45f);
            else if (state.CakeTier == 2)
                SpawnUnit(state, "House Fly", Faction.Enemy, UnitType.Flying, GameConfig.TableWidth - 140f, 120f, 105f, 22f, 160f, 0.9f, 45, 60f);
            else
                SpawnUnit(state, "Flying Roach", Faction.Enemy, UnitType.Flying, GameConfig.TableWidth - 140f, 190f, 115f, 34f, 180f, 0.85f, 65, 85f);
        }
        else
        {
            if (state.CakeTier == 1)
                SpawnUnit(state, "Mantis", Faction.Enemy, UnitType.Mount, GameConfig.TableWidth - 140f, 350f, 45f, 38f, 55f, 1.6f, 55, 75f);
            else if (state.CakeTier == 2)
                SpawnUnit(state, "Mouse", Faction.Enemy, UnitType.Mount, GameConfig.TableWidth - 140f, 550f, 40f, 55f, 55f, 1.8f, 85, 110f);
            else
                SpawnUnit(state, "Kitchen Rat", Faction.Enemy, UnitType.Mount, GameConfig.TableWidth - 140f, 950f, 38f, 85f, 60f, 2.0f, 140, 160f);
        }
    }
}