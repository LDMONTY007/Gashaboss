using System.Collections;
using UnityEngine;

//TODO: Add Capsule animation for spawning from machine
// want it to drop out of machine with a small bounce, need machine and capsule models to finish
public class Capsule: MonoBehaviour, IDamageable{
    private GameObject objectHeld;

    public Animator capsuleAnimator;

    public void SetObjectHeld(GameObject obj){
        objectHeld = obj;
    }
    public void TakeDamage(int damage, GameObject other){
        // TODO: Play Capsule Pop Animation/Sound
        PlayCapsuleAnimation();
        
    }

    public void PlayCapsuleAnimation()
    {
        //Play the capsule animation using a coroutine
        //the coroutine will track until the animation ends and
        //then when it ends the capsule will spawn it's held object.
        StartCoroutine(CapsuleAnimationCoroutine());
    }

    public IEnumerator CapsuleAnimationCoroutine()
    {
        //TODO:
        //Start the actual capsule popping open animation here.

        //wait for the capsule animation to finish playing
        //yield return LDUtil.WaitForAnimationFinish(capsuleAnimator);

        SpawnHeldObject();

        //TODO:
        //Remove the following line
        //after the capsule animation is implemented.
        yield return null;

        //Destroy the capsule object.
        Destroy(gameObject);
    }

    public void SpawnHeldObject()
    {
        //spawn the held object at the capsule's position.
        Instantiate(objectHeld, transform.position, objectHeld.transform.rotation);
    }
}