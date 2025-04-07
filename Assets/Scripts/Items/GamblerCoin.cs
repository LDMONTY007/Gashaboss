using System;
using UnityEngine;
[CreateAssetMenu(fileName = "New Gambler's Coin", menuName = "Items/GamblerCoin")]
public class GamblerCoin: ItemData{
    public override void OnPickup(){
        this.name = "Gambler's Coin";
        this.description = "Everytime you get hit, you can gamble with this coin to see if you can keep it!";
        Player.instance.onPlayerHit += ApplyEffect;
        Player.instance.curHealth += 1;
    }
    public override void RemoveItem(){
        Player.instance.inventory.Remove(this);
        Player.instance.onPlayerHit -= ApplyEffect;
    }
    public override void ApplyEffect(){
        System.Random rand = new System.Random();
        int randomFlip = rand.Next(2);
        if (randomFlip == 0){
            Debug.Log("Flipped Heads, You Get Your Coin Back!");
            //TODO: Play Animation for Flipping Heads?
            Player.instance.curHealth += 1;
        }else{
            //TODO: Play Animation for Flipping Tails?
            Debug.Log("Flipped Tails, You Lose Your Coin!");
            RemoveItem();
        }
    }
}