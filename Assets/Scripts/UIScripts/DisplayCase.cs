/*
 * ==============================================
 *  DisplayCase.cs - Interactable Display Case Script
 * ==============================================
 * 
 *     Purpose:
 *     - Allows the player to interact with a display case.
 *     - When the player is near, they can press [E] (or whatever button we decide on) to open the Collection UI.
 * 
 *     How to Use:
 *     - Attach this script to a **new "DisplayCase" GameObject** in the scene.
 *     - Assign `CollectionPanelUI` in the **Inspector**.
 *     - Add a **Box Collider** to the `DisplayCase` and set it as **"Is Trigger"**.
 *     - Adjust the collider size so the player can walk into it.
 *     - Test by pressing **"E"** near the Display Case to open the Collection UI.
 * 
 *     Expected Behavior:
 *     - Walking near the display case shows a **message in the Console**.
 *     - Pressing **[E]** **opens the Collection UI**.
 *     - Walking away stops interaction.
 */

using UnityEngine;

public class DisplayCase : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject collectionPanelUI; // Assign the CollectionPanelUI in the Inspector

    private bool playerInRange = false; // Tracks if the player is near the display case

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player is near the display case. Press [E] to open collection.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Player left the display case area.");
        }
    }

    private void Update()
    {
        if (UIManager.Instance != null && UIManager.Instance.uiBlock) return;

        if (playerInRange && Input.GetKeyDown(KeyCode.E)) // Press "E" to interact
        {
            CollectionManager.instance.OpenCollection();
        }
    }

}

