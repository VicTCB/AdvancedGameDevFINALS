using System.Numerics;
using Raylib_cs;

namespace CakeWars;

class Program
{
    static GameState state = new();
    static Camera2D camera;

    static void Main()
    {
        Raylib.InitWindow(GameConfig.ScreenWidth, GameConfig.ScreenHeight, "Cake Wars - Clean Architecture");
        Raylib.SetTargetFPS(60);

        camera = new Camera2D
        {
            Target = new Vector2(0, 0),
            Offset = new Vector2(0, 0),
            Rotation = 0.0f,
            Zoom = 1.0f
        };

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();

            if (state.CurrentPhase == GamePhase.Playing)
            {
                UpdateGame(dt);
            }
            else if (state.CurrentPhase == GamePhase.FlavorSelection)
            {
                UpdateFlavorSelection();
            }

            // ----------------- DRAW WORLD -----------------
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(245, 238, 220, 255));

            Raylib.BeginMode2D(camera);

            Raylib.DrawRectangle(0, (int)GameConfig.GroundY, (int)GameConfig.TableWidth, GameConfig.ScreenHeight - (int)GameConfig.GroundY, new Color(185, 135, 95, 255));

            UIRenderer.DrawCake(state);

            // Turret visual flames
            for (int i = 0; i < 3; i++)
            {
                var t = state.CakeTurrets[i];
                if (t.Unlocked && t.HasTurret && t.IsFlamethrower)
                {
                    Vector2 turretPos = new Vector2(20, GameConfig.GroundY) + t.RelativeOffset;
                    Color flameColor = t.Type == TurretType.CremeBruleeTorch ? Color.SkyBlue : Color.Orange;
                    Raylib.DrawTriangle(
                        turretPos + new Vector2(15, 10),
                        turretPos + new Vector2(t.Range, -15),
                        turretPos + new Vector2(t.Range, 35),
                        new Color((int)flameColor.R, (int)flameColor.G, (int)flameColor.B, 110)
                    );
                }
            }

            // Window Base
            Raylib.DrawRectangle((int)GameConfig.TableWidth - 120, (int)GameConfig.GroundY - 260, 120, 260, Color.SkyBlue);
            Raylib.DrawRectangleLines((int)GameConfig.TableWidth - 120, (int)GameConfig.GroundY - 260, 120, 260, Color.DarkBlue);
            Raylib.DrawText("WINDOW", (int)GameConfig.TableWidth - 105, (int)GameConfig.GroundY - 130, 18, Color.DarkBlue);

            // Pesticide Clouds
            foreach (var cloud in state.ActiveClouds)
            {
                float alpha = (cloud.Duration / cloud.MaxDuration);
                Color gasColor = new Color((byte)90, (byte)210, (byte)90, (byte)(110 * alpha));
                Raylib.DrawCircleV(cloud.Position, cloud.Radius, gasColor);
                Raylib.DrawCircleLines((int)cloud.Position.X, (int)cloud.Position.Y, cloud.Radius, Color.Lime);
            }

            // Damp Cloth Sweep
            if (state.DampClothSweepX >= 0)
            {
                Raylib.DrawRectangle((int)state.DampClothSweepX - 160, 0, 160, GameConfig.ScreenHeight, new Color(130, 200, 240, 140));
                Raylib.DrawRectangle((int)state.DampClothSweepX - 20, 0, 20, GameConfig.ScreenHeight, Color.White);
            }

            // Projectiles
            foreach (var p in state.Projectiles)
            {
                Raylib.DrawCircleV(p.Position, p.Radius, p.VisualColor);
                if (p.IsAoe) Raylib.DrawCircleLines((int)p.Position.X, (int)p.Position.Y, p.Radius + 2f, Color.Gold);
            }

