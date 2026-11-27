using Raylib_cs;

namespace CakeWars;

public static class AssetManager
{
    public static Texture2D BackgroundTex;
    public static Texture2D TableTex;
    public static bool HasBackground = false;
    public static bool HasTable = false;

    public static Dictionary<string, Texture2D> IdleTextures = new();
    public static Dictionary<string, Texture2D> AttackTextures = new();
    public static Dictionary<string, float> SpriteSizes = new();

    // Key format: "{flavor}_{position}_{tier}" -> e.g. "ube_bottom_1"
    public static Dictionary<string, Texture2D> CakeTextures = new();

    public static void LoadAll()
    {
        if (File.Exists("assets/background.png"))
        {
            BackgroundTex = Raylib.LoadTexture("assets/background.png");
            HasBackground = true;
        }

        if (File.Exists("assets/bgtable.png"))
        {
            TableTex = Raylib.LoadTexture("assets/bgtable.png");
            HasTable = true;
        }

        LoadFriendUnits();
        LoadAllCakes();
    }

    private static void LoadFriendUnits()
    {
        // 1. Ground Units (Swapped: Long Dough -> Long Bread -> Eclair)
        LoadUnitSprite("Long Dough", "assets/FriendUnit/Ground/TIER1/fground_t1_idle.png", "assets/FriendUnit/Ground/TIER1/fground_t1_atk.png", 160f);
        LoadUnitSprite("Long Bread", "assets/FriendUnit/Ground/TIER2/fground_t2_idle.png", "assets/FriendUnit/Ground/TIER2/fground_t2_atk.png", 160f);
        LoadUnitSprite("Eclair", "assets/FriendUnit/Ground/TIER3/fground_t3_idle.png", "assets/FriendUnit/Ground/TIER3/fground_t3_atk.png", 165f);

        // 2. Ranged Units (Cookie Dough -> Baked Cookie -> Choco Cookie)
        LoadUnitSprite("Cookie Dough", "assets/FriendUnit/Range/TIER1/frange_t1_idle.png", "assets/FriendUnit/Range/TIER1/frange_t1_atk.png", 150f);
        LoadUnitSprite("Baked Cookie", "assets/FriendUnit/Range/TIER2/frange_t2_idle.png", "assets/FriendUnit/Range/TIER2/frange_t2_atk.png", 150f);
        LoadUnitSprite("Choco Cookie", "assets/FriendUnit/Range/TIER3/frange_t3_idle.png", "assets/FriendUnit/Range/TIER3/frange_t3_atk.png", 155f);

        // 3. Big / Tank Units (Swapped: Wet Flour -> Naked Cupcake -> Frosted Cupcake)
        LoadUnitSprite("Wet Flour", "assets/FriendUnit/Big/TIER1/fbig_t1_idle.png", "assets/FriendUnit/Big/TIER1/fbig_t1_atk.png", 240f);
        LoadUnitSprite("Naked Cupcake", "assets/FriendUnit/Big/TIER2/fbig_t2_idle.png", "assets/FriendUnit/Big/TIER2/fbig_t2_atk.png", 240f);
        LoadUnitSprite("Frosted Cupcake", "assets/FriendUnit/Big/TIER3/fbig_t3_idle.png", "assets/FriendUnit/Big/TIER3/fbig_t3_atk.png", 250f);

        // 4. Flying Units (Flour Puff -> Powder Sugar -> Cocoa Powder)
        LoadUnitSprite("Flour Puff", "assets/FriendUnit/Flying/TIER1/fflying_t1_idle.png", "assets/FriendUnit/Flying/TIER1/fflying_t1_atk.png", 140f);
        LoadUnitSprite("Powder Sugar", "assets/FriendUnit/Flying/TIER2/fflying_t2_idle.png", "assets/FriendUnit/Flying/TIER2/fflying_t2_atk.png", 145f);
        LoadUnitSprite("Cocoa Powder", "assets/FriendUnit/Flying/TIER3/fflying_t3_idle.png", "assets/FriendUnit/Flying/TIER3/fflying_t3_atk.png", 150f);
    }

    private static void LoadAllCakes()
    {
        var flavorFolders = new Dictionary<string, string>
        {
            { "ube", "Ube Cake" },
            { "red", "Red Velvet Cake" },
            { "pista", "Pistachio Cake" },
            { "mint", "Mint Cake" },
            { "choco", "Chocolate Cake" },
            { "cheese", "Cheese Cake" },
            { "carrot", "Carrot Cake" }
        };

        string[] positions = { "bottom", "middle", "top" };

        foreach (var flavor in flavorFolders)
        {
            string keyPrefix = flavor.Key;
            string folderName = flavor.Value;

            foreach (string pos in positions)
            {
                for (int tier = 1; tier <= 3; tier++)
                {
                    string filePath = $"assets/Cake/{folderName}/{keyPrefix}{pos}{tier}.png";
                    if (File.Exists(filePath))
                    {
                        string dictKey = $"{keyPrefix}_{pos}_{tier}";
                        CakeTextures[dictKey] = Raylib.LoadTexture(filePath);
                    }
                }
            }
        }
    }

    private static void LoadUnitSprite(string unitName, string idlePath, string attackPath = "", float drawSize = 150f)
    {
        if (!string.IsNullOrEmpty(idlePath) && File.Exists(idlePath))
        {
            IdleTextures[unitName] = Raylib.LoadTexture(idlePath);
            SpriteSizes[unitName] = drawSize;
        }

        if (!string.IsNullOrEmpty(attackPath) && File.Exists(attackPath))
        {
            AttackTextures[unitName] = Raylib.LoadTexture(attackPath);
        }
    }

    public static void UnloadAll()
    {
        if (HasBackground) Raylib.UnloadTexture(BackgroundTex);
        if (HasTable) Raylib.UnloadTexture(TableTex);

        foreach (var tex in IdleTextures.Values) Raylib.UnloadTexture(tex);
        foreach (var tex in AttackTextures.Values) Raylib.UnloadTexture(tex);
        foreach (var tex in CakeTextures.Values) Raylib.UnloadTexture(tex);
    }
}