using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileWeapon : Weapon
{
    public GameObject projectile;

    public float specialWaitTime = .5f;
    
    public float altSizeMult = 1.5f;

    //if the launch transform's forward is angled upwards it will launch upwards.
    public Transform launchTransform;

    public override IEnumerator AttackCoroutine()
    {
        //don't allow other attacks during our current attack.
        canAttack = false;

        //TODO:
        //implement an animation for attacking,
        //but for now enable the collider for an attack.
        //weaponCollider.enabled = true;

        //Create projectile with the launch transform rotation at launch position
        Instantiate(projectile, launchTransform.position, launchTransform.rotation);

        //if we have a cooldown, wait the cooldown time before attacking.
        if (hasCooldown)
            yield return new WaitForSeconds(cooldownTime);

        //restore default color
        //collisionSensor.sensorColor = ogMeshColor;

        //allow us to attack again.
        canAttack = true;

        yield break;
    }

    public override IEnumerator AltAttackCoroutine(){
        //start the attack animation
        if (animator != null)
            animator.SetTrigger("altAttack");

        //don't allow other attacks during our current attack.
        canAttack = false;

        //Create projectile with the launch transform rotation at launch position
        GameObject p = Instantiate(projectile, launchTransform.position, launchTransform.rotation);
        
        // check if projectile is explosive 
        BombProjectile bomb = p.GetComponent<BombProjectile>();
        if (bomb != null){
            //Instantly spawn and detonate the explosive at our feet
            bomb.Detonate();
        }else{
            //For the non-explosive alt we will fire a single Larger Shot
            p.transform.localScale *= altSizeMult;
        }
        
        //launch the player upwards, regardless of projectile type
        player.LaunchPlayer(player.transform.up, 30f, 1f, 2f);

        //set color of debug mesh to show we are in cooldown
        collisionSensor.sensorColor = cooldownMeshColor;

        //if we have a cooldown, wait the cooldown time before attacking.
        if (hasCooldown)
            yield return new WaitForSeconds(altCooldownTime);

        //restore default color
        collisionSensor.sensorColor = ogMeshColor;

        //allow us to attack again.
        canAttack = true;

        yield break;
    }

    public override IEnumerator SpecialAttackCoroutine(){
        //start the attack animation
        if (animator != null)
            animator.SetTrigger("specialAttack");
        
        //don't allow other attacks during our current attack.
        canAttack = false;

        //TODO:
        //implement an animation for attacking,
        //but for now enable the collider for an attack.
        //weaponCollider.enabled = true;

        //Create projectile at launch position but launch it directly upwards.
        GameObject p = Instantiate(projectile, launchTransform.position, Quaternion.LookRotation(player.transform.up));
        // check if projectile is explosive 
        BombProjectile bomb = p.GetComponent<BombProjectile>();
        if (bomb != null){
            //change the starting launch speed to 130 so it goes higher.
            p.launchSpeed = 130f;
            //make it instantly explode when it hits something.
            p.bouncesBeforeExplosion = 0;
        }else{
            //For the non-explosive Special we will fire two shots, in quick succession
            yield return new WaitForSeconds(specialWaitTime);
            Instantiate(projectile, launchTransform.position, Quaternion.LookRotation(player.transform.up));
        }

        //set color of debug mesh to show we are in cooldown
        collisionSensor.sensorColor = cooldownMeshColor;

        //if we have a cooldown, wait the cooldown time before attacking.
        if (hasCooldown)
            yield return new WaitForSeconds(specialCooldownTime);

        //restore default color
        collisionSensor.sensorColor = ogMeshColor;

        //allow us to attack again.
        canAttack = true;

        yield break;
    }
}
