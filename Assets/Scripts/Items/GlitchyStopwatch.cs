using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "GlitchyStopwatch", menuName = "Items/Glitchy Stopwatch")]
public class GlitchyStopwatch : ItemData
{
    [Tooltip("Time between stun extensions in seconds")]
    public float stunCooldown = 10f; // Changed from 30s to 10s as requested

    [Tooltip("Additional seconds added to boss stun time")]
    public float additionalStunDuration = 1f; // Added instead of using stunDuration

    [Tooltip("Visual effect prefab for the stopwatch ready indicator")]
    public GameObject stunReadyIndicator;

    [System.NonSerialized]
    private bool readyToExtendStun = false;

    [System.NonSerialized]
    private MonoBehaviour coroutineRunner;

    [System.NonSerialized]
    private Coroutine cooldownRoutine;

    // Keep track of the current indicator
    private GameObject currentIndicator;

    public override void OnPickup()
    {
        Debug.Log("Glitchy Stopwatch picked up! Your next attack will periodically extend boss stun time.");

        coroutineRunner = Player.instance;

        if (cooldownRoutine == null && coroutineRunner != null)
        {
            cooldownRoutine = coroutineRunner.StartCoroutine(StunCooldownRoutine());
        }

        // Subscribe to all player attack events for better effect triggering
        if (Player.instance != null)
        {
            Player.instance.OnAttack += OnPlayerAttack;
            Player.instance.OnAltAttack += OnPlayerAttack;
            Player.instance.OnSpecialAttack += OnPlayerAttack;

            // Keep weapon subscription for backward compatibility
            if (Player.instance.curWeapon != null)
            {
                Player.instance.curWeapon.onAttack += OnPlayerAttack;
            }
        }
    }

    public override void RemoveItem()
    {
        // Unsubscribe from all attack events
        if (Player.instance != null)
        {
            Player.instance.OnAttack -= OnPlayerAttack;
            Player.instance.OnAltAttack -= OnPlayerAttack;
            Player.instance.OnSpecialAttack -= OnPlayerAttack;

            if (Player.instance.curWeapon != null)
            {
                Player.instance.curWeapon.onAttack -= OnPlayerAttack;
            }
        }

        if (coroutineRunner != null && cooldownRoutine != null)
        {
            coroutineRunner.StopCoroutine(cooldownRoutine);
            cooldownRoutine = null;
        }

        // Clean up any existing indicator
        CleanupIndicator();

        // Remove from inventory
        if (Player.instance != null && Player.instance.inventory.Contains(this))
        {
            Player.instance.inventory.Remove(this);
        }
    }

    public override void ApplyEffect()
    {
        if (readyToExtendStun)
        {
            readyToExtendStun = false;

            // Find a boss that's in the stun state - UPDATED to use non-deprecated method
            BossController[] bosses = Object.FindObjectsByType<BossController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            BossController stunnedBoss = null;

            foreach (BossController boss in bosses)
            {
                if (boss != null && !boss.isDead && boss.curState == BossController.BossState.stun)
                {
                    stunnedBoss = boss;
                    break;
                }
            }

            if (stunnedBoss != null)
            {
                // Extend the stun by starting a new StunCoroutine
                if (coroutineRunner != null)
                {
                    coroutineRunner.StartCoroutine(ExtendStunRoutine(stunnedBoss));
                }

                Debug.Log($"Glitchy Stopwatch extended {stunnedBoss.bossName}'s stun time by {additionalStunDuration} seconds!");

                // Clean up the indicator when used
                CleanupIndicator();
            }
            else
            {
                Debug.Log("No stunned boss found to extend stun time. Effect saved for next opportunity.");
                // Don't consume the effect if there's no stunned boss
                readyToExtendStun = true;
            }
        }
    }

    // Event handler for player attacks
    private void OnPlayerAttack()
    {
        if (readyToExtendStun)
        {
            ApplyEffect();
        }
    }

    private IEnumerator ExtendStunRoutine(BossController boss)
    {
        bool bossExisted = (boss != null);

        if (bossExisted)
        {
            // Get the original time when the boss would exit stun 
            // (we can't directly access this, so we'll just add our time)
            Debug.Log($"Extended stun time for {boss.bossName} by {additionalStunDuration} seconds!");

            // Wait for the additional stun time
            yield return new WaitForSeconds(additionalStunDuration);

            // Only change state if still stunned and the boss still exists
            if (bossExisted && boss != null && !boss.isDead && boss.curState == BossController.BossState.stun)
            {
                boss.SwitchToIdle(1f);
                Debug.Log($"{boss.bossName} has recovered from extended stun.");

                // Note: The stun immunity will be handled by the boss's StunCoroutine already running
            }
        }
    }

    private IEnumerator StunCooldownRoutine()
    {
        while (true)
        {
            readyToExtendStun = false;

            // Clean up any existing indicator
            CleanupIndicator();

            Debug.Log($"Glitchy Stopwatch cooling down for {stunCooldown} seconds.");
            yield return new WaitForSeconds(stunCooldown);

            readyToExtendStun = true;

            if (stunReadyIndicator != null && Player.instance != null)
            {
                // Create new indicator
                currentIndicator = Object.Instantiate(stunReadyIndicator,
                    Player.instance.transform.position + Vector3.up * 2,
                    Quaternion.identity);

                // Make it follow the player
                if (currentIndicator != null)
                {
                    IndicatorFollower follower = currentIndicator.AddComponent<IndicatorFollower>();
                    follower.Initialize(Player.instance.transform, Vector3.up * 2);
                }
            }

            Debug.Log("Glitchy Stopwatch charged and ready to extend a boss's stun time!");

            // Wait until the effect is used
            yield return new WaitUntil(() => !readyToExtendStun);
        }
    }

    private void CleanupIndicator()
    {
        if (currentIndicator != null)
        {
            Object.Destroy(currentIndicator);
            currentIndicator = null;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from all attack events
        if (Player.instance != null)
        {
            Player.instance.OnAttack -= OnPlayerAttack;
            Player.instance.OnAltAttack -= OnPlayerAttack;
            Player.instance.OnSpecialAttack -= OnPlayerAttack;

            if (Player.instance.curWeapon != null)
            {
                Player.instance.curWeapon.onAttack -= OnPlayerAttack;
            }
        }

        if (coroutineRunner != null && cooldownRoutine != null)
        {
            coroutineRunner.StopCoroutine(cooldownRoutine);
        }

        // Clean up any existing indicator
        CleanupIndicator();
    }
}

// Simple component to make indicator follow the player
// Kept the same as original
public class IndicatorFollower : MonoBehaviour
{
    private Transform target;
    private Vector3 offset;

    public void Initialize(Transform followTarget, Vector3 followOffset)
    {
        target = followTarget;
        offset = followOffset;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}