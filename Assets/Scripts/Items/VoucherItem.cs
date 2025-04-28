using UnityEngine;

[CreateAssetMenu(fileName = "Voucher", menuName = "Items/Voucher")]
public class VoucherItem : ItemData
{
    [Header("Voucher Settings")]
    [Tooltip("Discount percentage for caps purchases (0-1)")]
    public float discountPercentage = 0.5f; // 50% discount

    private static bool isApplied = false;

    public override void OnPickup()
    {
        Debug.Log("Voucher picked up! Next caps purchase will be 50% off.");

        // Prevent stacking by checking if already applied
        if (!isApplied)
        {
            // Add the effect component to the player
            VoucherEffect effect = Player.instance.gameObject.AddComponent<VoucherEffect>();
            effect.Initialize(this);
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
    private VoucherItem itemData;
    [HideInInspector]
    public bool hasDiscount = true;
    [HideInInspector]
    public bool hasFreeGacha = true;  // Added for backward compatibility

    public void Initialize(VoucherItem data)
    {
        itemData = data;
        Debug.Log($"Voucher effect initialized! Next caps purchase will be {itemData.discountPercentage * 100}% off.");
    }

    // Method to apply discount to a caps purchase
    public int GetDiscountedCost(int originalCost)
    {
        if (hasDiscount)
        {
            int discountedCost = Mathf.RoundToInt(originalCost * (1 - itemData.discountPercentage));
            Debug.Log($"Applied discount: {originalCost} caps -> {discountedCost} caps");
            return discountedCost;
        }
        return originalCost;
    }

    // Method to use the discount
    public bool UseDiscount()
    {
        if (hasDiscount)
        {
            hasDiscount = false;
            Debug.Log("Used caps purchase discount!");
            return true;
        }
        return false;
    }

    // Method to check if player has a discount available
    public bool HasDiscountAvailable()
    {
        return hasDiscount;
    }

    // Method for backward compatibility with GachaMachine.cs
    public void ResetFreeGacha()
    {
        // This now resets the discount for caps
        hasDiscount = true;
        hasFreeGacha = true;  // Keep for compatibility
        Debug.Log("Caps discount refreshed after boss defeat!");
    }

    // Method for backward compatibility with GachaMachine.cs
    public bool HasFreeGacha()
    {
        // For compatibility
        return hasFreeGacha;
    }

    // Method for backward compatibility with GachaMachine.cs
    public bool UseFreeGacha()
    {
        // For compatibility - this will now use the discount
        if (hasFreeGacha)
        {
            hasFreeGacha = false;
            hasDiscount = false;  // Also use up the discount
            Debug.Log("Used free gacha (caps discount)!");
            return true;
        }
        return false;
    }

    private void OnDestroy()
    {
        Debug.Log("Voucher effect removed.");
    }
}