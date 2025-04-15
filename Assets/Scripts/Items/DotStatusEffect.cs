using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DotStatusEffect", menuName = "Items/DOT Status Effect")]
public class DotStatusEffect : ItemData
{
    [Header("DOT Settings")]
    [Tooltip("Damage per tick")]
    public int dotDamage = 1;

    [Tooltip("Seconds between damage ticks")]
    public float tickInterval = 3f;

    [Tooltip("Total duration of DOT effect")]
    public float dotDuration = 10f;

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

    // Track which bosses are affected by DOT
    private Dictionary<BossController, DotInfo> affectedBosses = new Dictionary<BossController, DotInfo>();

    private class DotInfo
    {
        public Coroutine dotCoroutine;
        public GameObject dotVisual;
        public float remainingDuration;
    }

    public void Initialize(DotStatusEffect data)
    {
        itemData = data;

        // Subscribe to weapon attacks
        playerWeapon = Player.instance.curWeapon;
        if (playerWeapon != null)
        {
            playerWeapon.onAttack += OnPlayerAttack;
        }

        Debug.Log("DOT Status Effect initialized!");
    }

    private void Update()
    {
        // Handle weapon switching
        if (Player.instance.curWeapon != playerWeapon)
        {
            if (playerWeapon != null)
            {
                playerWeapon.onAttack -= OnPlayerAttack;
            }

            playerWeapon = Player.instance.curWeapon;
            if (playerWeapon != null)
            {
                playerWeapon.onAttack += OnPlayerAttack;
            }
        }
    }

    // Called when player attacks
    private void OnPlayerAttack()
    {
        // Find bosses in weapon range
        Collider[] colliders = Physics.OverlapSphere(
            playerWeapon.transform.position,
            playerWeapon.attackDistance,
            ~LayerMask.GetMask("Player", "Ignore Raycast")
        );

        foreach (Collider collider in colliders)
        {
            BossController boss = collider.GetComponent<BossController>();
            if (boss != null && !boss.isDead)
            {
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
            Debug.Log($"DOT refreshed on {boss.bossName}");
        }
        else
        {
            // Apply new DOT
            DotInfo newDotInfo = new DotInfo
            {
                remainingDuration = itemData.dotDuration,
                dotCoroutine = StartCoroutine(ApplyDotDamage(boss))
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
            Debug.Log($"DOT applied to {boss.bossName}");
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

    private IEnumerator ApplyDotDamage(BossController boss)
    {
        DotInfo dotInfo = affectedBosses[boss];

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
                boss.TakeDamage(itemData.dotDamage, gameObject);
                Debug.Log($"DOT dealt {itemData.dotDamage} damage to {boss.bossName}");

                // Flash effect
                if (dotInfo.dotVisual != null)
                {
                    ParticleSystem particles = dotInfo.dotVisual.GetComponent<ParticleSystem>();
                    if (particles != null)
                    {
                        var emission = particles.emission;
                        float originalRate = emission.rateOverTime.constant;
                        emission.rateOverTime = originalRate * 3;

                        StartCoroutine(ResetEmissionRate(particles, originalRate));
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

            affectedBosses.Remove(boss);
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from events
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