using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{

    [Header("UI Text")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI capsText;

    [Header("Image Indicators")]
    public Image attackImage;
    public Image dashImage;

    //LD Montello
    //updates the visual for how many coins the player has.
    //here is where we'd implement any animations we'd 
    //want for UI
    public void UpdateCoins(int coins)
    {
        coinsText.text = coins.ToString();
    }

    //LD Montello
    //updates the visual for how many caps the player has.
    //here is where we'd implement any animations we'd 
    //want for UI
    public void UpdateCaps(int caps)
    {
        capsText.text = caps.ToString();
    }

    //LD Montello
    //show gray when cooling
    //down and show white when able to attack.
    public void UpdateAttackIndicator(bool canAttack)
    {
        if (canAttack)
        {
            attackImage.color = Color.white;
        }
        else
        {
            attackImage.color = Color.grey;
        }
    }

    //LD Montello
    //show gray when cooling
    //down and show white when able to dash.
    public void UpdateDashIndicator(bool inCooldown)
    {
        if (!inCooldown)
        {
            dashImage.color = Color.white;
        }
        else
        {
            dashImage.color = Color.grey;
        }
    }
}
