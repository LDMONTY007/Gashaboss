using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PhantomCloak", menuName = "Items/Phantom Cloak")]
public class PhantomCloak : ItemData
{
    [Tooltip("Duration of invisibility in seconds")]
    public float invisibilityDuration = 3f;

    [Tooltip("Damage multiplier for surprise attack (1.5 = 50% more damage)")]
    public float surpriseAttackMultiplier = 1.5f;

    [Tooltip("Cooldown between activations in seconds")]
    public float cooldownDuration = 15f;

    [System.NonSerialized]
    private bool isInvisible = false;

    [System.NonSerialized]
    private bool isOnCooldown = false;

    [System.NonSerialized]
    private MonoBehaviour coroutineRunner;

    [System.NonSerialized]
    private Coroutine invisibilityRoutine;

    [System.NonSerialized]
    private Coroutine cooldownRoutine;

    public override void OnPickup()
    {
        Debug.Log("Phantom Cloak picked up! You'll become invisible when damaged (with a cooldown).");

        // Store a reference to the player as our coroutine runner
        coroutineRunner = Player.instance;

        // Subscribe to player hit event to trigger invisibility
        Player.instance.onPlayerHit += OnPlayerHit;
    }

    public override void RemoveItem()
    {
        // Stop invisibility if active
        if (invisibilityRoutine != null && coroutineRunner != null)
        {
            coroutineRunner.StopCoroutine(invisibilityRoutine);
            invisibilityRoutine = null;
        }

        // Stop cooldown if active
        if (cooldownRoutine != null && coroutineRunner != null)
        {
            coroutineRunner.StopCoroutine(cooldownRoutine);
            cooldownRoutine = null;
        }

        // Unsubscribe from player hit event
        if (Player.instance != null)
        {
            Player.instance.onPlayerHit -= OnPlayerHit;
        }

        // Ensure player is visible again
        SetPlayerVisibility(true);

        // Remove from player's inventory
        if (Player.instance != null && Player.instance.inventory.Contains(this))
        {
            Player.instance.inventory.Remove(this);
        }
    }

    public override void ApplyEffect()
    {
        // Only activate if not invisible and not on cooldown
        if (!isInvisible && !isOnCooldown && invisibilityRoutine == null && coroutineRunner != null)
        {
            invisibilityRoutine = coroutineRunner.StartCoroutine(InvisibilityRoutine());
        }
        else if (isOnCooldown)
        {
            Debug.Log("Phantom Cloak is on cooldown!");
        }
    }

    // Event handler for when player takes damage
    private void OnPlayerHit()
    {
        ApplyEffect();
    }

    // Coroutine to handle invisibility
    private IEnumerator InvisibilityRoutine()
    {
        // Set player as invisible
        isInvisible = true;
        SetPlayerVisibility(false);

        Debug.Log("You've become a phantom! Bosses can't see you.");

        // Subscribe to attack event to check for surprise attack
        if (Player.instance != null && Player.instance.curWeapon != null)
        {
            Player.instance.curWeapon.onAttack += OnInvisibleAttack;
        }

        // Wait for duration or until player attacks
        float elapsed = 0;
        while (elapsed < invisibilityDuration && isInvisible)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Unsubscribe from attack event
        if (Player.instance != null && Player.instance.curWeapon != null)
        {
            Player.instance.curWeapon.onAttack -= OnInvisibleAttack;
        }

        // Make player visible again
        isInvisible = false;
        SetPlayerVisibility(true);

        Debug.Log("You're visible again.");

        // Reset for next use
        invisibilityRoutine = null;

        // Start cooldown
        isOnCooldown = true;
        cooldownRoutine = coroutineRunner.StartCoroutine(CooldownRoutine());
    }

