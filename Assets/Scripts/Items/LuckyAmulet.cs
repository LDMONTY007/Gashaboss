using UnityEngine;

[CreateAssetMenu(fileName = "LuckyAmulet", menuName = "Items/Lucky Amulet")]
public class LuckyAmulet : ItemData
{
    [Header("Item Settings")]
    [Tooltip("Chance to nullify damage (0.75 = 75%)")]
    public float nullifyChance = 0.75f;

    [Tooltip("Damage multiplier when not nullified")]
    public float damageMultiplier = 1.5f;

    private static bool isApplied = false;

    public override void OnPickup()
    {
        Debug.Log("Lucky Amulet picked up! Chance to nullify boss damage.");

        if (!isApplied)
        {
            Player.instance.gameObject.AddComponent<LuckyAmuletEffect>().Initialize(this);
            isApplied = true;
        }
    }

    public override void RemoveItem()
    {
        LuckyAmuletEffect effect = Player.instance.GetComponent<LuckyAmuletEffect>();
        if (effect != null)
        {
            Destroy(effect);
        }

        isApplied = false;
        Player.instance.inventory.Remove(this);

        Debug.Log("Lucky Amulet removed from inventory.");
    }

    public override void ApplyEffect()
    {
        // Passive effect, applied via LuckyAmuletEffect component
    }
}

public class LuckyAmuletEffect : MonoBehaviour
{
    private LuckyAmulet itemData;

    public void Initialize(LuckyAmulet data)
    {
        itemData = data;
        Debug.Log("Lucky Amulet effect initialized!");
    }

    public bool TryModifyDamage(ref int damage, GameObject damageSource)
    {
        // Only apply effect if damage is from a boss
        BossController boss = damageSource.GetComponent<BossController>();
        if (boss == null)
            return false;

        float roll = Random.value;

        if (roll <= itemData.nullifyChance)
        {
            damage = 0;
            Debug.Log("Lucky Amulet: Protected! Damage nullified!");
            return true;
        }
        else
        {
            damage = Mathf.RoundToInt(damage * itemData.damageMultiplier);
            Debug.Log("Lucky Amulet: Unlucky! Damage increased to " + damage);
            return true;
        }
    }

    private void OnDestroy()
    {
        Debug.Log("Lucky Amulet effect removed.");
    }
}