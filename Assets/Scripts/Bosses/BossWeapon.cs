using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

public class BossWeapon: Weapon{
    public override void AltAttack(){
        if (!canAttack){
            //Debug.LogWarning("Cannot attack during cooldown");
            return;
        }
        StartCoroutine(AltAttackCoroutine());
    }
    public override IEnumerator AttackCoroutine(){
        //don't allow other attacks during our current attack.
        canAttack = false;

        List<GameObject> objs = collisionSensor.ScanForObjects();

        if (objs.Count > 0 ){
            for ( int i = 0; i < objs.Count; i++ ){
                if (objs[i] != null){
                    IDamageable damageable = objs[i].GetComponent<IDamageable>();
                    if (damageable != null){
                        //if the damageable is a boss, and is dead, skip this object in the loop.
                        BossController boss = objs[i].GetComponent<BossController>();
                        if (boss != null && boss.isDead){
                            continue;
                        }

                        //make the damageable take damage.
                        //and tell it we gave it damage.
                        damageable.TakeDamage(1, gameObject);

                        //TODO:
                        //Spawn a particle system burst that destroys itself
                        //when we hit a damageable so that the player can see where they hit.
                        //maybe give each weapon an individualized particle effect.
                        //spawn a particle effect facing outward from the normal
                        //at the position that was hit.
                        //TODO:
                        //in the future replace this with
                        //some code that checks the direction
                        //the attack is coming from, does a box 
                        //cast and uses the normal and hit point from
                        //that box cast to calculate where the damage
                        //particle effect should spawn. 
                        Collider c = objs[i].GetComponent<Collider>();

                        Vector3 closestPoint = c.ClosestPoint(Camera.main.transform.position);

                        Instantiate(hitParticles, closestPoint + (-Camera.main.transform.forward.normalized * 0.25f), Quaternion.LookRotation(Camera.main.transform.position));
                    }
                }
                else{
                    Debug.LogWarning("Object was null while checking if it is damageable".Color("Orange"));
                }
                
            }
        }
    }

    public override IEnumerator AltAttackCoroutine(){
        //don't allow other attacks during our current attack.
        canAttack = false;

        List<GameObject> objs = collisionSensor.ScanForObjects();
        if (objs.Count > 0){
            for (int i = 0; i < objs.Count; i++){
                if (objs[i] != null){
                    IDamageable damageable = objs[i].GetComponent<IDamageable>();
                    if (damageable != null){
                        //if the damageable is a boss, and is dead, skip this object in the loop.
                        BossController boss = objs[i].GetComponent<BossController>();
                        if (boss != null && boss.isDead){
                            continue;
                        }

                        //make the damageable take damage.
                        damageable.TakeDamage(1, gameObject);

                        //TODO:
                        //Spawn a particle system burst that destroys itself
                        //when we hit a damageable so that the player can see where they hit.
                        //maybe give each weapon an individualized particle effect.
                        //spawn a particle effect facing outward from the normal
                        //at the position that was hit.
                        //TODO:
                        //in the future replace this with
                        //some code that checks the direction
                        //the attack is coming from, does a box 
                        //cast and uses the normal and hit point from
                        //that box cast to calculate where the damage
                        //particle effect should spawn. 
                        Collider c = objs[i].GetComponent<Collider>();

                        Vector3 closestPoint = c.ClosestPoint(Camera.main.transform.position);

                        Instantiate(hitParticles, closestPoint + (-Camera.main.transform.forward.normalized * 0.25f), Quaternion.LookRotation(Camera.main.transform.position));
                    }
                }else{
                    Debug.LogWarning("Object was null while checking if it is damageable".Color("Orange"));
                }
            }
        }

        //set color of debug mesh to show we are in cooldown
        collisionSensor.sensorColor = cooldownMeshColor;

        //if we have a cooldown, wait the cooldown time before attacking.
        if (hasCooldown) yield return new WaitForSeconds(altCooldownTime);

        //restore default color
        collisionSensor.sensorColor = ogMeshColor;

        //allow us to attack again.
        canAttack = true;

        yield break;
    }
    public override IEnumerator SpecialAttackCoroutine(){
        //don't allow other attacks during our current attack.
        canAttack = false;

        List<GameObject> objs = collisionSensor.ScanForObjects();

        if (objs.Count > 0){
            for (int i = 0; i < objs.Count; i++){
                if (objs[i] != null){
                    IDamageable damageable = objs[i].GetComponent<IDamageable>();
                    if (damageable != null){
                        //if the damageable is a boss, and is dead, skip this object in the loop.
                        BossController boss = objs[i].GetComponent<BossController>();
                        if (boss != null && boss.isDead){
                            continue;
                        }
                        //make the damageable take damage.
                        //and tell it we gave it damage.
                        damageable.TakeDamage(1, gameObject);

                        //TODO:
                        //Spawn a particle system burst that destroys itself
                        //when we hit a damageable so that the player can see where they hit.
                        //maybe give each weapon an individualized particle effect.
                        //spawn a particle effect facing outward from the normal
                        //at the position that was hit.
                        //TODO:
                        //in the future replace this with
                        //some code that checks the direction
                        //the attack is coming from, does a box 
                        //cast and uses the normal and hit point from
                        //that box cast to calculate where the damage
                        //particle effect should spawn. 
                        Collider c = objs[i].GetComponent<Collider>();

                        Vector3 closestPoint = c.ClosestPoint(Camera.main.transform.position);

                        Instantiate(hitParticles, closestPoint + (-Camera.main.transform.forward.normalized * 0.25f), Quaternion.LookRotation(Camera.main.transform.position));

                        //wait for fixedupdate before launching player.
                        //if we didn't wait we'd have inconsistent physics.
                        yield return new WaitForFixedUpdate();

                        //player.LaunchPlayer(player.transform.up, 30f, 1f, 2f);
                    }
                }else{
                    Debug.LogWarning("Object was null while checking if it is damageable".Color("Orange"));
                }
            }
        }
        //set color of debug mesh to show we are in cooldown
        collisionSensor.sensorColor = cooldownMeshColor;

        //this special attack has no cooldown other than it's animation time.
        //if we have a cooldown, wait the cooldown time before attacking.
        if (hasCooldown) yield return new WaitForSeconds(specialCooldownTime);

        //restore default color
        collisionSensor.sensorColor = ogMeshColor;

        //allow us to attack again.
        canAttack = true;
        yield break;
    }
}
