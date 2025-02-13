using UnityEngine;

// Used to contain the drop stats for the drop tables
public class CapsuleDrop{
    public GameObject droppedObject;
    public int weight;
    public bool isRemovedAfterDrop;
}

// Base GachaMachine FrameWork
public class GachaMachine implements IDamageable{
    [SerializeField] private List<CapsuleDrop> drops;
    private int totalWeights;
    private Random rand;

    public void Start(){
        rand = new Random();
        loadDrops();
        totalWeights = 0;
        for (CapsuleDrop drop in drops){
            totalWeights += drop.weight;
        }

    }

    public GameObject getRandomDrop(){
        int dropRoll = rand.Next(1, totalWeights +1);
        int currDrop = 0;
        //Go through the objects in the list adding their weights up until
        //we find the weight we want
        //We should never have a case where we reach the end and haven't found our rolled object
        for (CapsuleDrop drop in drops){
            if (currDrop >= dropRoll){
                if (drop.isRemovedAfterDrop){
                    drops.Remove(drop);
                    totalWeights -= drop.weight;
                }
                return drop.droppedObject;
            }
            currDrop += drop.weight;
        }
    }

    public void takeDamage(int damage, GameObject other){
        GameObject drop = getRandomDrop();
        //Instaite the Capsule
        //Pass the object to the Capsule
        //TODO: Play Gacha Animation for drawing a capsule
    }
}