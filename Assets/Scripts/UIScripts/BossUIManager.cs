using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossUIManager : MonoBehaviour
{
    [Header("UI Objects")]
    public TextMeshProUGUI bossNameText;
    public Slider healthBar;

    //sets the name of the boss in the UI.
    public void SetBossName(string name)
    {
        bossNameText.text = name;
    }

    //LD Montello
    //updates the health bar to 
    //match the bosses heatlh.
    public void UpdateHealthBar(float health, float maxHealth)
    {
        healthBar.value = health / maxHealth;
    }
}
