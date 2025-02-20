using TMPro;
using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{

    [Header("UI Text")]
    public TextMeshProUGUI coinsText;

    //LD Montello
    //updates the visual for how many coins the player has.
    //here is where we'd implement any animations we'd 
    //want for UI
    public void UpdateCoins(int coins)
    {
        coinsText.text = coins.ToString();
    }
}
