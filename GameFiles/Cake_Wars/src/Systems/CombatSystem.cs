using System.Numerics;
using Raylib_cs;

namespace CakeWars;

public static class CombatSystem
{
    public static void HandleKill(GameState state, Unit u)
    {
        if (u.Faction == Faction.Enemy)
        {
            state.Crumbs += u.CrumbReward;
            state.CakeExp += u.ExpReward;

            foreach (var quest in state.ActiveQuests)
            {
                if (!quest.Completed)
                {
                    quest.CurrentCount++;
                    if (quest.CurrentCount >= quest.TargetCount)
                    {
                        quest.Completed = true;
                        quest.ApplyBuff();
                    }
                }
            }
        }
    }

    public static void UpdateUnits(GameState state, float dt)
    {
        for (int i = state.Units.Count - 1; i >= 0; i--)
        {
            var u = state.Units[i];
            u.Speed = u.BaseSpeed;

            if (u.StunTimer > 0)
            {
                u.StunTimer -= dt;
                continue;
            }

            u.AttackTimer -= dt;
            bool isAirUnit = (u.Type == UnitType.Flying);

            if (u.Faction == Faction.Ally && u.Position.X + u.Size.X >= GameConfig.TableWidth - 140f)
            {
                state.WindowSealed += 3.5f * dt;
                continue;
            }

            if (u.Faction == Faction.Enemy && u.Position.X <= 180f)
            {
                if (u.AttackTimer <= 0)
                {
                    state.CakeHp -= u.Damage;
                    u.AttackTimer = u.AttackCooldown;
                }
                continue;
            }

            bool blockedByFriend = false;
            foreach (var friend in state.Units)
            {
                if (friend == u || friend.Faction != u.Faction) continue;

                bool friendIsAir = (friend.Type == UnitType.Flying);
                if (isAirUnit != friendIsAir) continue;

                float gap = (u.Faction == Faction.Ally)
                    ? (friend.Position.X - (u.Position.X + u.Size.X))
                    : (u.Position.X - (friend.Position.X + friend.Size.X));

                if (gap >= -15f && gap < 8f)
                {
                    if ((u.Faction == Faction.Ally && friend.Position.X > u.Position.X) ||
                        (u.Faction == Faction.Enemy && friend.Position.X < u.Position.X))
                    {
                        blockedByFriend = true;
                        break;
                    }
                }
            }

            Unit? target = null;
            float closestEdgeDist = u.AttackRange;

            foreach (var other in state.Units)
            {
                if (other.Faction == u.Faction) continue;

                if (u.Type != UnitType.Flying && u.Type != UnitType.Ranged && other.Type == UnitType.Flying)
                    continue;

                float edgeDist = (u.Faction == Faction.Ally)
                    ? (other.Position.X - (u.Position.X + u.Size.X))
                    : (u.Position.X - (other.Position.X + other.Size.X));

                if (edgeDist >= -12f && edgeDist <= closestEdgeDist)
                {
                    closestEdgeDist = edgeDist;
                    target = other;
                }
            }

            if (target != null)
            {
                if (u.AttackTimer <= 0)
                {
                    if (u.Type == UnitType.Ranged || u.Type == UnitType.Flying)
                    {
                        float spawnX = (u.Faction == Faction.Ally) ? (u.Position.X + u.Size.X) : u.Position.X;
                        Vector2 spawnPos = new Vector2(spawnX, u.Position.Y + u.Size.Y / 2);
                        Vector2 targetCenter = target.Position + target.Size / 2;
                        Vector2 dir = Vector2.Normalize(targetCenter - spawnPos);

                        state.Projectiles.Add(new Projectile
                        {
                            Position = spawnPos,
                            Velocity = dir * 420f,
                            Damage = u.Damage,
                            Radius = 5f,
                            Faction = u.Faction,
                            IsAoe = false,
                            VisualColor = u.Faction == Faction.Ally ? Color.Brown : Color.Purple
                        });
                    }
                    else
                    {
                        target.Hp -= u.Damage;
                    }

                    u.AttackTimer = u.AttackCooldown;
                }
            }
            else if (!blockedByFriend)
            {
                float dir = (u.Faction == Faction.Ally) ? 1f : -1f;
                u.Position.X += dir * u.Speed * dt;
            }

            if (u.Hp <= 0)
            {
                HandleKill(state, u);
                state.Units.RemoveAt(i);
            }
        }
    }

