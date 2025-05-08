using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "FrenzyItem", menuName = "Items/Frenzy")]
public class FrenzyItem : ItemData
{
    [Header("Frenzy Settings")]
    [Tooltip("Number of attacks needed to trigger frenzy")]
    public int attacksToTrigger = 3;

    [Tooltip("How much to multiply movement speed by when in frenzy")]
    public float speedMultiplier = 1.5f;

    [Tooltip("Duration of frenzy effect in seconds")]
    public float frenzyDuration = 5f;

    [Tooltip("Cooldown before frenzy can be triggered again")]
    public float frenzyCooldown = 15f;

    private static bool isApplied = false;

    public override void OnPickup()
    {
        Debug.Log("Frenzy item picked up! Chain attacks to increase movement speed.");

        if (!isApplied)
        {
            FrenzyItemEffect effect = Player.instance.gameObject.AddComponent<FrenzyItemEffect>();
            effect.Initialize(this);
            isApplied = true;
        }
    }

    public override void RemoveItem()
    {
        FrenzyItemEffect effect = Player.instance.GetComponent<FrenzyItemEffect>();
        if (effect != null)
        {
            Destroy(effect);
        }

        isApplied = false;
        Player.instance.inventory.Remove(this);

        Debug.Log("Frenzy item removed from inventory.");
    }

    public override void ApplyEffect()
    {
        // Passive effect, handled by FrenzyItemEffect
    }
}

// Combined in same file for efficiency
public class FrenzyItemEffect : MonoBehaviour
{
    private FrenzyItem itemData;
    private Weapon playerWeapon;
    private int attackCounter = 0;
    private bool inFrenzy = false;
    private bool inCooldown = false;
    private float originalMoveSpeed;
    private ParticleSystem frenzyParticles;

    public void Initialize(FrenzyItem data)
    {
        itemData = data;

        // Subscribe to weapon attacks and successful hits
        playerWeapon = Player.instance.curWeapon;
        if (playerWeapon != null)
        {
            playerWeapon.onSuccessfulHit += OnSuccessfulHit;
        }

        // Store original speed - assuming Player has a moveSpeed property
        originalMoveSpeed = Player.instance.moveSpeed;

        // Create particle effect
        CreateFrenzyParticles();

        Debug.Log("Frenzy effect initialized! Now tracking successful hits on bosses.");
    }

    private void CreateFrenzyParticles()
    {
        GameObject particlesObj = new GameObject("FrenzyParticles");
        particlesObj.transform.parent = transform;
        particlesObj.transform.localPosition = Vector3.zero;

        frenzyParticles = particlesObj.AddComponent<ParticleSystem>();

        // Orange particle effect
        var main = frenzyParticles.main;
        main.startColor = new Color(1f, 0.5f, 0f, 0.5f);
        main.startSize = 0.3f;
        main.startSpeed = 3f;
        main.startLifetime = 0.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = frenzyParticles.emission;
        emission.rateOverTime = 20;

        var shape = frenzyParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 1f;

        frenzyParticles.Stop();
    }

    // New method that only triggers on successful hits
    private void OnSuccessfulHit()
    {
        if (inFrenzy || inCooldown)
            return;

        attackCounter++;
        Debug.Log($"Frenzy counter (successful hit): {attackCounter}/{itemData.attacksToTrigger}");

        if (attackCounter >= itemData.attacksToTrigger)
        {
            StartCoroutine(ActivateFrenzy());
            attackCounter = 0;
        }
    }

    private IEnumerator ActivateFrenzy()
    {
        inFrenzy = true;

        // Increase movement speed
        Player.instance.moveSpeed *= itemData.speedMultiplier;

        frenzyParticles.Play();

        Debug.Log($"Frenzy activated! Speed +{itemData.speedMultiplier}x for {itemData.frenzyDuration}s");

        // Duration
        yield return new WaitForSeconds(itemData.frenzyDuration);

        // Restore original speed
        Player.instance.moveSpeed = originalMoveSpeed;

        frenzyParticles.Stop();

        inFrenzy = false;
        inCooldown = true;

        Debug.Log($"Frenzy ended. Cooldown: {itemData.frenzyCooldown}s");

        // Cooldown
        yield return new WaitForSeconds(itemData.frenzyCooldown);

        inCooldown = false;
        Debug.Log("Frenzy cooldown ended. Ready to trigger again.");
    }

    private void Update()
    {
        // Check if weapon has changed
        if (Player.instance.curWeapon != playerWeapon)
        {
            if (playerWeapon != null)
            {
                playerWeapon.onSuccessfulHit -= OnSuccessfulHit;
            }

            playerWeapon = Player.instance.curWeapon;
            if (playerWeapon != null)
            {
                playerWeapon.onSuccessfulHit += OnSuccessfulHit;
            }

            // Reset counter when switching weapons
            attackCounter = 0;
        }
    }

    private void OnDisable()
    {
        if (playerWeapon != null)
        {
            playerWeapon.onSuccessfulHit -= OnSuccessfulHit;
        }

        // Restore original speed if in frenzy
        if (inFrenzy)
        {
            Player.instance.moveSpeed = originalMoveSpeed;
        }

        if (frenzyParticles != null)
        {
            Destroy(frenzyParticles.gameObject);
        }

        Debug.Log("Frenzy effect removed.");
    }
}