    // Coroutine to handle cooldown
    private IEnumerator CooldownRoutine()
    {
        Debug.Log($"Phantom Cloak on cooldown for {cooldownDuration} seconds");

        // Wait for cooldown duration
        float elapsed = 0;
        while (elapsed < cooldownDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset cooldown
        isOnCooldown = false;
        cooldownRoutine = null;

        Debug.Log("Phantom Cloak ready to use again!");

        // Use yield break to end the coroutine properly
        yield break;
    }

    // Set player model visibility
    private void SetPlayerVisibility(bool visible)
    {

        if (Player.instance == null)
            return;

        // Set the static visibility flag that the boss checks
        Player.instance.isVisible = visible;

        if (Player.instance.animatedModel != null)
        {
            // Make model semi-transparent when invisible
            Renderer[] renderers = Player.instance.animatedModel.GetComponentsInChildren<Renderer>();
            if (renderers != null)
            {
                foreach (Renderer renderer in renderers)
                {
                    if (renderer != null && renderer.material != null)
                    {
                        try
                        {
                            // Get current material color
                            // Color color = renderer.material.color;

                            // Set alpha based on visibility
                            // color.a = visible ? 1f : 0.3f;

                            // Apply color with new alpha
                            // renderer.material.color = color;

                            // Make sure the material is set to use transparency
                            if (!visible)
                            {
                                renderer.enabled = false;
                                /*renderer.material.SetFloat("_Mode", 3); // Transparent rendering mode
                                renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                                renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                                renderer.material.SetInt("_ZWrite", 0);
                                renderer.material.DisableKeyword("_ALPHATEST_ON");
                                renderer.material.EnableKeyword("_ALPHABLEND_ON");
                                renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                                renderer.material.renderQueue = 3000;*/
                            }
                            else
                            {
                                renderer.enabled = true;

                                // Restore opaque rendering
                                /*                                renderer.material.SetFloat("_Mode", 0); // Opaque rendering mode
                                                                renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                                                                renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                                                                renderer.material.SetInt("_ZWrite", 1);
                                                                renderer.material.DisableKeyword("_ALPHATEST_ON");
                                                                renderer.material.DisableKeyword("_ALPHABLEND_ON");
                                                                renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                                                                renderer.material.renderQueue = -1;*/
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning($"Error changing material transparency: {e.Message}");
                        }
                    }
                }
            }
        }
    }

    // Event handler for when the invisible player attacks
    private void OnInvisibleAttack()
    {
        if (isInvisible)
        {
            Debug.Log($"Surprise attack! Damage increased by {(surpriseAttackMultiplier - 1) * 100}%");

            // Find the boss and apply the damage multiplier
            BossController boss = Object.FindFirstObjectByType<BossController>();
            if (boss != null && !boss.isDead)
            {
                // Set the damage multiplier - uses the same mechanism as the Glitchy Stopwatch
                boss.incomingDamageMultiplier = surpriseAttackMultiplier;

                // Start a coroutine to reset the multiplier after a short delay
                if (coroutineRunner != null)
                {
                    coroutineRunner.StartCoroutine(ResetDamageMultiplier(boss));
                }
            }

            // End invisibility after attack
            isInvisible = false;
            SetPlayerVisibility(true);

            // Clean up
            if (Player.instance != null && Player.instance.curWeapon != null)
            {
                Player.instance.curWeapon.onAttack -= OnInvisibleAttack;
            }

            if (invisibilityRoutine != null && coroutineRunner != null)
            {
                coroutineRunner.StopCoroutine(invisibilityRoutine);
                invisibilityRoutine = null;
            }

            // Start cooldown after attack ends invisibility early
            if (!isOnCooldown)
            {
                isOnCooldown = true;
                cooldownRoutine = coroutineRunner.StartCoroutine(CooldownRoutine());
            }
        }
    }

    private IEnumerator ResetDamageMultiplier(BossController boss)
    {
        // Wait for a very short time to ensure the attack damage is calculated
        yield return new WaitForEndOfFrame();

        // Reset the damage multiplier if the boss still exists
        if (boss != null && !boss.isDead)
        {
            boss.incomingDamageMultiplier = 1.0f;
        }
    }

    // Clean up when the script is disabled or destroyed
    private void OnDisable()
    {
        if (Player.instance != null)
        {
            Player.instance.onPlayerHit -= OnPlayerHit;

            if (Player.instance.curWeapon != null)
            {
                Player.instance.curWeapon.onAttack -= OnInvisibleAttack;
            }
        }

        if (coroutineRunner != null)
        {
            if (invisibilityRoutine != null)
            {
                coroutineRunner.StopCoroutine(invisibilityRoutine);
                invisibilityRoutine = null;
            }

            if (cooldownRoutine != null)
            {
                coroutineRunner.StopCoroutine(cooldownRoutine);
                cooldownRoutine = null;
            }
        }

        // Make sure player is visible
        SetPlayerVisibility(true);
    }
}