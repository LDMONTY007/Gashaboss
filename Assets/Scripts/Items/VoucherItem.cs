using UnityEngine;

[CreateAssetMenu(fileName = "Voucher", menuName = "Items/Voucher")]
public class VoucherItem : ItemData
{
    private static bool isApplied = false;

    public override void OnPickup()
    {
        Debug.Log("Voucher picked up! First gacha pull after each boss will be free.");

        // Prevent stacking by checking if already applied
        if (!isApplied)
        {
            // Add the effect component to the player
            Player.instance.gameObject.AddComponent<VoucherEffect>();
            isApplied = true;
        }
    }

    public override void RemoveItem()
    {
        // Remove the effect component
        VoucherEffect effect = Player.instance.GetComponent<VoucherEffect>();
        if (effect != null)
        {
            Destroy(effect);
        }

        // Mark as not applied
        isApplied = false;

        // Remove from player's inventory
        Player.instance.inventory.Remove(this);

        Debug.Log("Voucher removed from inventory.");
    }

    public override void ApplyEffect()
    {
        // This effect is passive and is applied when picked up
        // Effect handling is done in the VoucherEffect component
    }
}

// Separate MonoBehaviour component to handle the functionality
public class VoucherEffect : MonoBehaviour
{
    [HideInInspector]
    public bool hasFreeGacha = true;

    private void Start()
    {
        Debug.Log("Voucher effect initialized! First gacha pull will be free.");
    }

    // Method to use the free gacha pull
    public bool UseFreeGacha()
    {
        if (hasFreeGacha)
        {
            hasFreeGacha = false;
            Debug.Log("Used free gacha pull!");
            return true;
        }
        return false;
    }

    // Method to reset free gacha after boss defeat
    public void ResetFreeGacha()
    {
        hasFreeGacha = true;
        Debug.Log("Free gacha pull restored after boss defeat!");
    }

    private void OnDestroy()
    {
        Debug.Log("Voucher effect removed.");
    }
}