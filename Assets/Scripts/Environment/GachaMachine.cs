using UnityEngine;

public class GachaMachine: IDamagable, MonoBehavior{
    [SerializeField] private List<CapsuleDrop> drops;
    [SerializeField] private GameObject capsule;
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

    public GameObject GetRandomDrop(){
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
        return null;
    }

    public void TakeDamage(int damage, GameObject other){
        //TODO: Play Gacha Animation for drawing a capsule
        GameObject drop = GetRandomDrop();
        if (drop == null){
            // TODO: Handle Error <- Shouldn't be possible
            return;
        }
        GameObject intCapsule = Instantiate(capsule);
        intCapsule.getComponent<Capsule.cs>().SetObjectHeld(drop);
    }
}