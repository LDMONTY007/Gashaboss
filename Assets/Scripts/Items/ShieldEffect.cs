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
            // Create a basic shield visual with transparent blue material
            shieldVisual = new GameObject("ShieldVisual");
            shieldVisual.transform.parent = Player.instance.transform;
            shieldVisual.transform.localPosition = Vector3.zero;

            MeshFilter meshFilter = shieldVisual.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateSphereMesh(1.2f);

            MeshRenderer renderer = shieldVisual.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = new Color(0.2f, 0.6f, 1f, 0.3f);
            renderer.material.SetFloat("_Mode", 3);
            renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            renderer.material.SetInt("_ZWrite", 0);
            renderer.material.DisableKeyword("_ALPHATEST_ON");
            renderer.material.EnableKeyword("_ALPHABLEND_ON");
            renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            renderer.material.renderQueue = 3000;
        }

        UpdateShieldVisual();
    }

    private Mesh CreateSphereMesh(float radius)
    {
        GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Mesh mesh = tempSphere.GetComponent<MeshFilter>().sharedMesh;
        Destroy(tempSphere);

        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] *= radius;
        }
        mesh.vertices = vertices;

        return mesh;
    }

    private void UpdateShieldVisual()
    {
        if (shieldVisual != null)
        {
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

        MeshRenderer renderer = shieldVisual.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            renderer.material.color = new Color(1f, 1f, 1f, 0.7f);

            yield return new WaitForSeconds(0.1f);

            renderer.material.color = originalColor;
        }
    }

    private IEnumerator ShieldBreakEffect()
    {
        if (shieldVisual == null)
            yield break;

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