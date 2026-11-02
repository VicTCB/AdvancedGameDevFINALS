using System.Numerics;
using Raylib_cs;

namespace CakeWars;

public static class UIRenderer
{
    public static void DrawCake(GameState state)
    {
        Color tierFrostingColor = state.CakeTier switch
        {
            2 => Color.Pink,
            3 => Color.White,
            _ => new Color(220, 175, 115, 255)
        };

        Raylib.DrawRectangle(20, (int)GameConfig.GroundY - 80, 160, 80, new Color(240, 190, 130, 255));
        Raylib.DrawRectangle(20, (int)GameConfig.GroundY - 80, 160, 10, tierFrostingColor);

        if (state.CakeLayersUnlocked >= 2)
        {
            Raylib.DrawRectangle(35, (int)GameConfig.GroundY - 140, 130, 60, new Color(245, 205, 145, 255));
            Raylib.DrawRectangle(35, (int)GameConfig.GroundY - 140, 130, 10, tierFrostingColor);
        }

        if (state.CakeLayersUnlocked >= 3)
        {
            Raylib.DrawRectangle(50, (int)GameConfig.GroundY - 190, 100, 50, new Color(250, 215, 160, 255));
            Raylib.DrawRectangle(50, (int)GameConfig.GroundY - 190, 100, 10, tierFrostingColor);
        }

        for (int i = 0; i < 3; i++)
        {
            Vector2 pos = new Vector2(20, GameConfig.GroundY) + state.CakeTurrets[i].RelativeOffset;
            if (state.CakeTurrets[i].Unlocked)
            {
                if (state.CakeTurrets[i].HasTurret)
                {
                    Color boxCol = state.CakeTurrets[i].IsFlamethrower ? Color.Orange : Color.Magenta;
                    Raylib.DrawRectangle((int)pos.X, (int)pos.Y, 20, 20, boxCol);
                    Raylib.DrawText("T", (int)pos.X + 5, (int)pos.Y + 2, 14, Color.White);
                }
                else
                {
                    Raylib.DrawRectangleLines((int)pos.X, (int)pos.Y, 20, 20, Color.DarkGray);
                }
            }
        }
    }

    public static void DrawTopHUD(GameState state)
    {
        Raylib.DrawRectangle(0, 0, GameConfig.ScreenWidth, 60, new Color(0, 0, 0, 180));

        Raylib.DrawText($"Crumbs: {(int)state.Crumbs}", 20, 18, 22, Color.Gold);
        Raylib.DrawText($"Cake HP: {(int)state.CakeHp}/{state.MaxCakeHp}", 210, 18, 20, Color.RayWhite);
        Raylib.DrawText($"Window Sealed: {state.WindowSealed:F1}%", 460, 18, 20, Color.SkyBlue);

        string tierName = state.CakeTier == 1 ? "Plain Sponge" : (state.CakeTier == 2 ? "Frosted" : "Frosted + Iced");
        Raylib.DrawText($"Cake: {tierName}", 740, 12, 16, Color.Yellow);

        if (state.CakeTier < 3)
        {
            float targetExp = state.ExpMilestones[state.CakeTier];
            float expPct = Math.Clamp(state.CakeExp / targetExp, 0f, 1f);
            Raylib.DrawRectangle(740, 32, 160, 10, Color.DarkGray);
            Raylib.DrawRectangle(740, 32, (int)(160 * expPct), 10, Color.Orange);
            Raylib.DrawText($"EXP: {(int)state.CakeExp}/{(int)targetExp}", 910, 30, 12, Color.RayWhite);
        }
        else
        {
            Raylib.DrawText("MAX TIER", 740, 32, 12, Color.Green);
        }

        Rectangle ultimateBtn = new Rectangle(1030, 10, 230, 40);
        Vector2 m = Raylib.GetMousePosition();
        bool hovered = Raylib.CheckCollisionPointRec(m, ultimateBtn);

        Color btnColor = state.UltimateTimer <= 0
            ? (state.UltimateTargetingActive ? Color.Red : (hovered ? Color.DarkGreen : Color.Green))
            : Color.DarkGray;

        Raylib.DrawRectangleRec(ultimateBtn, btnColor);
        Raylib.DrawRectangleLinesEx(ultimateBtn, 1, Color.White);

        string abilityName = state.CakeTier == 1 ? "BUG SWATTER" : (state.CakeTier == 2 ? "BUG SPRAY" : "DAMP CLOTH");
        string ultimateLabel = state.UltimateTimer <= 0
            ? (state.UltimateTargetingActive ? (state.CakeTier == 3 ? "WIPING TABLE..." : "CLICK TO DEPLOY") : $"[READY] {abilityName}")
            : $"{abilityName}: {state.UltimateTimer:F1}s";

        Raylib.DrawText(ultimateLabel, (int)ultimateBtn.X + 10, (int)ultimateBtn.Y + 12, 13, Color.White);

        int questY = 70;
        foreach (var q in state.ActiveQuests)
        {
            Color qColor = q.Completed ? Color.Green : Color.Yellow;
            string status = q.Completed ? "ACTIVE" : $"{q.CurrentCount}/{q.TargetCount}";
            Raylib.DrawText($"Flavor: {q.Name} ({status}) - {q.BuffDescription}", GameConfig.ScreenWidth - 450, questY, 14, qColor);
            questY += 20;
        }
    }

