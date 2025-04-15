using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "CritFishing", menuName = "Items/Crit Fishing")]
public class CritFishing : ItemData
{
    [Header("Critical Hit Settings")]
    [Tooltip("Base critical hit chance (0.1 = 10%)")]
    public float baseCritChance = 0.0f;

    [Tooltip("Critical hit chance added per item (0.1 = 10%)")]
    public float critChancePerItem = 0.1f;

    [Tooltip("Base critical damage multiplier")]
    public float baseCritDamage = 1.5f;

    [Tooltip("Crit damage increase per item")]
    public float critDamageIncreasePerItem = 0.1f;

    [Tooltip("Maximum crit damage multiplier")]
    public float maxCritDamage = 2.5f;

    [Tooltip("Visual effect prefab for critical hits")]
    public GameObject critVisualPrefab;

    // Static field directly in the main class
    private static int itemCount = 0;

    public override void OnPickup()
    {
        Debug.Log("Crit Fishing item picked up! Your attacks now have a chance to critically hit.");

        // Increase item count
        itemCount++;

        // Add the effect component if first pickup, or update if already exists
        CritFishingEffect effect = Player.instance.GetComponent<CritFishingEffect>();
        if (effect == null)
        {
            effect = Player.instance.gameObject.AddComponent<CritFishingEffect>();
            effect.Initialize(this);
        }
        else
        {
            effect.UpdateStats(this);
        }
    }

    public override void RemoveItem()
    {
        // Decrease item count
        itemCount--;

        // Update or remove the effect component
        CritFishingEffect effect = Player.instance.GetComponent<CritFishingEffect>();
        if (effect != null)
        {
            if (itemCount <= 0)
            {
                Destroy(effect);
                itemCount = 0; // Safeguard
            }
            else
            {
                effect.UpdateStats(this);
            }
        }

        // Remove from player's inventory
        Player.instance.inventory.Remove(this);

        Debug.Log("Crit Fishing item removed. Remaining count: " + itemCount);
    }

    public override void ApplyEffect()
    {
        // Effect is applied when picked up and on attacks
    }

    // Public static method to access the item count
    public static int GetItemCount()
    {
        return itemCount;
    }
}

// Effect controller component
public class CritFishingEffect : MonoBehaviour
{
    private CritFishing itemData;
    private Weapon playerWeapon;

    // Current critical hit stats
    private float currentCritChance;
    private float currentCritDamage;

    public void Initialize(CritFishing data)
    {
        itemData = data;

        // Calculate current stats
        UpdateStats(data);

        // Subscribe to weapon attacks
        playerWeapon = Player.instance.curWeapon;
        if (playerWeapon != null)
        {
            playerWeapon.onAttack += OnPlayerAttack;
        }

        Debug.Log($"Crit Fishing initialized! Chance: {currentCritChance * 100}%, Damage: {currentCritDamage}x");
    }

    public void UpdateStats(CritFishing data)
    {
        itemData = data;

        // Calculate stats based on item count
        int count = CritFishing.GetItemCount(); // Use the public method

        currentCritChance = itemData.baseCritChance + (itemData.critChancePerItem * count);
        currentCritDamage = Mathf.Min(
            itemData.maxCritDamage,
            itemData.baseCritDamage + (itemData.critDamageIncreasePerItem * count)
        );

        Debug.Log($"Crit Fishing updated! Chance: {currentCritChance * 100}%, Damage: {currentCritDamage}x");
    }

    private void Update()
    {
        // Check if weapon has changed
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
        // Roll for critical hit
        if (Random.value <= currentCritChance)
        {
            // Critical hit! Find what the player hit
            StartCoroutine(ApplyCriticalHit());
        }
    }

    private IEnumerator ApplyCriticalHit()
    {
        // Wait a tiny bit for the normal attack to process
        yield return null;

        // Get all damageables in range
        Collider[] colliders = Physics.OverlapSphere(
            playerWeapon.transform.position,
            playerWeapon.attackDistance,
            ~LayerMask.GetMask("Player", "Ignore Raycast")
        );

        foreach (Collider collider in colliders)
        {
            // Look for boss or other damageable
            IDamageable damageable = collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // Apply critical damage - ensure at least 1 extra damage for crits
                int criticalDamage = Mathf.Max(1, Mathf.RoundToInt(currentCritDamage - 1f));
                damageable.TakeDamage(criticalDamage, gameObject);

                // Play critical hit effect
                PlayCriticalEffect(collider.transform.position);

                Debug.Log($"Critical hit! Applied {criticalDamage} bonus damage ({currentCritDamage}x total)");
            }
        }
    }

    private void PlayCriticalEffect(Vector3 position)
    {
        // Create critical hit visual
        if (itemData.critVisualPrefab != null)
        {
            Instantiate(itemData.critVisualPrefab, position, Quaternion.identity);
        }
        else
        {
            // Create default crit effect
            GameObject critEffect = new GameObject("CritEffect");
            critEffect.transform.position = position;

            // Add particle system
            ParticleSystem particles = critEffect.AddComponent<ParticleSystem>();

            // Critical hit effect (red sparks)
            var main = particles.main;
            main.startColor = new Color(1f, 0.3f, 0.3f, 0.8f); // Red
            main.startSize = 0.3f;
            main.startSpeed = 3f;
            main.startLifetime = 0.5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = particles.emission;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 20)
            });

            // Auto-destroy after effect finishes
            Destroy(critEffect, 2f);

            // Play the effect
            particles.Play();
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from weapon events
        if (playerWeapon != null)
        {
            playerWeapon.onAttack -= OnPlayerAttack;
        }

        Debug.Log("Crit Fishing effect removed.");
    }
}