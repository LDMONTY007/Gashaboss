using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaMachine : MonoBehaviour, IDamageable {
    //LD Montello.
    //these need to be GameObjects because they
    //will be prefabs with the CapsuleDrop class on them.
    //this is also how we can have specific models for different
    //capsules for different machines or even bosses.
    [SerializeField] private List<DropData> drops;
    [SerializeField] private GameObject capsule;
    private int totalWeights;

    public Transform capsuleSpawnTransform;

    public Animator gachaAnimator;

    private GameObject currentCapsule;
    //the currently dropped object from a capsule
    public GameObject currentDrop;

    //if current capsule isn't null, then
    //a capsule exists
    bool capsuleExists => currentCapsule != null;

    //if current drop isn't null, then
    //a drop exists
    bool dropExists => currentDrop != null;
   
    //this isn't the best way to check for this,
    //but seeing as we don't do it often
    //when we check we'll just search for if a boss controller
    //exists.
    bool bossExists => FindAnyObjectByType<BossController>() != null;

    //set to true when the animation for dispensing starts and set back to false
    //when the animatino ends in SpawnCapsule
    public bool isDispensing = false;

    public SetRandomSeed setRandomSeedAnimated;

    public AudioClip gachaRollClip;

    private AudioSource gachaAudioSource;

    [HideInInspector] public event Action OnDispense;
    [HideInInspector] public event Action BossDefeated;

    public void Start(){
        gachaAudioSource = GetComponent<AudioSource>();


        //always attempt to remove all drops that are flagged for removal,
        //this is so any drops that are permanantly owned once collected such as a new
        //gashapon machine aren't dispensed more than once in a run regardless of if the 
        //player has died.
        //we simply check if the collection manager contains an environmental unlock for now.
        //we can discuss if the drops that only get dropped once also aren't reset.
        drops.RemoveAll(d => d.isEnvironmentalUnlock == true && CollectionManager.instance.CollectionContains(d));

        

        totalWeights = 0;
        foreach (DropData drop in drops){
            totalWeights += drop.weight;
        }

    }

    public GameObject GetRandomDrop(){


        OnDispense?.Invoke();

        int dropRoll = UnityEngine.Random.Range(1, totalWeights + 1);
        int currDrop = 0;
        //Go through the objects in the list adding their weights up until
        //we find the weight we want
        //We should never have a case where we reach the end and haven't found our rolled object
        foreach (DropData drop in drops){
            currDrop += drop.weight;
            if (currDrop >= dropRoll){
                if (drop.isRemovedAfterDrop){
                    drops.Remove(drop);
                    totalWeights -= drop.weight;
                }
                
                return drop.droppedObject;
            }
        }
        Debug.LogError("DropRoll: " + dropRoll + "\ncurrDrop: " + currDrop);
        return null;
    }

    public void TakeDamage(int damage, GameObject other){
        
        //try to buy a capsule so long as there isn't a capsule waiting to be opened.
        if (!isDispensing && !capsuleExists && !bossExists && Player.instance != null && TryBuyCapsule(Player.instance))
        {
            //say we are dispensing.
            isDispensing = true;

            //if a drop exists, 
            //we know it isn't a boss if we 
            //reach this check so destroy it.
            //this is how the player gets rid of items or a weapon
            //they don't want, if they don't pick it up then it gets destroyed.
            if (dropExists)
            {
                Destroy(currentDrop);
            }
            

            //Generate a new color for the capsule
            setRandomSeedAnimated.GenRandomSeed();

            //after the animation finishes playing it will spawn the capsule.
            PlayDispenseAnimation();
        }

        //print out data about the machine taking damage.
        Debug.Log("Machine Took: ".Color("Cyan") + damage.ToString().Color("Red") + " from " + other.transform.root.name.Color("Red"));
    }

    public void SpawnCapsule(){
        isDispensing = false;

        Debug.Log("TRY SPAWN DROP");
        GameObject drop = GetRandomDrop();
        if (drop == null)
        {
            // TODO: Handle Error <- Shouldn't be possible
            Debug.LogError("DROP WAS NULL");
            return;
        }
        Debug.Log("Spawning Drop...");
        //instantiate the capsule at the spawn position for capsules.
        GameObject intCapsule = Instantiate(capsule, capsuleSpawnTransform.position, capsule.transform.rotation);
        intCapsule.GetComponent<Capsule>().SetObjectHeld(drop);

        //set the parent machine of the capsule.
        intCapsule.GetComponent<Capsule>().parentMachine = this;


        //copy the seed color from the animated capsule's color seed to the new instanced capsule.
        intCapsule.GetComponentInChildren<SetRandomSeed>().seed = setRandomSeedAnimated.seed;

        //set a reference to our current capsule.
        currentCapsule = intCapsule;
    }

    public void SpawnSpecificCapsule(DropData toDrop){
        Debug.Log("TRY SPAWN SPECIFIC DROP");
        if (toDrop == null){
            Debug.LogError("Dropdata was null, check passed value");
            return;
        }
        GameObject drop = toDrop.droppedObject;
        if (drop == null){
            Debug.LogError("Drop from drop data was null, check dropdata prefab");
            return;
        }
        //instantiate the capsule at the spawn position for capsules.
        GameObject intCapsule = Instantiate(capsule, capsuleSpawnTransform.position, capsule.transform.rotation);
        intCapsule.GetComponent<Capsule>().SetObjectHeld(drop);

        //copy the seed color from the animated capsule's color seed to the new instanced capsule.
        intCapsule.GetComponentInChildren<SetRandomSeed>().seed = setRandomSeedAnimated.seed;

        //set a reference to our current capsule.
        currentCapsule = intCapsule;
    }

    public bool TryBuyCapsule(Player p)
    {
        // Check if player has voucher effect and can use free gacha
        VoucherEffect voucher = p.GetComponent<VoucherEffect>();
        if (voucher != null && voucher.hasFreeGacha)
        {
            // Use the free gacha pull
            voucher.UseFreeGacha();
            Debug.Log("Free gacha pull used from Voucher item!");
            return true;
        }
        if (p.curHealth > 0)
        {
            // Decrement player coins, providing both required parameters
            p.TakeDamage(1, this.gameObject);  // Pass 'this.gameObject' as the source of damage

            // Give player a cap if they use gacha machine, cause *shrugs*
            p.caps += 1;
            return true;
        }

        // Player didn't have enough coins.
        return false;

    }

    public void PlayDispenseAnimation()
    {
        //play the audio for the gacha roll
        gachaAudioSource.PlayOneShot(gachaRollClip);

        //this animation has an event that will call the "SpawnCapsule" method when it ends.
        gachaAnimator.SetTrigger("drop");
    }

    public void OnBossDefeated()
    {
        BossDefeated?.Invoke();
    }
}