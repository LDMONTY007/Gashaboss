using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "GlitchyStopwatch", menuName = "Items/Glitchy Stopwatch")]
public class GlitchyStopwatch : ItemData
{
    [Tooltip("Time between stun activations in seconds")]
    public float stunCooldown = 30f;

    [Tooltip("Duration of boss stun in seconds")]
    public float stunDuration = 3f;

    [Tooltip("Damage multiplier while boss is stunned (1.2 = 20% more damage)")]
    public float damageMultiplier = 1.2f;

    [System.NonSerialized]
    private bool readyToStun = false;

    [System.NonSerialized]
    private MonoBehaviour coroutineRunner;

    [System.NonSerialized]
    private Coroutine stunRoutine;

    public GameObject stunReadyIndicator;

    // Keep track of the current indicator
    private GameObject currentIndicator;

    public override void OnPickup()
    {
        Debug.Log("Glitchy Stopwatch picked up! Your next attack will periodically stun bosses.");

        coroutineRunner = Player.instance;

        if (stunRoutine == null && coroutineRunner != null)
        {
            stunRoutine = coroutineRunner.StartCoroutine(StunCooldownRoutine());
        }

        // Subscribe to the attack event if weapon exists
        if (Player.instance != null && Player.instance.curWeapon != null)
        {
            Player.instance.curWeapon.onAttack += OnPlayerAttack;
        }
    }

    public override void RemoveItem()
    {
        // Unsubscribe from attack event
        if (Player.instance != null && Player.instance.curWeapon != null)
        {
            Player.instance.curWeapon.onAttack -= OnPlayerAttack;
        }

        if (coroutineRunner != null && stunRoutine != null)
        {
            coroutineRunner.StopCoroutine(stunRoutine);
            stunRoutine = null;
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
        if (readyToStun)
        {
            readyToStun = false;

            BossController boss = Object.FindObjectOfType<BossController>();
            if (boss != null && !boss.isDead)
            {
                boss.curState = BossController.BossState.stun;

                if (coroutineRunner != null)
                {
                    coroutineRunner.StartCoroutine(StunEffectRoutine(boss));
                }

                Debug.Log("Boss stunned by Glitchy Stopwatch!");

                // Clean up the indicator when used
                CleanupIndicator();
            }
        }
    }

    // Event handler for player attacks
    private void OnPlayerAttack()
    {
        if (readyToStun)
        {
            ApplyEffect();
        }
    }

    private IEnumerator StunEffectRoutine(BossController boss)
    {
        bool bossExisted = (boss != null);

        if (bossExisted)
        {
            // Apply damage multiplier using our new field
            boss.incomingDamageMultiplier = damageMultiplier;

            Debug.Log($"Boss is glitched and will take {damageMultiplier}x damage!");
        }

        yield return new WaitForSeconds(stunDuration);

        if (bossExisted && boss != null && !boss.isDead)
        {
            // Only change state if still stunned (could have been changed by another effect)
            if (boss.curState == BossController.BossState.stun)
            {
                boss.SwitchToIdle(1f);
            }

            // Reset damage multiplier
            boss.incomingDamageMultiplier = 1.0f;

            Debug.Log("Boss has returned to normal!");
        }
    }

    private IEnumerator StunCooldownRoutine()
    {
        while (true)
        {
            readyToStun = false;

            // Clean up any existing indicator
            CleanupIndicator();

            yield return new WaitForSeconds(stunCooldown);

            readyToStun = true;

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

            Debug.Log("Glitchy Stopwatch charged and ready to use!");

            yield return new WaitUntil(() => !readyToStun);
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
        // Unsubscribe from weapon event
        if (Player.instance != null && Player.instance.curWeapon != null)
        {
            Player.instance.curWeapon.onAttack -= OnPlayerAttack;
        }

        if (coroutineRunner != null && stunRoutine != null)
        {
            coroutineRunner.StopCoroutine(stunRoutine);
        }

        // Clean up any existing indicator
        CleanupIndicator();
    }
}

// Simple component to make indicator follow the player
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