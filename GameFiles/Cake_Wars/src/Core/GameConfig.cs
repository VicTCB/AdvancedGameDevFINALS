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
}