    public static void DrawBottomControlPanel(GameState state)
    {
        Raylib.DrawRectangle(0, 590, GameConfig.ScreenWidth, 130, new Color(20, 20, 25, 235));
        Raylib.DrawLine(0, 590, GameConfig.ScreenWidth, 590, Color.Gold);

        Vector2 m = Raylib.GetMousePosition();

        DrawTabButton("RECIPES", BottomTab.Recipes, state.ActiveTab, new Rectangle(20, 600, 140, 36), m);
        DrawTabButton("CAKE", BottomTab.Cake, state.ActiveTab, new Rectangle(170, 600, 140, 36), m);
        DrawTabButton("TOPPINGS", BottomTab.Toppings, state.ActiveTab, new Rectangle(320, 600, 140, 36), m);

        if (state.OvenQueue.Count > 0)
        {
            var top = state.OvenQueue.Peek();
            float pct = Math.Clamp(1f - (top.BakeTimeRemaining / top.BakeTimeTotal), 0f, 1f);
            Raylib.DrawText($"Oven: {top.Name} ({state.OvenQueue.Count} queued)", 500, 608, 15, Color.Orange);
            Raylib.DrawRectangle(730, 611, 120, 12, Color.DarkGray);
            Raylib.DrawRectangle(730, 611, (int)(120 * pct), 12, Color.Gold);
        }
        else
        {
            Raylib.DrawText("Oven: Idle", 500, 608, 15, Color.Gray);
        }

        if (state.ActiveTab == BottomTab.Recipes)
        {
            int gCost = state.CakeTier == 1 ? 25 : (state.CakeTier == 2 ? 45 : 75);
            string gName = state.CakeTier == 1 ? "Wet Flour" : (state.CakeTier == 2 ? "Naked Cupcake" : "Frosted Cupcake");
            float gTime = state.CakeTier == 1 ? 2.0f : (state.CakeTier == 2 ? 3.0f : 4.5f);
            DrawActionButton(new Rectangle(20, 646, 170, 60), gName, $"{gCost}c | {gTime:F1}s", state.Crumbs >= gCost && state.OvenQueue.Count < 4, m);

            int rCost = state.CakeTier == 1 ? 50 : (state.CakeTier == 2 ? 80 : 120);
            string rName = state.CakeTier == 1 ? "Cookie Dough" : (state.CakeTier == 2 ? "Baked Cookie" : "Choco Cookie");
            float rTime = state.CakeTier == 1 ? 3.5f : (state.CakeTier == 2 ? 4.0f : 5.0f);
            DrawActionButton(new Rectangle(200, 646, 170, 60), rName, $"{rCost}c | {rTime:F1}s", state.Crumbs >= rCost && state.OvenQueue.Count < 4, m);

            int mCost = state.CakeTier == 1 ? 90 : (state.CakeTier == 2 ? 140 : 210);
            string mName = state.CakeTier == 1 ? "Long Dough" : (state.CakeTier == 2 ? "Long Bread" : "Eclair");
            float mTime = state.CakeTier == 1 ? 6.0f : (state.CakeTier == 2 ? 7.5f : 9.0f);
            DrawActionButton(new Rectangle(380, 646, 170, 60), mName, $"{mCost}c | {mTime:F1}s", state.Crumbs >= mCost && state.OvenQueue.Count < 4, m);

            int fCost = state.CakeTier == 1 ? 80 : (state.CakeTier == 2 ? 110 : 150);
            string fName = state.CakeTier == 1 ? "Flour Puff" : (state.CakeTier == 2 ? "Powder Sugar" : "Cocoa Powder");
            float fTime = state.CakeTier == 1 ? 4.0f : (state.CakeTier == 2 ? 4.5f : 5.5f);
            DrawActionButton(new Rectangle(560, 646, 170, 60), fName, $"{fCost}c | {fTime:F1}s", state.Crumbs >= fCost && state.OvenQueue.Count < 4, m);
        }
        else if (state.ActiveTab == BottomTab.Cake)
        {
            int layerCost = state.CakeLayersUnlocked == 1 ? 120 : (state.CakeLayersUnlocked == 2 ? 250 : 0);
            Rectangle layerBtn = new Rectangle(20, 646, 260, 60);

            if (state.CakeLayersUnlocked < 3)
            {
                DrawActionButton(layerBtn, $"Upgrade Layer {state.CakeLayersUnlocked + 1}", $"Cost: {layerCost} Crumbs", state.Crumbs >= layerCost, m);
            }
            else
            {
                DrawLockedButton(layerBtn, "MAX LAYERS (3/3)");
            }

            Raylib.DrawText($"Current Cake Layers: {state.CakeLayersUnlocked}/3", 310, 655, 16, Color.Gold);
            Raylib.DrawText("Each layer allows mounting 1 additional Turret.", 310, 678, 14, Color.LightGray);
        }
        else if (state.ActiveTab == BottomTab.Toppings)
        {
            for (int i = 0; i < 3; i++)
            {
                var slot = state.CakeTurrets[i];
                if (!slot.Unlocked)
                {
                    DrawLockedButton(new Rectangle(20 + (i * 400), 646, 385, 56), $"Layer {i + 1} Slot: Locked (Upgrade Cake First)");
                }
                else if (slot.HasTurret)
                {
                    DrawLockedButton(new Rectangle(20 + (i * 400), 646, 385, 56), $"Layer {i + 1}: {slot.Name} [MOUNTED]");
                }
                else
                {
                    int rapidCost = state.CakeTier == 1 ? 60 : (state.CakeTier == 2 ? 110 : 180);
                    string rapidName = state.CakeTier == 1 ? "Sprinkles" : (state.CakeTier == 2 ? "Choco Chip" : "Cherry");

                    int aoeCost = state.CakeTier == 1 ? 80 : (state.CakeTier == 2 ? 130 : 200);
                    string aoeName = state.CakeTier == 1 ? "Popping Candy" : (state.CakeTier == 2 ? "Marshmallow" : "Whipped Cream");

                    int flameCost = state.CakeTier == 1 ? 70 : (state.CakeTier == 2 ? 120 : 190);
                    string flameName = state.CakeTier == 1 ? "B-Day Candle" : (state.CakeTier == 2 ? "Firework" : "Creme Brulee");

                    DrawActionButton(new Rectangle(20 + (i * 400), 646, 125, 56), rapidName, $"{rapidCost}c", state.Crumbs >= rapidCost, m);
                    DrawActionButton(new Rectangle(150 + (i * 400), 646, 125, 56), aoeName, $"{aoeCost}c (AOE)", state.Crumbs >= aoeCost, m);
                    DrawActionButton(new Rectangle(280 + (i * 400), 646, 125, 56), flameName, $"{flameCost}c (Melt)", state.Crumbs >= flameCost, m);
                }
            }
        }
    }

