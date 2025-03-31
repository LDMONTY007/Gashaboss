using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileWeapon : Weapon
{
    public GameObject projectile;

    //if the launch transform's forward is angled upwards it will launch upwards.
    public Transform launchTransform;

    public float launchForce = 15f;

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


        Projectile p = projectile.GetComponent<Projectile>();

        //if we have a cooldown, wait the cooldown time before attacking.
        if (hasCooldown)
            yield return new WaitForSeconds(cooldownTime);

        //restore default color
        //collisionSensor.sensorColor = ogMeshColor;

        //allow us to attack again.
        canAttack = true;

        yield break;
    }
}
