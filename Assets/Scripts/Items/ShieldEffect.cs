using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "ShieldEffect", menuName = "Items/Shield Effect")]
public class ShieldEffect : ItemData
{
    [Header("Shield Settings")]
    [Tooltip("Number of hits the shield can absorb")]
    public int shieldHealth = 3;

    [Tooltip("Visual effect prefab for the shield")]
    public GameObject shieldVisualPrefab;

    private static bool isApplied = false;

    public override void OnPickup()
    {
        Debug.Log("Shield Effect item picked up! You now have protection against damage.");

        if (!isApplied)
        {
            ShieldEffectComponent effect = Player.instance.gameObject.AddComponent<ShieldEffectComponent>();
            effect.Initialize(this);
            isApplied = true;
        }
    }

    public override void RemoveItem()
    {
        ShieldEffectComponent effect = Player.instance.GetComponent<ShieldEffectComponent>();
        if (effect != null)
        {
            Destroy(effect);
        }

        isApplied = false;
        Player.instance.inventory.Remove(this);

        Debug.Log("Shield Effect removed from inventory.");
    }

    public override void ApplyEffect()
    {
        // Passive effect, handled by ShieldEffectComponent
    }
}

// Combining this into the same file for efficiency
public class ShieldEffectComponent : MonoBehaviour
{
    private ShieldEffect itemData;
    private int currentShieldHealth;
    private GameObject shieldVisual;

    public void Initialize(ShieldEffect data)
    {
        itemData = data;
        currentShieldHealth = itemData.shieldHealth;

        CreateShieldVisual();
        Player.instance.onPlayerHit += OnPlayerHit;

        Debug.Log($"Shield initialized with {currentShieldHealth} hit points!");
    }

    private void CreateShieldVisual()
    {
        if (itemData.shieldVisualPrefab != null)
        {
            shieldVisual = Instantiate(itemData.shieldVisualPrefab, Player.instance.transform);
            shieldVisual.transform.localPosition = Vector3.zero;
        }
        else
        {
            // Create a particle effect instead of a solid mesh sphere
            shieldVisual = new GameObject("ShieldVisual");
            shieldVisual.transform.parent = Player.instance.transform;
            shieldVisual.transform.localPosition = Vector3.zero;

            // Add particle system
            ParticleSystem particles = shieldVisual.AddComponent<ParticleSystem>();

            // Blue particle effect for shield
            var main = particles.main;
            main.startColor = new Color(0.2f, 0.6f, 1f, 0.3f);
            main.startSize = 0.2f;
            main.startSpeed = 0.5f;
            main.startLifetime = 2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = particles.emission;
            emission.rateOverTime = 30;

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 1.2f;

            // Make particles move in circular patterns
            var velocityOverLifetime = particles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.orbitalX = 0.5f;
            velocityOverLifetime.orbitalY = 0.5f;
            velocityOverLifetime.orbitalZ = 0.5f;

            particles.Play();
        }

        UpdateShieldVisual();
    }

    private void UpdateShieldVisual()
    {
        if (shieldVisual == null) return;

        // If using particles
        ParticleSystem particleSystem = shieldVisual.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            // Fade from blue to red as health decreases
            float healthRatio = (float)currentShieldHealth / itemData.shieldHealth;
            Color shieldColor = Color.Lerp(new Color(1f, 0.2f, 0.2f, 0.3f), new Color(0.2f, 0.6f, 1f, 0.3f), healthRatio);

            var main = particleSystem.main;
            main.startColor = shieldColor;

            // Adjust emission rate based on health
            var emission = particleSystem.emission;
            emission.rateOverTime = 20 + (healthRatio * 10);
        }
        else
        {
            // For legacy mesh renderer (if present)
            MeshRenderer renderer = shieldVisual.GetComponent<MeshRenderer>();
            if (renderer != null && renderer.material != null)
            {
                // Fade from blue to red as health decreases
                float healthRatio = (float)currentShieldHealth / itemData.shieldHealth;
                Color shieldColor = Color.Lerp(new Color(1f, 0.2f, 0.2f, 0.3f), new Color(0.2f, 0.6f, 1f, 0.3f), healthRatio);
                renderer.material.color = shieldColor;

                // Scale shield based on health
                float scale = 1.0f + (0.2f * healthRatio);
                shieldVisual.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }

    public bool TryAbsorbDamage(ref int damage)
    {
        if (currentShieldHealth <= 0)
            return false;

        // Absorb damage up to current shield health
        int damageTaken = Mathf.Min(currentShieldHealth, damage);
        currentShieldHealth -= damageTaken;

        // If shield absorbed all damage, set damage to 0
        if (damageTaken >= damage)
        {
            damage = 0;
        }
        else
        {
            damage -= damageTaken;
        }

        // Update visuals
        UpdateShieldVisual();

        // Shield break or hit effect
        if (currentShieldHealth <= 0)
        {
            StartCoroutine(ShieldBreakEffect());
        }
        else
        {
            StartCoroutine(ShieldHitEffect());
        }

        Debug.Log($"Shield absorbed {damageTaken} damage! {currentShieldHealth} shield health remaining.");
        return true;
    }

    private IEnumerator ShieldHitEffect()
    {
        if (shieldVisual == null)
            yield break;

        ParticleSystem particleSystem = shieldVisual.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            // Store original rate
            var emission = particleSystem.emission;
            float originalRate = emission.rateOverTime.constant;

            // Burst particles
            emission.rateOverTime = originalRate * 3f;

            yield return new WaitForSeconds(0.2f);

            // Restore original rate
            emission.rateOverTime = originalRate;
        }
        else
        {
            // Legacy mesh renderer effect
            MeshRenderer renderer = shieldVisual.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Color originalColor = renderer.material.color;
                renderer.material.color = new Color(1f, 1f, 1f, 0.7f);

                yield return new WaitForSeconds(0.1f);

                renderer.material.color = originalColor;
            }
        }
    }

    private IEnumerator ShieldBreakEffect()
    {
        if (shieldVisual == null)
            yield break;

        ParticleSystem particleSystem = shieldVisual.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            // Final burst effect
            var emission = particleSystem.emission;
            emission.rateOverTime = 100;

            // Change color to white
            var main = particleSystem.main;
            main.startColor = new Color(1f, 1f, 1f, 0.7f);

            yield return new WaitForSeconds(0.3f);

            // Fade out effect
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                emission.rateOverTime = Mathf.Lerp(100, 0, t);

                yield return null;
            }

            particleSystem.Stop();
        }
        else
        {
            // Legacy mesh renderer effect
            MeshRenderer renderer = shieldVisual.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                // Flash white
                renderer.material.color = new Color(1f, 1f, 1f, 0.7f);
                yield return new WaitForSeconds(0.1f);

                // Fade out
                float duration = 0.5f;
                float elapsed = 0f;
                Color startColor = renderer.material.color;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;

                    Color newColor = startColor;
                    newColor.a = Mathf.Lerp(startColor.a, 0f, t);
                    renderer.material.color = newColor;

                    yield return null;
                }

                shieldVisual.SetActive(false);
            }
        }
    }

    private void OnPlayerHit()
    {
        // This is where stagger resistance would be implemented
        // Stagger system is not implemented yet
    }

    private void OnDisable()
    {
        if (Player.instance != null)
        {
            Player.instance.onPlayerHit -= OnPlayerHit;
        }

        if (shieldVisual != null)
        {
            Destroy(shieldVisual);
            shieldVisual = null;
        }

        Debug.Log("Shield Effect removed.");
    }
}