    private static void DrawTabButton(string label, BottomTab tab, BottomTab activeTab, Rectangle rect, Vector2 mousePos)
    {
        bool isActive = (activeTab == tab);
        bool hovered = Raylib.CheckCollisionPointRec(mousePos, rect);

        Color col = isActive ? new Color(210, 160, 80, 255) : (hovered ? Color.DarkGray : new Color(50, 50, 60, 255));
        Raylib.DrawRectangleRec(rect, col);
        Raylib.DrawRectangleLinesEx(rect, 1, isActive ? Color.Gold : Color.Gray);

        Raylib.DrawText(label, (int)rect.X + 25, (int)rect.Y + 10, 16, isActive ? Color.Black : Color.White);
    }

    private static void DrawActionButton(Rectangle rect, string title, string subtext, bool canAfford, Vector2 mousePos)
    {
        bool hovered = Raylib.CheckCollisionPointRec(mousePos, rect);
        Color col = canAfford ? (hovered ? new Color(90, 130, 90, 255) : new Color(55, 85, 55, 255)) : new Color(60, 60, 65, 255);

        Raylib.DrawRectangleRec(rect, col);
        Raylib.DrawRectangleLinesEx(rect, 1, canAfford ? Color.Green : Color.DarkGray);

        Raylib.DrawText(title, (int)rect.X + 8, (int)rect.Y + 10, 14, canAfford ? Color.White : Color.Gray);
        Raylib.DrawText(subtext, (int)rect.X + 8, (int)rect.Y + 32, 12, canAfford ? Color.Gold : Color.DarkGray);
    }

