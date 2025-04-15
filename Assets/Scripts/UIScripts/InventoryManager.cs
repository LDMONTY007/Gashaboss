using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class InventoryManager: MonoBehaviour{
    [SerializeField] private GameObject inventoryItem;
    [SerializeField] private GameObject playerStatListMenu;
    [SerializeField] private Transform ItemContentWindow;
    [SerializeField] private GameObject closeInventoryButton;

    public void UpdatePlayerStats(){
        //Stats displayed currently: (Keep Up To Date list, pls?)
        //caps, hp, iframtime, minspeed, max speed, maxaccleration, max decleration, movespeed, 
        //rotationspeed, dashspeed, dashDistance, dashCooldown, Jump Count, jumpHieght, fallMultiplier

        // Taking and updating the text of each corresponding stat listing
        playerStatListMenu.transform.Find("Coins").GetComponent<TextMeshProUGUI>().text = "Coins: " + Player.instance.curHealth;
        playerStatListMenu.transform.Find("Caps").GetComponent<TextMeshProUGUI>().text = "Caps: " + Player.instance.caps;
        // TODO: Apply all modifiers for this before use? No, update Player to have a modified 
        // variation of each stat, that holds the modified value and a method that calculates the new value
        // That way we can rebuild the value everytime it needs to be updated, without restraint, 
        // yet not recalculate it everytime needed, Use a Variation on the value for the method, 
        // allowing the value of stat to be passed, the assigned to modified stat, etc.
        //TODO: For now, just add each stat that can be modified to this for loop?
        
        
        
        float modifiedIFrameTime = Player.instance.iFrameTime;

        foreach(StatModifier modifier in Player.instance.modifiers){
            StatModified stat = modifier.stat;
            switch(stat){
                case StatModified.iFrameTime:
                    modifiedIFrameTime = modifier.makeModifications(modifiedIFrameTime);
                    break;
            }
        }

        playerStatListMenu.transform.Find("IFrameTime").GetComponent<TextMeshProUGUI>().text = "I-Frame Time: " + modifiedIFrameTime;
        playerStatListMenu.transform.Find("MinSpeed").GetComponent<TextMeshProUGUI>().text = "Minimum Speed: " + Player.instance.minSpeed;
        playerStatListMenu.transform.Find("MaxSpeed").GetComponent<TextMeshProUGUI>().text = "Maximum Speed: " + Player.instance.maxSpeed;
        playerStatListMenu.transform.Find("MaxAcceleration").GetComponent<TextMeshProUGUI>().text = "Maximum Acceleration: " + Player.instance.maxAcceleration;
        playerStatListMenu.transform.Find("MaxDeceleration").GetComponent<TextMeshProUGUI>().text = "Maximum Deceleration: " + Player.instance.maxDeceleration;
        playerStatListMenu.transform.Find("MoveSpeed").GetComponent<TextMeshProUGUI>().text = "Move Speed: " + Player.instance.moveSpeed;
        playerStatListMenu.transform.Find("RotationSpeed").GetComponent<TextMeshProUGUI>().text = "Rotation Speed: " +  Player.instance.rotationSpeed;
        playerStatListMenu.transform.Find("DashSpeed").GetComponent<TextMeshProUGUI>().text = "Dash Speed: " + Player.instance.dashSpeed;
        playerStatListMenu.transform.Find("DashDistance").GetComponent<TextMeshProUGUI>().text = "Dash Distance: " + Player.instance.dashDist;
        playerStatListMenu.transform.Find("DashCooldown").GetComponent<TextMeshProUGUI>().text = "Dash Cooldown: " + Player.instance.dashCooldown;
        playerStatListMenu.transform.Find("JumpCount").GetComponent<TextMeshProUGUI>().text = "Jump Count: " + Player.instance.jumpCount;
        playerStatListMenu.transform.Find("JumpHeight").GetComponent<TextMeshProUGUI>().text = "Jump Height: " + Player.instance.jumpHeight;
        playerStatListMenu.transform.Find("FallMultiplier").GetComponent<TextMeshProUGUI>().text = "Fall Multiplier: " + Player.instance.fallMultiplier;
    }
    public void OnOpen(){
        // for each item in the inventory, make a listing on the menu
        foreach(ItemData item in Player.instance.inventory){
            GameObject itemDetails = Instantiate(inventoryItem, ItemContentWindow);
            TextMeshProUGUI itemName = itemDetails.transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI itemDescription = itemDetails.transform.Find("ItemDescription").GetComponent<TextMeshProUGUI>();
            if(item.name != null){
                itemName.text = item.name;
                itemDescription.text = item.description;
            }
        }
        UpdatePlayerStats();
    }
    public void OnClose(){
        foreach(Transform child in ItemContentWindow){
            Destroy(child.gameObject);
        }
    }
}