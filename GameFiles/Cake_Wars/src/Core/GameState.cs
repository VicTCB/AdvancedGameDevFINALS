using System.Numerics;

namespace CakeWars;

public class FlavorOption
{
    public string Name = "";
    public string BuffDescription = "";
    public string QuestDescription = "";
    public int TargetCount;
    public int CurrentCount;
    public bool Completed;
    public bool IsActiveQuest;
    public Action ApplyBuff = () => { };
}

public class GameState
{
    // Cake & Base Health
    public string[] LayerFlavors = { "ube", "ube", "ube" };
    private int nextFlavorSlot = 0; // Flavors fill the cake from the Bottom layer (0) up to the Top layer (2)

    public void AssignNextFlavor(string flavorKey)
    {
        if (nextFlavorSlot < LayerFlavors.Length)
        {
            LayerFlavors[nextFlavorSlot] = flavorKey;
            nextFlavorSlot++;
        }
    }
    public float CakeHp = 1200f;
    public float MaxCakeHp = 1200f;
    public int CakeLayersUnlocked = 1;
    // GameElapsedTime at which each layer was unlocked (bottom layer exists from the start)
    public float[] LayerUnlockTimes = { 0f, -1f, -1f };
    public int CakeTier = 1;
    public float CakeExp = 0f;
    public float[] ExpMilestones = { 0f, 1800f, 5400f };

    public int EnemyTier => GameElapsedTime >= GameConfig.EnemyTier3Time ? 3
                          : (GameElapsedTime >= GameConfig.EnemyTier2Time ? 2 : 1);

    public float WindowSealed = 0f;
    public float MaxWindowSealed = 100f;

    // Economy & Modifiers
    public float Crumbs = 200f;
    public float CrumbRate = 7f;
    public float AllyDamageMultiplier = 1.0f;
    public float AllyDefenseMultiplier = 1.0f;

    // Ultimates
    public float UltimateCooldownMultiplier = 1.0f; // Ube buff lowers this
    public float UltimateTimer = 0f;

    public float GetUltimateCooldown() => GameConfig.UltimateBaseCooldown(CakeTier) * UltimateCooldownMultiplier;
    public bool UltimateTargetingActive = false;
    public float DampClothSweepX = -1f;

    // State Tracking
    public GamePhase CurrentPhase = GamePhase.Playing;
    public BottomTab ActiveTab = BottomTab.Recipes;
    public float EnemySpawnTimer = 3.5f;
    public float GameElapsedTime = 0f;

    // Collections
    public List<Unit> Units = new();
    public List<Projectile> Projectiles = new();
    public List<GasCloud> ActiveClouds = new();
    public TurretSlot[] CakeTurrets = new TurretSlot[3];
    public Queue<BakingOrder> OvenQueue = new();

    // Roguelike Flavor Quests
    public List<FlavorOption> ActiveQuests = new();
    public List<FlavorOption> SelectedChoices = new();
    public List<FlavorOption> FlavorPool = new();
    public int FlavorBuffsOffered = 0;
    public const float FlavorBuffDelay = 30f;
    // Safety net: minimum gap after choosing a buff before the next selection can appear
    public const float FlavorBuffCooldownAfterPick = 2f;
    public float FlavorBuffCooldown = 0f;

    // Checks whether the next flavor buff selection should be offered.
    // 1st: 30s into the game.
    // 2nd: 2nd layer has existed for 30s AND the cake has leveled up (tier 2+).
    // 3rd: 3rd layer has existed for 30s AND the cake is max level (tier 3).
    public bool IsNextFlavorBuffReady()
    {
        if (FlavorBuffCooldown > 0f) return false;

        switch (FlavorBuffsOffered)
        {
            case 0:
                return GameElapsedTime >= FlavorBuffDelay;
            case 1:
                return LayerExistedFor(1, FlavorBuffDelay) && CakeTier >= 2;
            case 2:
                return LayerExistedFor(2, FlavorBuffDelay) && CakeTier >= 3;
            default:
                return false;
        }
    }

    private bool LayerExistedFor(int layerIndex, float seconds)
    {
        return CakeLayersUnlocked > layerIndex
            && LayerUnlockTimes[layerIndex] >= 0f
            && GameElapsedTime - LayerUnlockTimes[layerIndex] >= seconds;
    }

    public GameState()
    {
        CakeTurrets[0] = new TurretSlot { RelativeOffset = new Vector2(95, -65), Unlocked = true };
        CakeTurrets[1] = new TurretSlot { RelativeOffset = new Vector2(95, -165), Unlocked = false };
        CakeTurrets[2] = new TurretSlot { RelativeOffset = new Vector2(95, -255), Unlocked = false };

        InitFlavorPool();
    }

    private void InitFlavorPool()
    {
        FlavorPool = new List<FlavorOption>
        {
            new FlavorOption
            {
                Name = "Chocolate",
                BuffDescription = "+25% Ally Attack Damage",
                QuestDescription = "Kill 8 Bugs",
                TargetCount = 8,
                ApplyBuff = () => { AllyDamageMultiplier += 0.25f; AssignNextFlavor("choco"); }
            },
            new FlavorOption
            {
                Name = "Pistachio",
                BuffDescription = "+30% Ally Max Health",
                QuestDescription = "Kill 10 Bugs",
                TargetCount = 10,
                ApplyBuff = () => { AllyDefenseMultiplier += 0.30f; AssignNextFlavor("pista"); }
            },
            new FlavorOption
            {
                Name = "Mint",
                BuffDescription = "+10 Passive Crumbs / sec",
                QuestDescription = "Kill 6 Bugs",
                TargetCount = 6,
                ApplyBuff = () => { CrumbRate += 10f; AssignNextFlavor("mint"); }
            },
            new FlavorOption
            {
                Name = "Ube",
                BuffDescription = "Ultimate cooldown cut by 40%",
                QuestDescription = "Kill 12 Bugs",
                TargetCount = 12,
                ApplyBuff = () => { UltimateCooldownMultiplier *= 0.6f; AssignNextFlavor("ube"); }
            },
            new FlavorOption
            {
                Name = "Red Velvet",
                BuffDescription = "+35% Ally Attack Damage",
                QuestDescription = "Kill 10 Bugs",
                TargetCount = 10,
                ApplyBuff = () => { AllyDamageMultiplier += 0.35f; AssignNextFlavor("red"); }
            },
            new FlavorOption
            {
                Name = "Cheese",
                BuffDescription = "+400 Max Cake HP & Heal",
                QuestDescription = "Kill 8 Bugs",
                TargetCount = 8,
                ApplyBuff = () => { MaxCakeHp += 400f; CakeHp = Math.Min(MaxCakeHp, CakeHp + 400f); AssignNextFlavor("cheese"); }
            },
            new FlavorOption
            {
                Name = "Carrot",
                BuffDescription = "+15 Passive Crumbs / sec",
                QuestDescription = "Kill 9 Bugs",
                TargetCount = 9,
                ApplyBuff = () => { CrumbRate += 15f; AssignNextFlavor("carrot"); }
            }
        };
    }
}