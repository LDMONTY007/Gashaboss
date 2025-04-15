using UnityEngine;

[CreateAssetMenu(fileName = "WingedBoot", menuName = "Items/Winged Boot")]
public class WingedBoot : ItemData
{
    [Tooltip("Number of extra jumps this item grants")]
    public int extraJumps = 1;

    public override void OnPickup()
    {
        Debug.Log("Winged Boot picked up! Player can now jump higher.");

        // Increase player's total jump count
        Player.instance.jumpTotal += extraJumps;

        // The jumpCount will be automatically reset to jumpTotal when the player lands
        // due to the logic in the Player's Update method:
        // if (isGrounded) { jumpCount = jumpTotal; }
    }

    public override void RemoveItem()
    {
        // Decrease player's total jump count
        Player.instance.jumpTotal -= extraJumps;

        // Make sure we don't go below the base jump count
        Player.instance.jumpTotal = Mathf.Max(Player.instance.jumpTotal, 1);

        // Remove from player's inventory
        Player.instance.inventory.Remove(this);

        Debug.Log("Winged Boot removed from inventory.");
    }

    public override void ApplyEffect()
    {
        // This effect is passive and is applied when picked up
        // No additional logic needed here as jump count is already increased
    }
}