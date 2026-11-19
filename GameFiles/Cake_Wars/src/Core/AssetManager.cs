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

        // Load Unit Sprites
        LoadUnitSprite("Wet Flour", "assets/TIER1_IDLE.png", "assets/TIER1_ATTACK.png", 230f);
        LoadUnitSprite("Frosted Cupcake", "assets/TIER3_IDLE.png", "assets/TIER3_ATTACK.png", 230f);

        // Load All Cake Flavors & Layers
        LoadAllCakes();
    }

    private static void LoadAllCakes()
    {
        // Keys match the exact filename prefixes in your folders[cite: 14, 15, 16]
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
            string keyPrefix = flavor.Key;     // e.g. "choco", "pista", "red"
            string folderName = flavor.Value;  // e.g. "Chocolate Cake"[cite: 14]

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