    public static void UpdateTurrets(GameState state, float dt)
    {
        for (int i = 0; i < 3; i++)
        {
            var t = state.CakeTurrets[i];
            if (!t.Unlocked || !t.HasTurret) continue;

            Vector2 turretPos = new Vector2(20, GameConfig.GroundY) + t.RelativeOffset;

            if (t.IsFlamethrower)
            {
                t.CooldownTimer -= dt;
                if (t.CooldownTimer <= 0)
                {
                    for (int uIdx = state.Units.Count - 1; uIdx >= 0; uIdx--)
                    {
                        var u = state.Units[uIdx];
                        if (u.Faction == Faction.Enemy)
                        {
                            float dist = u.Position.X - turretPos.X;
                            if (dist >= 0 && dist <= t.Range)
                            {
                                u.Hp -= t.Damage * state.AllyDamageMultiplier;
                                if (u.Hp <= 0)
                                {
                                    HandleKill(state, u);
                                    state.Units.RemoveAt(uIdx);
                                }
                            }
                        }
                    }
                    t.CooldownTimer = t.Cooldown;
                }
                continue;
            }

            t.CooldownTimer -= dt;
            if (t.CooldownTimer <= 0)
            {
                Unit? target = null;
                float closestDist = float.MaxValue;

                foreach (var u in state.Units)
                {
                    if (u.Faction == Faction.Enemy)
                    {
                        float dist = u.Position.X - turretPos.X;
                        if (dist >= 0 && dist <= t.Range && dist < closestDist)
                        {
                            closestDist = dist;
                            target = u;
                        }
                    }
                }

                if (target != null)
                {
                    Vector2 spawnPos = turretPos + new Vector2(10, 10);
                    Vector2 targetCenter = target.Position + target.Size / 2;

                    if (t.Type == TurretType.PoppingCandy || t.Type == TurretType.Marshmallow || t.Type == TurretType.WhippedCream)
                    {
                        float dx = targetCenter.X - spawnPos.X;
                        float flightTime = Math.Clamp(dx / 320f, 0.6f, 1.4f);
                        float vx = dx / flightTime;
                        float gravity = 550f;
                        float vy = ((targetCenter.Y - spawnPos.Y) - (0.5f * gravity * flightTime * flightTime)) / flightTime;

                        float aoeRadius = t.Type == TurretType.PoppingCandy ? 60f : (t.Type == TurretType.Marshmallow ? 85f : 110f);
                        Color bulletColor = t.Type == TurretType.WhippedCream ? Color.RayWhite : (t.Type == TurretType.Marshmallow ? Color.Pink : Color.Orange);

                        state.Projectiles.Add(new Projectile
                        {
                            Position = spawnPos,
                            Velocity = new Vector2(vx, vy),
                            Damage = t.Damage * state.AllyDamageMultiplier,
                            Radius = 7f,
                            Faction = Faction.Ally,
                            IsAoe = true,
                            AoeRadius = aoeRadius,
                            AppliesStun = (t.Type == TurretType.WhippedCream),
                            StunDuration = 1.2f,
                            AffectedByGravity = true,
                            VisualColor = bulletColor
                        });
                    }
                    else
                    {
                        Vector2 diff = targetCenter - spawnPos;
                        Vector2 dir = diff.LengthSquared() > 0.001f ? Vector2.Normalize(diff) : new Vector2(1, 0);

                        float projSpeed = t.Type == TurretType.CherryCannon ? 620f : 500f;
                        float projRadius = t.Type == TurretType.CherryCannon ? 9f : 6f;
                        Color bulletColor = t.Type == TurretType.CherryCannon ? Color.Maroon : (t.Type == TurretType.ChocolateChip ? Color.DarkBrown : Color.Magenta);

                        state.Projectiles.Add(new Projectile
                        {
                            Position = spawnPos,
                            Velocity = dir * projSpeed,
                            Damage = t.Damage * state.AllyDamageMultiplier,
                            Radius = projRadius,
                            Faction = Faction.Ally,
                            IsAoe = false,
                            AffectedByGravity = false,
                            VisualColor = bulletColor
                        });
                    }

                    t.CooldownTimer = t.Cooldown;
                }
            }
        }
    }

    public static void UpdateProjectiles(GameState state, float dt)
    {
        for (int i = state.Projectiles.Count - 1; i >= 0; i--)
        {
            var p = state.Projectiles[i];

            if (p.AffectedByGravity)
            {
                p.Velocity.Y += 550f * dt;
            }

            Vector2 nextPos = p.Position + p.Velocity * dt;
            bool explode = false;

            if (p.AffectedByGravity && nextPos.Y >= GameConfig.GroundY - 10f)
            {
                explode = true;
                nextPos.Y = GameConfig.GroundY - 10f;
            }

            for (int uIdx = state.Units.Count - 1; uIdx >= 0; uIdx--)
            {
                var u = state.Units[uIdx];
                if (u.Faction != p.Faction)
                {
                    Rectangle rect = new Rectangle(u.Position.X, u.Position.Y, u.Size.X, u.Size.Y);

                    if (Raylib.CheckCollisionCircleRec(nextPos, p.Radius, rect) ||
                        Raylib.CheckCollisionPointRec(p.Position, rect))
                    {
                        explode = true;
                        if (!p.IsAoe)
                        {
                            u.Hp -= p.Damage;
                            if (u.Hp <= 0)
                            {
                                HandleKill(state, u);
                                state.Units.RemoveAt(uIdx);
                            }
                        }
                        break;
                    }
                }
            }

            if (explode && p.IsAoe)
            {
                for (int uIdx = state.Units.Count - 1; uIdx >= 0; uIdx--)
                {
                    var u = state.Units[uIdx];
                    if (u.Faction != p.Faction)
                    {
                        Vector2 uCenter = u.Position + u.Size / 2;
                        if (Vector2.Distance(nextPos, uCenter) <= p.AoeRadius)
                        {
                            u.Hp -= p.Damage;
                            if (p.AppliesStun)
                            {
                                u.StunTimer = p.StunDuration;
                            }

                            if (u.Hp <= 0)
                            {
                                HandleKill(state, u);
                                state.Units.RemoveAt(uIdx);
                            }
                        }
                    }
                }
            }

            p.Position = nextPos;

            if (explode || p.Position.X < 0 || p.Position.X > GameConfig.TableWidth || p.Position.Y > GameConfig.GroundY + 30f)
            {
                state.Projectiles.RemoveAt(i);
            }
        }
    }
}