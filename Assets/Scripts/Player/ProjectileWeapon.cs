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
        //don't allow other attacks during our current attack.
        canAttack = false;

        //Create projectile with the launch transform rotation at launch position
        GameObject spawnedProj = Instantiate(projectile, launchTransform.position, launchTransform.rotation);

        // Scale the size of the object up, for our alt attack
        spawnedProj.transform.localScale *= altSizeMult;

        //if we have a cooldown, wait the cooldown time before attacking.
        if (hasCooldown)
            yield return new WaitForSeconds(altCooldownTime);

        //restore default color
        //collisionSensor.sensorColor = ogMeshColor;

        //allow us to attack again.
        canAttack = true;

        yield break;
    }

    public override IEnumerator SpecialAttackCoroutine()
    {
        //don't allow other attacks during our current attack.
        canAttack = false;

        //TODO:
        //implement an animation for attacking,
        //but for now enable the collider for an attack.
        //weaponCollider.enabled = true;

        //Create projectile with the launch transform rotation at launch position
        Instantiate(projectile, launchTransform.position, launchTransform.rotation);

        // For the Special we will fire two shots, in quick succession
        yield return new WaitForSeconds(specialWaitTime);
        Instantiate(projectile, launchTransform.position, launchTransform.rotation);

        //if we have a cooldown, wait the cooldown time before attacking.
        if (hasCooldown)
            yield return new WaitForSeconds(specialCooldownTime);

        //restore default color
        //collisionSensor.sensorColor = ogMeshColor;

        //allow us to attack again.
        canAttack = true;

        yield break;
    }
}
