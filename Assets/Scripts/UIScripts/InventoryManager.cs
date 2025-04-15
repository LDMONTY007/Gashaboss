using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class inventoryPanel: MonoBehaviour{
    [SerializeField] private GameObject inventoryItem;
    [SerializeField] private GameObject playerStatListMenu;
    [SerializeField] private Transform ItemContentWindow;
    [SerializeField] private GameObject closeInventoryButton;

    public void UpdatePlayerStats(){
        //Stats displayed currently: (Keep Up To Date list, pls?)
        //caps, hp, iframtime, minspeed, max speed, maxaccleration, max decleration, movespeed, 
        //rotationspeed, dashspeed, dashDistance, dashCooldown, Jump Count, jumpHieght, fallMultiplier

        // Taking and updating the text of each corresponding stat listing
        playerStatListMenu.transform.Find("Coins").GetComponent<TextMeshProUGUI>().text = Player.instance.curHealth;
        playerStatListMenu.transform.Find("Caps").GetComponent<TextMeshProUGUI>().text = Player.instance.caps;
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
                    modifiedIFrameTime = modifire.makeModifications(modifiedIFrameTime);
            }
        }

        playerStatListMenu.transform.Find("IFramTime").GetComponent<TextMeshProUGUI>().text = modifiedIFrameTime;
        playerStatListMenu.transform.Find("MinSpeed").GetComponent<TextMeshProUGUI>().text = Player.instance.minSpeed;
        playerStatListMenu.transform.Find("MaxSpeed").GetComponent<TextMeshProUGUI>().text = Player.instance.maxSpeed;
        playerStatListMenu.transform.Find("MaxAcceleration").GetComponent<TextMeshProUGUI>().text = Player.instance.maxAcceleration;
        playerStatListMenu.transform.Find("MaxDeceleration").GetComponent<TextMeshProUGUI>().text = Player.instance.maxDeceleration;
        playerStatListMenu.transform.Find("MoveSpeed").GetComponent<TextMeshProUGUI>().text = Player.instance.moveSpeed;
        playerStatListMenu.transform.Find("RotationSpeed").GetComponent<TextMeshProUGUI>().text = Player.instance.rotationSpeed;
        playerStatListMenu.transform.Find("DashSpeed").GetComponent<TextMeshProUGUI>().text = Player.instance.dashSpeed;
        playerStatListMenu.transform.Find("DashDistance").GetComponent<TextMeshProUGUI>().text = Player.instance.dashDistance;
        playerStatListMenu.transform.Find("DashCooldown").GetComponent<TextMeshProUGUI>().text = Player.instance.dashCooldown;
        playerStatListMenu.transform.Find("JumpCount").GetComponent<TextMeshProUGUI>().text = Player.instance.jumpCount;
        playerStatListMenu.transform.Find("JumpHeight").GetComponent<TextMeshProUGUI>().text = Player.instance.jumpHeight;
        playerStatListMenu.transform.Find("FallMultiplier").GetComponent<TextMeshProUGUI>().text = Player.instance.fallMultiplier;
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