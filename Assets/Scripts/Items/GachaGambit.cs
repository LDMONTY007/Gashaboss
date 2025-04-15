using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "GachaGambit", menuName = "Items/Gacha Gambit")]
public class GachaGambit : ItemData
{
    [Header("Gambit Settings")]
    [Tooltip("Multiplier for coin rewards from bosses")]
    public float coinRewardMultiplier = 2.0f;

    [Tooltip("Multiplier for incoming damage while effect is active")]
    public float damageMultiplier = 1.5f;

    [Tooltip("Visual effect prefab for the gambit aura")]
    public GameObject gambitVisualPrefab;

    private static bool isApplied = false;

    public override void OnPickup()
    {
        Debug.Log("Gacha Gambit picked up! High risk, high reward mode activated.");

        if (!isApplied)
        {
            GachaGambitEffect effect = Player.instance.gameObject.AddComponent<GachaGambitEffect>();
            effect.Initialize(this);
            isApplied = true;
        }
    }

    public override void RemoveItem()
    {
        GachaGambitEffect effect = Player.instance.GetComponent<GachaGambitEffect>();
        if (effect != null)
        {
            Destroy(effect);
        }

        isApplied = false;
        Player.instance.inventory.Remove(this);

        Debug.Log("Gacha Gambit removed from inventory.");
    }

    public override void ApplyEffect()
    {
        // Passive effect, handled by GachaGambitEffect
    }
}

// Effect component in the same file
public class GachaGambitEffect : MonoBehaviour
{
    private GachaGambit itemData;
    private GameObject gambitVisual;

    public void Initialize(GachaGambit data)
    {
        itemData = data;

        // Create visual effect
        if (itemData.gambitVisualPrefab != null)
        {
            gambitVisual = Instantiate(itemData.gambitVisualPrefab, transform);
        }
        else
        {
            CreateDefaultVisual();
        }

        // Modify all bosses in the scene
        UpdateAllBossesInScene();

        Debug.Log($"Gacha Gambit initialized! Bosses will drop {itemData.coinRewardMultiplier}x coins but deal {itemData.damageMultiplier}x damage.");
    }

    private void CreateDefaultVisual()
    {
        // Create gold particle effect
        GameObject visualObj = new GameObject("GambitVisual");
        visualObj.transform.parent = transform;
        visualObj.transform.localPosition = Vector3.zero;

        ParticleSystem particles = visualObj.AddComponent<ParticleSystem>();
        gambitVisual = visualObj;

        // Gold particle effect
        var main = particles.main;
        main.startColor = new Color(1f, 0.84f, 0f, 0.7f);
        main.startSize = 0.1f;
        main.startSpeed = 2f;
        main.startLifetime = 1f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = particles.emission;
        emission.rateOverTime = 10;

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f;

        particles.Play();
    }

    private void UpdateAllBossesInScene()
    {
        BossController[] bosses = FindObjectsByType<BossController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (BossController boss in bosses)
        {
            if (!boss.isDead)
            {
                int originalCoins = boss.coinsRewarded;
                boss.coinsRewarded = Mathf.RoundToInt(originalCoins * itemData.coinRewardMultiplier);
                Debug.Log($"Modified boss {boss.bossName}: Coins {originalCoins} -> {boss.coinsRewarded}");
            }
        }
    }

    // Method to modify damage player takes from bosses
    public float ModifyIncomingDamage(int damage, GameObject source)
    {
        if (source.GetComponent<BossController>() != null)
        {
            // Increase damage from bosses
            return damage * itemData.damageMultiplier;
        }

        // Return unmodified damage for non-boss sources
        return damage;
    }

    // Call this when a new boss is spawned
    public void OnBossSpawned(BossController boss)
    {
        if (boss != null && !boss.isDead)
        {
            boss.coinsRewarded = Mathf.RoundToInt(boss.coinsRewarded * itemData.coinRewardMultiplier);
            Debug.Log($"Modified newly spawned boss {boss.bossName}");
        }
    }

    private void OnDisable()
    {
        // Clean up visual effect
        if (gambitVisual != null)
        {
            Destroy(gambitVisual);
        }

        // Restore original coin values on bosses
        BossController[] bosses = FindObjectsByType<BossController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (BossController boss in bosses)
        {
            if (!boss.isDead)
            {
                boss.coinsRewarded = Mathf.RoundToInt(boss.coinsRewarded / itemData.coinRewardMultiplier);
                Debug.Log($"Restored boss {boss.bossName} coins to original values");
            }
        }

        Debug.Log("Gacha Gambit effect removed.");
    }
}