using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "BubbleWand", menuName = "Items/Bubble Wand")]
public class BubbleWand : ItemData
{
    [Tooltip("Time between bubble activations in seconds")]
    public float bubbleCooldown = 30f;

    [System.NonSerialized] // Runtime-only variables shouldn't be serialized
    private bool bubbleActive = false;

    [System.NonSerialized]
    private MonoBehaviour coroutineRunner;

    [System.NonSerialized]
    private Coroutine bubbleRoutine;

    // Visual effect prefab (optional)
    public GameObject bubbleEffectPrefab;

    public override void OnPickup()
    {
        Debug.Log("Bubble Wand picked up! Your coin pool is protected periodically.");

        // Store a reference to the player as our coroutine runner
        coroutineRunner = Player.instance;

        // Start the bubble protection routine
        if (bubbleRoutine == null && coroutineRunner != null)
        {
            bubbleRoutine = coroutineRunner.StartCoroutine(BubbleProtectionRoutine());
        }
    }

    public override void RemoveItem()
    {
        // Stop the bubble protection routine if it's running
        if (bubbleRoutine != null && coroutineRunner != null)
        {
            coroutineRunner.StopCoroutine(bubbleRoutine);
            bubbleRoutine = null;
        }

        // Unsubscribe from player hit event if we're actively listening
        if (bubbleActive)
        {
            Player.instance.onPlayerHit -= OnPlayerHit;
        }

        // Remove from player's inventory
        Player.instance.inventory.Remove(this);

        Debug.Log("Bubble Wand removed from inventory.");
    }

    public override void ApplyEffect()
    {
        // Activate the bubble protection
        bubbleActive = true;

        // Visual feedback
        Debug.Log("Bubble protection activated!");

        // Spawn visual effect if available
        if (bubbleEffectPrefab != null && Player.instance != null)
        {
            GameObject.Instantiate(bubbleEffectPrefab, Player.instance.transform.position, Quaternion.identity, Player.instance.transform);
        }

        // Start listening for damage events
        Player.instance.onPlayerHit += OnPlayerHit;
    }

    // Coroutine to handle bubble protection logic
    private IEnumerator BubbleProtectionRoutine()
    {
        while (true)
        {
            // Wait for the cooldown period
            yield return new WaitForSeconds(bubbleCooldown);

            // Activate the bubble
            ApplyEffect();

            // Wait until the bubble is used
            yield return new WaitUntil(() => !bubbleActive);
        }
    }

    // Event handler for when the player takes damage
    private void OnPlayerHit()
    {
        if (bubbleActive)
        {
            // Prevent damage by adding a coin back to the player
            Player.instance.curHealth += 1;

            // Deactivate the bubble
            bubbleActive = false;

            // Unsubscribe from the event until next activation
            Player.instance.onPlayerHit -= OnPlayerHit;

            Debug.Log("Bubble protection used! Player coin protected.");

            // Could destroy visual effect here if implemented
        }
    }

    // Clean up when the script is disabled or destroyed
    private void OnDisable()
    {
        if (bubbleActive && Player.instance != null)
        {
            Player.instance.onPlayerHit -= OnPlayerHit;
        }

        if (bubbleRoutine != null && coroutineRunner != null)
        {
            coroutineRunner.StopCoroutine(bubbleRoutine);
            bubbleRoutine = null;
        }
    }
}