    private static void DrawLockedButton(Rectangle rect, string title)
    {
        Raylib.DrawRectangleRec(rect, new Color(40, 40, 45, 255));
        Raylib.DrawRectangleLinesEx(rect, 1, Color.DarkGray);
        Raylib.DrawText(title, (int)rect.X + 10, (int)rect.Y + 20, 13, Color.Gray);
    }

    public static void DrawFlavorSelectionModal(GameState state)
    {
        Raylib.DrawRectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight, new Color(0, 0, 0, 190));
        Raylib.DrawText("CAKE EVOLVED! SELECT A FLAVOR BUFF", GameConfig.ScreenWidth / 2 - 280, 160, 28, Color.Gold);

        Vector2 mousePos = Raylib.GetMousePosition();

        for (int i = 0; i < state.SelectedChoices.Count; i++)
        {
            var choice = state.SelectedChoices[i];
            Rectangle cardRect = new Rectangle(240 + (i * 280), 240, 240, 260);

            bool hovered = Raylib.CheckCollisionPointRec(mousePos, cardRect);
            Raylib.DrawRectangleRec(cardRect, hovered ? new Color(255, 245, 220, 255) : Color.RayWhite);
            Raylib.DrawRectangleLinesEx(cardRect, hovered ? 3 : 1, hovered ? Color.Gold : Color.DarkGray);

            Raylib.DrawText(choice.Name, (int)cardRect.X + 20, (int)cardRect.Y + 25, 22, Color.DarkBrown);
            Raylib.DrawText("Buff:", (int)cardRect.X + 20, (int)cardRect.Y + 70, 16, Color.Black);
            Raylib.DrawText(choice.BuffDescription, (int)cardRect.X + 20, (int)cardRect.Y + 95, 14, Color.DarkGreen);

            Raylib.DrawText("Quest Objective:", (int)cardRect.X + 20, (int)cardRect.Y + 145, 16, Color.Black);
            Raylib.DrawText(choice.QuestDescription, (int)cardRect.X + 20, (int)cardRect.Y + 170, 14, Color.Maroon);

            Raylib.DrawRectangle((int)cardRect.X + 20, (int)cardRect.Y + 210, 200, 30, hovered ? Color.Green : Color.DarkGreen);
            Raylib.DrawText("CHOOSE", (int)cardRect.X + 80, (int)cardRect.Y + 218, 14, Color.White);
        }
    }
}