using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class GachaMachine : MonoBehaviour, IDamageable {
    //LD Montello.
    //these need to be GameObjects because they
    //will be prefabs with the CapsuleDrop class on them.
    //this is also how we can have specific models for different
    //capsules for different machines or even bosses.
    [SerializeField] private List<GameObject> drops;
    [SerializeField] private GameObject capsule;
    private int totalWeights;

    public Transform capsuleSpawnTransform;

    public Animator gachaAnimator;

    private GameObject currentCapsule;

    //if current capsule isn't null, then
    //a capsule exists
    bool capsuleExists => currentCapsule != null;
   
    //this isn't the best way to check for this,
    //but seeing as we don't do it often
    //when we check we'll just search for if a boss controller
    //exists.
    bool bossExists => FindAnyObjectByType<BossController>() != null;

    public SetRandomSeed setRandomSeedAnimated;

    public AudioClip gachaRollClip;

    private AudioSource gachaAudioSource;

    public void Start(){
        gachaAudioSource = GetComponent<AudioSource>();

        //loadDrops();
        totalWeights = 0;
        foreach (GameObject drop in drops){
            totalWeights += drop.GetComponent<CapsuleDrop>().weight;
        }

    }

    public GameObject GetRandomDrop(){
        int dropRoll = Random.Range(1, totalWeights + 1);
        int currDrop = 0;
        //Go through the objects in the list adding their weights up until
        //we find the weight we want
        //We should never have a case where we reach the end and haven't found our rolled object
        foreach (GameObject drop in drops){
            if (currDrop >= dropRoll){
                if (drop.GetComponent<CapsuleDrop>().isRemovedAfterDrop){
                    drops.Remove(drop);
                    totalWeights -= drop.GetComponent<CapsuleDrop>().weight;
                }
                return drop.GetComponent<CapsuleDrop>().droppedObject;
            }
            currDrop += drop.GetComponent<CapsuleDrop>().weight;
        }
        return null;
    }

    public void TakeDamage(int damage, GameObject other){
        
        //try to buy a capsule so long as there isn't a capsule waiting to be opened.
        if (!capsuleExists && !bossExists && Player.instance != null && TryBuyCapsule(Player.instance))
        {
            //Generate a new color for the capsule
            setRandomSeedAnimated.GenRandomSeed();

            //after the animation finishes playing it will spawn the capsule.
            PlayDispenseAnimation();
        }
    }

    public void SpawnCapsule()
    {
        GameObject drop = GetRandomDrop();
        if (drop == null)
        {
            // TODO: Handle Error <- Shouldn't be possible
            return;
        }
        Debug.Log("Spawning Drop...");
        //instantiate the capsule at the spawn position for capsules.
        GameObject intCapsule = Instantiate(capsule, capsuleSpawnTransform.position, capsule.transform.rotation);
        intCapsule.GetComponent<Capsule>().SetObjectHeld(drop.gameObject);

        //copy the seed color from the animated capsule's color seed to the new instanced capsule.
        intCapsule.GetComponentInChildren<SetRandomSeed>().seed = setRandomSeedAnimated.seed;

        //set a reference to our current capsule.
        currentCapsule = intCapsule;
    }

    public bool TryBuyCapsule(Player p)
    {
        if (p.curHealth > 1)
        {
            //Decrement player coins.
            p.curHealth--;
            return true;
        }
        //player didn't have enough coins.
        return false;

    }

    public void PlayDispenseAnimation()
    {
        //play the audio for the gacha roll
        gachaAudioSource.PlayOneShot(gachaRollClip);

        //this animation has an event that will call the "SpawnCapsule" method when it ends.
        gachaAnimator.SetTrigger("drop");
    }
}