using System;
using UnityEngine;
[CreateAssetMenu(fileName = "New Gambler's Coin", menuName = "Items/GamblerCoin")]
public class GamblerCoin: ItemData{
    public override void OnPickup(){
        this.name = "Gambler's Coin";
        this.description = "Place Holder Description";
        Player.instance.onPlayerHit += ApplyEffect;
    }
    public override void RemoveItem(){
        Player.instance.inventory.Remove(this);
        Player.instance.onPlayerHit -= ApplyEffect;
    }
    public override void ApplyEffect(){
        System.Random rand = new System.Random();
        int randomFlip = rand.Next(2);
        if (randomFlip == 0){
            Player.instance.curHealth += 1;
        }
    }
}