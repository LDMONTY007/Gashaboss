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

    [Tooltip("Total duration of DOT effect")]
    public float dotDuration = 12f;

    [Tooltip("Visual effect prefab for the DOT")]
    public GameObject dotVisualPrefab;

    private static bool isApplied = false;

    public override void OnPickup()
    {
        Debug.Log("DOT Status Effect item picked up! Your attacks now apply damage over time.");

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
    private Weapon playerWeapon;
    private bool debugApplyToNearestBoss = false;

    // Track which bosses are affected by DOT
    private Dictionary<BossController, DotInfo> affectedBosses = new Dictionary<BossController, DotInfo>();

    private class DotInfo
    {
        public IEnumerator dotCoroutine;
        public GameObject dotVisual;
        public float remainingDuration;
        public GameObject dotDamageSource;
    }

    public void Initialize(DotStatusEffect data)
    {
        itemData = data;

        // Subscribe to ALL player attacks like in CoordinatedAttackItem
        Player.instance.OnAttack += OnPlayerAttack;
        Player.instance.OnAltAttack += OnPlayerAttack;
        Player.instance.OnSpecialAttack += OnPlayerAttack;

        // // Keep the weapon subscription for backward compatibility
        // playerWeapon = Player.instance.curWeapon;
        // if (playerWeapon != null)
        // {
        //     playerWeapon.onAttack += OnPlayerAttack;
        // }

        Debug.Log("DOT Status Effect initialized! Now listening to ALL attack types.");

        // Force apply on initialization for testing
        // ForceApplyToAllBosses();
    }

    private void Update()
    {
        // // Handle weapon switching
        // if (Player.instance.curWeapon != playerWeapon)
        // {
        //     if (playerWeapon != null)
        //     {
        //         playerWeapon.onAttack -= OnPlayerAttack;
        //     }

        //     playerWeapon = Player.instance.curWeapon;
        //     if (playerWeapon != null)
        //     {
        //         playerWeapon.onAttack += OnPlayerAttack;
        //     }
        // }

        // Testing: Apply DOT to nearest boss when flag is set
        if (debugApplyToNearestBoss)
        {
            debugApplyToNearestBoss = false;  // Reset flag
            BossController boss = FindNearestBoss();
            if (boss != null)
            {
                Debug.Log($"DEBUG: Forcing DOT application on nearest boss: {boss.name}");
                ApplyDotToBoss(boss);
            }
        }
    }

    // Called when player attacks
    private void OnPlayerAttack()
    {
        Debug.Log("DOT Status Effect: Attack detected!");

        // Find nearest boss directly like CoordinatedAttackEffect does
        BossController boss = FindNearestBoss();

        if (boss != null)
        {
            Debug.Log($"DOT Status Effect: Found boss '{boss.name}', applying DOT effect!");
            ApplyDotToBoss(boss);
        }
        else
        {
            Debug.Log("DOT Status Effect: No valid boss targets found.");
        }
    }

    private BossController FindNearestBoss()
    {
        BossController[] bosses = Object.FindObjectsByType<BossController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        BossController nearest = null;
        float nearestDist = float.MaxValue;

        foreach (BossController boss in bosses)
        {
            if (boss == null || boss.isDead)
                continue;

            float dist = Vector3.Distance(Player.instance.transform.position, boss.transform.position);
            if (dist < nearestDist)
            {
                nearest = boss;
                nearestDist = dist;
            }
        }

        if (nearest != null)
            Debug.Log($"Found nearest boss: {nearest.name} at distance {nearestDist}");

        return nearest;
    }

    public void ForceApplyToAllBosses()
    {
        Debug.Log("DOT Status Effect: Force applying to all bosses");
        BossController[] bosses = Object.FindObjectsByType<BossController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        Debug.Log($"DOT Status Effect: Found {bosses.Length} bosses in scene");
        foreach (BossController boss in bosses)
        {
            if (boss != null && !boss.isDead)
            {
                Debug.Log($"DOT Status Effect: Force applying to boss {boss.name}");
                ApplyDotToBoss(boss);
            }
        }
    }

    private void ApplyDotToBoss(BossController boss)
    {
        // Check if boss is already affected
        if (affectedBosses.TryGetValue(boss, out DotInfo dotInfo))
        {
            // Refresh DOT duration
            dotInfo.remainingDuration = itemData.dotDuration;
            Debug.Log($"DOT refreshed on {boss.name}");
        }
        else
        {
            // Create a dedicated damage source
            GameObject dotDamageSource = new GameObject("DotDamageSource");
            dotDamageSource.transform.position = boss.transform.position;

            // Apply new DOT
            DotInfo newDotInfo = new DotInfo
            {
                remainingDuration = itemData.dotDuration,
                dotDamageSource = dotDamageSource,
                dotCoroutine = ApplyDotDamage(boss, dotDamageSource)
            };

            // Create visual effect
            if (itemData.dotVisualPrefab != null)
            {
                newDotInfo.dotVisual = Instantiate(
                    itemData.dotVisualPrefab,
                    boss.transform.position,
                    Quaternion.identity,
                    boss.transform
                );
            }
            else
            {
                newDotInfo.dotVisual = CreateDotVisual(boss);
            }

            affectedBosses.Add(boss, newDotInfo);
            StartCoroutine(newDotInfo.dotCoroutine);
            Debug.Log($"DOT applied to {boss.name}");
        }
    }

    private GameObject CreateDotVisual(BossController boss)
    {
        // Create a green poison visual effect
        GameObject visualObj = new GameObject("DOTVisual");
        visualObj.transform.parent = boss.transform;
        visualObj.transform.localPosition = Vector3.zero;

        ParticleSystem particles = visualObj.AddComponent<ParticleSystem>();

        // Green particle effect for poison
        var main = particles.main;
        main.startColor = new Color(0.4f, 0.8f, 0.2f, 0.7f);
        main.startSize = 0.2f;
        main.startSpeed = 1f;
        main.startLifetime = 1f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = particles.emission;
        emission.rateOverTime = 15;

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        particles.Play();

        return visualObj;
    }

    private IEnumerator ApplyDotDamage(BossController boss, GameObject dotDamageSource)
    {
        DotInfo dotInfo = affectedBosses[boss];
        Debug.Log($"DOT Effect: Starting DOT on {boss.name} for {dotInfo.remainingDuration} seconds");

        // Apply damage every interval until duration expires
        while (dotInfo.remainingDuration > 0 && boss != null && !boss.isDead)
        {
            // Wait for tick interval
            yield return new WaitForSeconds(itemData.tickInterval);

            // Reduce remaining duration
            dotInfo.remainingDuration -= itemData.tickInterval;

            // Apply damage if boss still exists
            if (boss != null && !boss.isDead)
            {
                // Apply damage using the dedicated damage source - THIS IS KEY
                yield return ApplyDotDamageTick(boss, itemData.dotDamage, dotDamageSource);

                // Flash effect
                if (dotInfo.dotVisual != null)
                {
                    ParticleSystem particles = dotInfo.dotVisual.GetComponent<ParticleSystem>();
                    if (particles != null)
                    {
                        var emission = particles.emission;
                        float originalRate = emission.rateOverTime.constant;
                        emission.rateOverTime = originalRate * 3;

                        yield return ResetEmissionRate(particles, originalRate);
                    }
                }
            }
            else
            {
                CleanupDotEffect(boss);
                yield break;
            }
        }

        // DOT expired, clean up
        CleanupDotEffect(boss);
    }

    private IEnumerator ApplyDotDamageTick(BossController boss, int damage, GameObject damageSource)
    {
        if (boss == null || boss.isDead)
            yield break;

        // Deal damage using the same approach as CompanionController
        IDamageable damageable = boss.GetComponent<IDamageable>();
        if (damageable != null)
        {
            Debug.Log($"DOT dealing {damage} damage to {boss.name} using {damageSource.name}");
            damageable.TakeDamage(damage, damageSource);
        }
    }

    private IEnumerator ResetEmissionRate(ParticleSystem particles, float originalRate)
    {
        yield return new WaitForSeconds(0.2f);

        if (particles != null)
        {
            var emission = particles.emission;
            emission.rateOverTime = originalRate;
        }
    }

    private void CleanupDotEffect(BossController boss)
    {
        if (affectedBosses.TryGetValue(boss, out DotInfo dotInfo))
        {
            // Clean up coroutine and visual
            if (dotInfo.dotCoroutine != null)
            {
                StopCoroutine(dotInfo.dotCoroutine);
            }

            if (dotInfo.dotVisual != null)
            {
                Destroy(dotInfo.dotVisual);
            }

            // Destroy the damage source
            if (dotInfo.dotDamageSource != null)
            {
                Destroy(dotInfo.dotDamageSource);
            }

            affectedBosses.Remove(boss);
            Debug.Log($"Cleaned up DOT effect for {boss.name}");
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from all events
        if (Player.instance != null)
        {
            Player.instance.OnAttack -= OnPlayerAttack;
            Player.instance.OnAltAttack -= OnPlayerAttack;
            Player.instance.OnSpecialAttack -= OnPlayerAttack;
        }

        // Also unsubscribe from weapon events for backward compatibility
        if (playerWeapon != null)
        {
            playerWeapon.onAttack -= OnPlayerAttack;
        }

        // Clean up all active DOTs
        foreach (KeyValuePair<BossController, DotInfo> pair in new Dictionary<BossController, DotInfo>(affectedBosses))
        {
            CleanupDotEffect(pair.Key);
        }

        StopAllCoroutines();

        Debug.Log("DOT Status Effect removed.");
    }
}