            // Units
            foreach (var u in state.Units)
            {
                Color boxColor = u.Faction == Faction.Ally ? Color.Beige : Color.Maroon;
                if (u.Type == UnitType.Flying) boxColor = u.Faction == Faction.Ally ? Color.Gold : Color.Purple;
                if (u.Type == UnitType.Mount) boxColor = u.Faction == Faction.Ally ? Color.Orange : new Color(100, 60, 30, 255);
                if (u.StunTimer > 0) boxColor = Color.SkyBlue;

                Raylib.DrawRectangleV(u.Position, u.Size, boxColor);
                Raylib.DrawRectangleLines((int)u.Position.X, (int)u.Position.Y, (int)u.Size.X, (int)u.Size.Y, Color.Black);

                float healthPct = Math.Clamp(u.Hp / u.MaxHp, 0f, 1f);
                Raylib.DrawRectangle((int)u.Position.X, (int)u.Position.Y - 10, (int)u.Size.X, 5, Color.Red);
                Raylib.DrawRectangle((int)u.Position.X, (int)u.Position.Y - 10, (int)(u.Size.X * healthPct), 5, Color.Green);

                string label = u.StunTimer > 0 ? "FROZEN" : u.Name;
                Raylib.DrawText(label, (int)u.Position.X, (int)u.Position.Y - 22, 10, u.StunTimer > 0 ? Color.Blue : Color.DarkGray);
            }

            // Ultimate Reticle
            if (state.UltimateTargetingActive && state.UltimateTimer <= 0)
            {
                Vector2 mouseWorld = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
                if (state.CakeTier == 1)
                {
                    Raylib.DrawCircleLines((int)mouseWorld.X, (int)mouseWorld.Y, 160f, new Color(255, 0, 0, 150));
                    Raylib.DrawCircleLines((int)mouseWorld.X, (int)mouseWorld.Y, 155f, Color.Red);
                }
                else if (state.CakeTier == 2)
                {
                    Raylib.DrawCircleLines((int)mouseWorld.X, (int)mouseWorld.Y, 160f, Color.Green);
                    Raylib.DrawCircle((int)mouseWorld.X, (int)mouseWorld.Y, 160f, new Color(50, 200, 50, 60));
                }
            }

            Raylib.EndMode2D();

            // ----------------- DRAW HUD -----------------
            UIRenderer.DrawTopHUD(state);
            UIRenderer.DrawBottomControlPanel(state);

