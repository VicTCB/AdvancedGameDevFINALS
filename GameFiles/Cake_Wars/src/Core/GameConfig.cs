namespace CakeWars;

public enum Faction { Ally, Enemy }
public enum UnitType { Ground, Ranged, Mount, Flying }
public enum TurretType 
{ 
    Sprinkles, ChocolateChip, CherryCannon,
    PoppingCandy, Marshmallow, WhippedCream,
    BirthdayCandle, FireworkCandle, CremeBruleeTorch 
}
public enum GamePhase { Playing, FlavorSelection, GameOver, Victory }
public enum BottomTab { Recipes, Cake, Toppings }

public static class GameConfig
{
    public const int ScreenWidth = 1280;
    public const int ScreenHeight = 720;
    public const float TableWidth = 2400f;
    public const float GroundY = 540f;
    public const float AirY = 340f;
    public const float AllySpawnX = 180f;

    public const float MaxPanSpeed = 1100f;
    public const float EdgeThreshold = 180f;

    // ---------------- Balance ----------------
    // Enemies evolve on their own clock (seconds into the game), independent of the player's cake tier
    public const float EnemyTier2Time = 240f; // 4:00
    public const float EnemyTier3Time = 480f; // 8:00

    // Enemy spawn interval: starts slow, tightens over time, reaches the floor at 5:00
    public const float SpawnIntervalStart = 5.0f;
    public const float SpawnIntervalFloor = 2.0f;
    public const float SpawnIntervalRampSeconds = 100f; // interval shrinks by 1s every 100s

    public const float CakeHpPerLevelUp = 500f;
    public const float WindowSealPerAllyPerSecond = 1.0f;

    // Ultimate base cooldowns per cake tier (multiplied by GameState.UltimateCooldownMultiplier)
    public static float UltimateBaseCooldown(int tier) => tier == 1 ? 30f : (tier == 2 ? 40f : 60f);

    public static int LayerCost(int layersUnlocked) => layersUnlocked == 1 ? 150 : (layersUnlocked == 2 ? 350 : 0);

    public static int RangedUnitCost(int tier) => tier == 1 ? 35 : (tier == 2 ? 55 : 85);
}