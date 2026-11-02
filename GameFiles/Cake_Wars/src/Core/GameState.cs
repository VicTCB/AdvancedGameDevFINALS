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
    public float CakeHp = 1200f;
    public float MaxCakeHp = 1200f;
    public int CakeLayersUnlocked = 1;
    public int CakeTier = 1;
    public float CakeExp = 0f;
    public float[] ExpMilestones = { 0f, 180f, 480f };

    public float WindowSealed = 0f;
    public float MaxWindowSealed = 100f;

    // Economy & Modifiers
    public float Crumbs = 150f;
    public float CrumbRate = 12f;
    public float AllyDamageMultiplier = 1.0f;
    public float AllyDefenseMultiplier = 1.0f;

    // Ultimates
    public float UltimateCooldownMax = 15f;
    public float UltimateTimer = 0f;
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

    public GameState()
    {
        CakeTurrets[0] = new TurretSlot { RelativeOffset = new Vector2(70, -65), Unlocked = true };
        CakeTurrets[1] = new TurretSlot { RelativeOffset = new Vector2(60, -125), Unlocked = false };
        CakeTurrets[2] = new TurretSlot { RelativeOffset = new Vector2(50, -185), Unlocked = false };

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
                ApplyBuff = () => { AllyDamageMultiplier += 0.25f; }
            },
            new FlavorOption
            {
                Name = "Pistachio",
                BuffDescription = "+30% Ally Max Health",
                QuestDescription = "Kill 10 Bugs",
                TargetCount = 10,
                ApplyBuff = () => { AllyDefenseMultiplier += 0.30f; }
            },
            new FlavorOption
            {
                Name = "Mint",
                BuffDescription = "+10 Passive Crumbs / sec",
                QuestDescription = "Kill 6 Bugs",
                TargetCount = 6,
                ApplyBuff = () => { CrumbRate += 10f; }
            },
            new FlavorOption
            {
                Name = "Ube",
                BuffDescription = "Ultimate cooldown cut by 40%",
                QuestDescription = "Kill 12 Bugs",
                TargetCount = 12,
                ApplyBuff = () => { UltimateCooldownMax *= 0.6f; }
            }
        };
    }
}