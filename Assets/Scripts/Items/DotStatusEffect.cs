using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DotStatusEffect", menuName = "Items/DOT Status Effect")]
public class DotStatusEffect : ItemData
{
    [Header("DOT Settings")]
    [Tooltip("Damage per tick")]
    public int dotDamage = 25;

    [Tooltip("Seconds between damage ticks")]
    public float tickInterval = 3f;

    [Tooltip("AOE radius around player")]
    public float aoeRadius = 30f;

    private static bool isApplied = false;

    public override void OnPickup()
    {
        Debug.Log("DOT Status Effect item picked up! Your presence now causes damage over time to nearby bosses.");

        if (!isApplied)
        {
            DotStatusController effect = Player.instance.gameObject.AddComponent<DotStatusController>();
            effect.Initialize(this);
            isApplied = true;
        }
    }

    public override void RemoveItem()
    {
        DotStatusController effect = Player.instance.GetComponent<DotStatusController>();
        if (effect != null)
        {
            Destroy(effect);
        }

        isApplied = false;
        Player.instance.inventory.Remove(this);

        Debug.Log("DOT Status Effect removed from inventory.");
    }

    public override void ApplyEffect()
    {
        // Passive effect, handled by the controller
    }
}

// Effect controller in the same file
public class DotStatusController : MonoBehaviour
{
    private DotStatusEffect itemData;

    // Player AOE visual effect
    private GameObject playerAoeVisual;

    // Damage application coroutine
    private Coroutine damageCoroutine;

    public void Initialize(DotStatusEffect data)
    {
        itemData = data;

        // Force the radius to be what's set in the script
        float scriptRadius = 50f; // Match this to what you have in your script
        if (itemData.aoeRadius != scriptRadius)
        {
            Debug.LogWarning($"AOE radius was {itemData.aoeRadius}, forcing to {scriptRadius}");
            itemData.aoeRadius = scriptRadius;
        }

        // Create AOE visual around player
        CreatePlayerAoeVisual();

        // Start the damage application coroutine
        damageCoroutine = StartCoroutine(ApplyDamageToAllInRange());

        Debug.Log($"DOT Status Effect initialized! Continuous AOE damage enabled with radius {itemData.aoeRadius}.");
    }

    private void CreatePlayerAoeVisual()
    {
        if (playerAoeVisual != null)
            return;

        // Create a poison aura visual effect around player - JUST the particles
        playerAoeVisual = new GameObject("PlayerDOTAura");
        playerAoeVisual.transform.parent = transform;
        playerAoeVisual.transform.localPosition = Vector3.zero;

        // Add particle system
        ParticleSystem particles = playerAoeVisual.AddComponent<ParticleSystem>();

        // Green particle effect for poison
        var main = particles.main;
        main.startColor = new Color(0.4f, 0.8f, 0.2f, 0.4f);
        main.startSize = 0.3f;
        main.startSpeed = 2f;
        main.startLifetime = 1.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = particles.emission;
        emission.rateOverTime = 20;

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = itemData.aoeRadius;

        // Make particles emit outward
        var velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.speedModifier = 0.5f;

        particles.Play();
    }

    // Simple coroutine that regularly checks for all bosses in range and damages them
    private IEnumerator ApplyDamageToAllInRange()
    {
        Debug.Log("Starting persistent DOT effect on all bosses in range");

        // Dictionary to track bosses with visual effects
        Dictionary<BossController, GameObject> bossVisuals = new Dictionary<BossController, GameObject>();

        while (true)
        {
            // Wait for the tick interval
            yield return new WaitForSeconds(itemData.tickInterval);

            // Find ALL bosses in the scene every time
            BossController[] allBosses = Object.FindObjectsByType<BossController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            // Process all bosses - applying damage to those in range, cleaning up those out of range
            foreach (BossController boss in allBosses)
            {
                if (boss == null || boss.isDead)
                    continue;

                float distance = Vector3.Distance(transform.position, boss.transform.position);

                if (distance <= itemData.aoeRadius)
                {
                    // Boss is in range - apply damage
                    int previousHealth = boss.curHealth;
                    boss.curHealth -= itemData.dotDamage;
                    int actualDamage = previousHealth - boss.curHealth;

                    Debug.Log($"DOT dealt {actualDamage} damage to {boss.name}, health now: {boss.curHealth}");

                    // Create or update visual effect
                    if (!bossVisuals.ContainsKey(boss))
                    {
                        bossVisuals[boss] = CreateDotVisual(boss);
                    }

                    // Flash effect
                    StartCoroutine(FlashEffect(bossVisuals[boss]));
                }
                else
                {
                    // Boss out of range - remove visual if exists
                    if (bossVisuals.ContainsKey(boss))
                    {
                        if (bossVisuals[boss] != null)
                        {
                            Destroy(bossVisuals[boss]);
                        }
                        bossVisuals.Remove(boss);
                    }
                }
            }

            // Clean up any visuals for bosses that no longer exist
            List<BossController> bossesToRemove = new List<BossController>();
            foreach (var kvp in bossVisuals)
            {
                BossController boss = kvp.Key;
                if (boss == null || boss.isDead)
                {
                    if (kvp.Value != null)
                    {
                        Destroy(kvp.Value);
                    }
                    bossesToRemove.Add(boss);
                }
            }

            // Remove any missing bosses from the dictionary
            foreach (BossController boss in bossesToRemove)
            {
                bossVisuals.Remove(boss);
            }
        }
    }

    private GameObject CreateDotVisual(BossController boss)
    {
        // Check if boss is valid
        if (boss == null)
            return null;

        // Create a green poison visual effect
        GameObject visualObj = new GameObject("DOTVisual");
        visualObj.transform.parent = boss.transform;
        visualObj.transform.localPosition = Vector3.zero;

        ParticleSystem particles = visualObj.AddComponent<ParticleSystem>();

        // Green particle effect for poison
        var main = particles.main;
        main.startColor = new Color(0.4f, 0.8f, 0.2f, 0.7f);
        main.startSize = 0.5f; // Larger particles
        main.startSpeed = 2f;  // Faster particles
        main.startLifetime = 1f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = particles.emission;
        emission.rateOverTime = 30; // More particles

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 1.0f; // Larger radius

        particles.Play();

        return visualObj;
    }

    private IEnumerator FlashEffect(GameObject visual)
    {
        if (visual == null)
            yield break;

        ParticleSystem particles = visual.GetComponent<ParticleSystem>();
        if (particles != null)
        {
            // Get original emission rate
            var emission = particles.emission;
            float originalRate = emission.rateOverTime.constant;

            // Increase emission temporarily
            emission.rateOverTime = originalRate * 3;

            // Wait a moment
            yield return new WaitForSeconds(0.2f);

            // Restore original rate if particles still exist
            if (particles != null)
            {
                emission = particles.emission;  // Get emission again in case it changed
                emission.rateOverTime = originalRate;
            }
        }
    }

    private void Update()
    {
        // Update the position of the AOE visual
        if (playerAoeVisual != null)
        {
            playerAoeVisual.transform.position = transform.position;
        }
    }

    private void OnDisable()
    {
        // Stop damage coroutine
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }

        // Destroy player AOE visual
        if (playerAoeVisual != null)
        {
            Destroy(playerAoeVisual);
            playerAoeVisual = null;
        }

        Debug.Log("DOT Status Effect removed.");
    }
}