            if (state.CurrentPhase == GamePhase.FlavorSelection)
            {
                UIRenderer.DrawFlavorSelectionModal(state);
            }
            else if (state.CurrentPhase == GamePhase.GameOver)
            {
                Raylib.DrawRectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight, new Color(0, 0, 0, 200));
                Raylib.DrawText("THE BUGS ATE THE CAKE! DEFEAT", GameConfig.ScreenWidth / 2 - 250, GameConfig.ScreenHeight / 2 - 20, 32, Color.Red);
            }
            else if (state.CurrentPhase == GamePhase.Victory)
            {
                Raylib.DrawRectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight, new Color(0, 0, 0, 200));
                Raylib.DrawText("WINDOW FULLY SEALED! VICTORY!", GameConfig.ScreenWidth / 2 - 260, GameConfig.ScreenHeight / 2 - 20, 32, Color.Green);
            }

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    static void UpdateGame(float dt)
    {
        state.Crumbs += state.CrumbRate * dt;
        state.GameElapsedTime += dt;
        Vector2 mouseScreen = Raylib.GetMousePosition();

        // 1. Proportional Mouse Edge Panning
        if (mouseScreen.Y < 590)
        {
            if (mouseScreen.X >= GameConfig.ScreenWidth - GameConfig.EdgeThreshold && mouseScreen.X <= GameConfig.ScreenWidth)
            {
                float factor = (mouseScreen.X - (GameConfig.ScreenWidth - GameConfig.EdgeThreshold)) / GameConfig.EdgeThreshold;
                camera.Target.X += GameConfig.MaxPanSpeed * factor * dt;
            }
            else if (mouseScreen.X <= GameConfig.EdgeThreshold && mouseScreen.X >= 0)
            {
                float factor = (GameConfig.EdgeThreshold - mouseScreen.X) / GameConfig.EdgeThreshold;
                camera.Target.X -= GameConfig.MaxPanSpeed * factor * dt;
            }
        }

        if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)) camera.Target.X += 800f * dt;
        if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left)) camera.Target.X -= 800f * dt;
        camera.Target.X = Math.Clamp(camera.Target.X, 0, GameConfig.TableWidth - GameConfig.ScreenWidth);

        // 2. Lingering Gas Clouds (Bug Spray)
        for (int i = state.ActiveClouds.Count - 1; i >= 0; i--)
        {
            var cloud = state.ActiveClouds[i];
            cloud.Duration -= dt;

            foreach (var u in state.Units)
            {
                if (u.Faction == Faction.Enemy)
                {
                    Vector2 uCenter = u.Position + u.Size / 2;
                    if (Vector2.Distance(cloud.Position, uCenter) <= cloud.Radius)
                    {
                        u.Hp -= cloud.DamagePerSecond * dt;
                        u.Speed = u.BaseSpeed * 0.45f;
                        if (u.Hp <= 0)
                        {
                            CombatSystem.HandleKill(state, u);
                        }
                    }
                }
            }

            if (cloud.Duration <= 0)
            {
                state.ActiveClouds.RemoveAt(i);
            }
        }

        // 3. Damp Cloth Wipe Sweep
        if (state.DampClothSweepX >= 0)
        {
            state.DampClothSweepX += 2800f * dt;
            if (state.DampClothSweepX > GameConfig.TableWidth + 300f)
            {
                state.DampClothSweepX = -1f;
            }
        }

        // 4. Cooldown & Ultimate Activation
        if (state.UltimateTimer > 0) state.UltimateTimer -= dt;

        if (state.UltimateTargetingActive && state.UltimateTimer <= 0)
        {
            if (state.CakeTier >= 3)
            {
                state.UltimateTimer = 45f;
                state.UltimateTargetingActive = false;
                state.DampClothSweepX = 0f;

                for (int uIdx = state.Units.Count - 1; uIdx >= 0; uIdx--)
                {
                    var u = state.Units[uIdx];
                    if (u.Faction == Faction.Enemy)
                    {
                        u.Hp -= 550f;
                        if (u.Hp <= 0)
                        {
                            CombatSystem.HandleKill(state, u);
                            state.Units.RemoveAt(uIdx);
                        }
                    }
                }
            }
            else if (Raylib.IsMouseButtonPressed(MouseButton.Left) && mouseScreen.Y < 590 && mouseScreen.Y > 60)
            {
                Vector2 mouseWorld = Raylib.GetScreenToWorld2D(mouseScreen, camera);

                if (state.CakeTier == 1)
                {
                    state.UltimateTimer = 15f;
                    state.UltimateTargetingActive = false;

                    for (int uIdx = state.Units.Count - 1; uIdx >= 0; uIdx--)
                    {
                        var u = state.Units[uIdx];
                        if (u.Faction == Faction.Enemy && Vector2.Distance(u.Position + u.Size / 2, mouseWorld) <= 160f)
                        {
                            u.Hp -= 220f;
                            if (u.Hp <= 0)
                            {
                                CombatSystem.HandleKill(state, u);
                                state.Units.RemoveAt(uIdx);
                            }
                        }
                    }
                }
                else if (state.CakeTier == 2)
                {
                    state.UltimateTimer = 25f;
                    state.UltimateTargetingActive = false;

                    state.ActiveClouds.Add(new GasCloud
                    {
                        Position = mouseWorld,
                        Radius = 160f,
                        Duration = 4.0f,
                        MaxDuration = 4.0f,
                        DamagePerSecond = 65f
                    });
                }
            }
            else if (Raylib.IsMouseButtonPressed(MouseButton.Right))
            {
                state.UltimateTargetingActive = false;
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Space) && state.UltimateTimer <= 0)
        {
            state.UltimateTargetingActive = !state.UltimateTargetingActive;
        }

        // 5. Input, Oven & Waves
        UpdateBottomPanelInput();
        SpawnerSystem.UpdateOven(state, dt);

        state.EnemySpawnTimer -= dt;
        if (state.EnemySpawnTimer <= 0)
        {
            SpawnerSystem.SpawnEnemyWave(state);
            state.EnemySpawnTimer = Math.Max(1.8f, 4.0f - (state.CakeTier * 0.6f) - (state.GameElapsedTime / 120f));
        }

        // 6. Combat Updates
        CombatSystem.UpdateUnits(state, dt);
        CombatSystem.UpdateTurrets(state, dt);
        CombatSystem.UpdateProjectiles(state, dt);

        // 7. Evolution & Game Conditions
        if (state.CakeTier < 3 && state.CakeExp >= state.ExpMilestones[state.CakeTier])
        {
            state.CakeTier++;
            state.CurrentPhase = GamePhase.FlavorSelection;
            RollFlavorChoices();
        }

        if (state.CakeHp <= 0) state.CurrentPhase = GamePhase.GameOver;
        if (state.WindowSealed >= state.MaxWindowSealed) state.CurrentPhase = GamePhase.Victory;
    }

    static void UpdateBottomPanelInput()
    {
        if (!Raylib.IsMouseButtonPressed(MouseButton.Left)) return;
        Vector2 m = Raylib.GetMousePosition();

        Rectangle ultimateBtn = new Rectangle(1030, 10, 230, 40);
        if (Raylib.CheckCollisionPointRec(m, ultimateBtn) && state.UltimateTimer <= 0)
        {
            state.UltimateTargetingActive = !state.UltimateTargetingActive;
            return;
        }

        if (Raylib.CheckCollisionPointRec(m, new Rectangle(20, 600, 140, 36))) { state.ActiveTab = BottomTab.Recipes; return; }
        if (Raylib.CheckCollisionPointRec(m, new Rectangle(170, 600, 140, 36))) { state.ActiveTab = BottomTab.Cake; return; }
        if (Raylib.CheckCollisionPointRec(m, new Rectangle(320, 600, 140, 36))) { state.ActiveTab = BottomTab.Toppings; return; }

        if (state.ActiveTab == BottomTab.Recipes)
        {
            int gCost = state.CakeTier == 1 ? 25 : (state.CakeTier == 2 ? 45 : 75);
            string gName = state.CakeTier == 1 ? "Wet Flour" : (state.CakeTier == 2 ? "Naked Cupcake" : "Frosted Cupcake");
            float gHp = (state.CakeTier == 1 ? 110f : (state.CakeTier == 2 ? 180f : 290f)) * state.AllyDefenseMultiplier;
            float gSpeed = state.CakeTier == 1 ? 75f : (state.CakeTier == 2 ? 70f : 68f);
            float gDmg = (state.CakeTier == 1 ? 18f : (state.CakeTier == 2 ? 28f : 44f)) * state.AllyDamageMultiplier;
            float gTime = state.CakeTier == 1 ? 2.0f : (state.CakeTier == 2 ? 3.0f : 4.5f);

            if (Raylib.CheckCollisionPointRec(m, new Rectangle(20, 646, 170, 60)) && state.Crumbs >= gCost && state.OvenQueue.Count < 4)
            {
                state.Crumbs -= gCost;
                state.OvenQueue.Enqueue(new BakingOrder
                {
                    Name = gName,
                    Type = UnitType.Ground,
                    Hp = gHp,
                    Speed = gSpeed,
                    Damage = gDmg,
                    AttackRange = 40f,
                    AttackCooldown = 1.0f,
                    BakeTimeTotal = gTime,
                    BakeTimeRemaining = gTime
                });
            }

            int rCost = state.CakeTier == 1 ? 50 : (state.CakeTier == 2 ? 80 : 120);
            string rName = state.CakeTier == 1 ? "Cookie Dough" : (state.CakeTier == 2 ? "Baked Cookie" : "Choco Cookie");
            float rHp = (state.CakeTier == 1 ? 65f : (state.CakeTier == 2 ? 100f : 150f)) * state.AllyDefenseMultiplier;
            float rSpeed = state.CakeTier == 1 ? 60f : (state.CakeTier == 2 ? 65f : 65f);
            float rDmg = (state.CakeTier == 1 ? 14f : (state.CakeTier == 2 ? 24f : 38f)) * state.AllyDamageMultiplier;
            float rRange = state.CakeTier == 1 ? 250f : (state.CakeTier == 2 ? 280f : 320f);
            float rCooldown = state.CakeTier == 1 ? 1.4f : (state.CakeTier == 2 ? 1.2f : 1.1f);
            float rTime = state.CakeTier == 1 ? 3.5f : (state.CakeTier == 2 ? 4.0f : 5.0f);

            if (Raylib.CheckCollisionPointRec(m, new Rectangle(200, 646, 170, 60)) && state.Crumbs >= rCost && state.OvenQueue.Count < 4)
            {
                state.Crumbs -= rCost;
                state.OvenQueue.Enqueue(new BakingOrder
                {
                    Name = rName,
                    Type = UnitType.Ranged,
                    Hp = rHp,
                    Speed = rSpeed,
                    Damage = rDmg,
                    AttackRange = rRange,
                    AttackCooldown = rCooldown,
                    BakeTimeTotal = rTime,
                    BakeTimeRemaining = rTime
                });
            }

            int mCost = state.CakeTier == 1 ? 90 : (state.CakeTier == 2 ? 140 : 210);
            string mName = state.CakeTier == 1 ? "Long Dough" : (state.CakeTier == 2 ? "Long Bread" : "Eclair");
            float mHp = (state.CakeTier == 1 ? 320f : (state.CakeTier == 2 ? 550f : 880f)) * state.AllyDefenseMultiplier;
            float mSpeed = state.CakeTier == 1 ? 45f : (state.CakeTier == 2 ? 42f : 40f);
            float mDmg = (state.CakeTier == 1 ? 35f : (state.CakeTier == 2 ? 55f : 85f)) * state.AllyDamageMultiplier;
            float mTime = state.CakeTier == 1 ? 6.0f : (state.CakeTier == 2 ? 7.5f : 9.0f);

            if (Raylib.CheckCollisionPointRec(m, new Rectangle(380, 646, 170, 60)) && state.Crumbs >= mCost && state.OvenQueue.Count < 4)
            {
                state.Crumbs -= mCost;
                state.OvenQueue.Enqueue(new BakingOrder
                {
                    Name = mName,
                    Type = UnitType.Mount,
                    Hp = mHp,
                    Speed = mSpeed,
                    Damage = mDmg,
                    AttackRange = 50f,
                    AttackCooldown = 1.8f,
                    BakeTimeTotal = mTime,
                    BakeTimeRemaining = mTime
                });
            }

            int fCost = state.CakeTier == 1 ? 80 : (state.CakeTier == 2 ? 110 : 150);
            string fName = state.CakeTier == 1 ? "Flour Puff" : (state.CakeTier == 2 ? "Powder Sugar" : "Cocoa Powder");
            float fHp = (state.CakeTier == 1 ? 90f : (state.CakeTier == 2 ? 140f : 210f)) * state.AllyDefenseMultiplier;
            float fSpeed = state.CakeTier == 1 ? 90f : (state.CakeTier == 2 ? 95f : 100f);
            float fDmg = (state.CakeTier == 1 ? 22f : (state.CakeTier == 2 ? 34f : 50f)) * state.AllyDamageMultiplier;
            float fTime = state.CakeTier == 1 ? 4.0f : (state.CakeTier == 2 ? 4.5f : 5.5f);

            if (Raylib.CheckCollisionPointRec(m, new Rectangle(560, 646, 170, 60)) && state.Crumbs >= fCost && state.OvenQueue.Count < 4)
            {
                state.Crumbs -= fCost;
                state.OvenQueue.Enqueue(new BakingOrder
                {
                    Name = fName,
                    Type = UnitType.Flying,
                    Hp = fHp,
                    Speed = fSpeed,
                    Damage = fDmg,
                    AttackRange = 160f,
                    AttackCooldown = 1.1f,
                    BakeTimeTotal = fTime,
                    BakeTimeRemaining = fTime
                });
            }
        }
        else if (state.ActiveTab == BottomTab.Cake)
        {
            int layerCost = state.CakeLayersUnlocked == 1 ? 120 : (state.CakeLayersUnlocked == 2 ? 250 : 0);
            Rectangle layerBtn = new Rectangle(20, 646, 260, 60);

            if (state.CakeLayersUnlocked < 3 && Raylib.CheckCollisionPointRec(m, layerBtn) && state.Crumbs >= layerCost)
            {
                state.Crumbs -= layerCost;
                state.CakeTurrets[state.CakeLayersUnlocked].Unlocked = true;
                state.CakeLayersUnlocked++;
            }
        }
        else if (state.ActiveTab == BottomTab.Toppings)
        {
            for (int i = 0; i < 3; i++)
            {
                var slot = state.CakeTurrets[i];
                if (!slot.Unlocked || slot.HasTurret) continue;

                int rapidCost = state.CakeTier == 1 ? 60 : (state.CakeTier == 2 ? 110 : 180);
                string rapidName = state.CakeTier == 1 ? "Sprinkles" : (state.CakeTier == 2 ? "Choco Chip" : "Cherry Cannon");
                Rectangle btn1 = new Rectangle(20 + (i * 400), 646, 125, 56);
                if (Raylib.CheckCollisionPointRec(m, btn1) && state.Crumbs >= rapidCost)
                {
                    state.Crumbs -= rapidCost;
                    slot.HasTurret = true;
                    slot.Name = rapidName;
                    slot.IsFlamethrower = false;
                    slot.Type = state.CakeTier == 1 ? TurretType.Sprinkles : (state.CakeTier == 2 ? TurretType.ChocolateChip : TurretType.CherryCannon);
                    slot.Range = state.CakeTier == 3 ? 420f : 340f;
                    slot.Damage = state.CakeTier == 1 ? 14f : (state.CakeTier == 2 ? 26f : 55f);
                    slot.Cooldown = state.CakeTier == 3 ? 1.0f : 0.75f;
                    break;
                }

                int aoeCost = state.CakeTier == 1 ? 80 : (state.CakeTier == 2 ? 130 : 200);
                string aoeName = state.CakeTier == 1 ? "Popping Candy" : (state.CakeTier == 2 ? "Marshmallow" : "Whipped Cream");
                Rectangle btn2 = new Rectangle(150 + (i * 400), 646, 125, 56);
                if (Raylib.CheckCollisionPointRec(m, btn2) && state.Crumbs >= aoeCost)
                {
                    state.Crumbs -= aoeCost;
                    slot.HasTurret = true;
                    slot.Name = aoeName;
                    slot.IsFlamethrower = false;
                    slot.Type = state.CakeTier == 1 ? TurretType.PoppingCandy : (state.CakeTier == 2 ? TurretType.Marshmallow : TurretType.WhippedCream);
                    slot.Range = 360f;
                    slot.Damage = state.CakeTier == 1 ? 25f : (state.CakeTier == 2 ? 50f : 90f);
                    slot.Cooldown = 1.6f;
                    break;
                }

                int flameCost = state.CakeTier == 1 ? 70 : (state.CakeTier == 2 ? 120 : 190);
                string flameName = state.CakeTier == 1 ? "B-Day Candle" : (state.CakeTier == 2 ? "Firework" : "Creme Brulee");
                Rectangle btn3 = new Rectangle(280 + (i * 400), 646, 125, 56);
                if (Raylib.CheckCollisionPointRec(m, btn3) && state.Crumbs >= flameCost)
                {
                    state.Crumbs -= flameCost;
                    slot.HasTurret = true;
                    slot.Name = flameName;
                    slot.IsFlamethrower = true;
                    slot.Type = state.CakeTier == 1 ? TurretType.BirthdayCandle : (state.CakeTier == 2 ? TurretType.FireworkCandle : TurretType.CremeBruleeTorch);
                    slot.Range = state.CakeTier == 3 ? 240f : 180f;
                    slot.Damage = state.CakeTier == 1 ? 8f : (state.CakeTier == 2 ? 16f : 30f);
                    slot.Cooldown = 0.25f;
                    break;
                }
            }
        }
    }

    static void RollFlavorChoices()
    {
        state.SelectedChoices.Clear();
        List<FlavorOption> available = new List<FlavorOption>(state.FlavorPool);
        Random rnd = new Random();

        for (int i = 0; i < 3 && available.Count > 0; i++)
        {
            int index = rnd.Next(available.Count);
            state.SelectedChoices.Add(available[index]);
            available.RemoveAt(index);
        }
    }

    static void UpdateFlavorSelection()
    {
        Vector2 mousePos = Raylib.GetMousePosition();

        for (int i = 0; i < state.SelectedChoices.Count; i++)
        {
            Rectangle cardRect = new Rectangle(240 + (i * 280), 240, 240, 260);

            if (Raylib.CheckCollisionPointRec(mousePos, cardRect) && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                var chosen = state.SelectedChoices[i];
                chosen.IsActiveQuest = true;
                state.ActiveQuests.Add(chosen);
                state.FlavorPool.Remove(chosen);
                state.CurrentPhase = GamePhase.Playing;
                break;
            }
        }
    }
}