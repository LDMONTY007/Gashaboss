using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//LD Montello
//Note: Make sure this
//object's collider excludes
//the player layer.
public class Weapon : MonoBehaviour
{
    //the weapon should handle animations within itself
    //cus it'll make scaling easier. 

    public ParticleSystem hitParticles;

    public CollisionSensor collisionSensor;

    //public Collider weaponCollider;

    public bool hasCooldown = false;

    public float cooldownTime = 1f;

    public bool canAttack = true;

    public float attackRadius = 10f;

    public float attackDistance = 1f;

    LayerMask playerMask;


    //colors used when debugging the collision sensor mesh
    private Color ogMeshColor;
    //blue with 100 alpha.
    private Color cooldownMeshColor = new Color(0, 0, 1, 100f / 255f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //get original mesh color
        ogMeshColor = collisionSensor.meshColor;
        //weaponCollider.enabled = false;

        //get player mask
        playerMask = LayerMask.GetMask("Player", "Ignore Raycast"); //Assign our layer mask to player
        playerMask = ~playerMask; //Invert the layermask value so instead of being just the player it becomes every layer but the mask

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Attack()
    {
        //if we can't attack,
        //print a message in case something
        //breaks because of this.
        if (!canAttack)
        {
            Debug.LogWarning("Cannot attack during cooldown");
            return;
        }

        //Start AttackCoroutine
        StartCoroutine(AttackCoroutine());


    }

    public IEnumerator AttackCoroutine()
    {
        //don't allow other attacks during our current attack.
        canAttack = false;

        //TODO:
        //implement an animation for attacking,
        //but for now enable the collider for an attack.
        //weaponCollider.enabled = true;

        List<GameObject> objs = collisionSensor.ScanForObjects();

        if (objs.Count > 0 )
        {
            for ( int i = 0; i < objs.Count; i++ )
            {
                if (objs[i] != null)
                {
                    IDamageable damageable = objs[i].GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        //if the damageable is a boss, and is dead, skip this object in the loop.
                        BossController boss = objs[i].GetComponent<BossController>();
                        if (boss.isDead)
                        {
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
                else
                {
                    Debug.LogWarning("Object was null while checking if it is damageable".Color("Orange"));
                }
                
            }
        }

        /*//RaycastHit hits = Physics.SphereCastAll(transform.position, attackRadius, transform.forward, attackDistance, playerMask);
        RaycastHit sphereHit;
        
        if (Physics.SphereCast(transform.position, attackRadius, transform.forward * attackDistance, out sphereHit, attackDistance, playerMask))
        {
            //if this hit is in front of us, check for damageables
            if (Mathf.Abs(Vector3.Dot(transform.position, sphereHit.point)) > 0)
            {
                IDamageable damageable = sphereHit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    //make the damageable take damage.
                    //and tell it we gave it damage.
                    damageable.TakeDamage(1, gameObject);

                    //TODO:
                    //Spawn a particle system burst that destroys itself
                    //when we hit a damageable so that the player can see where they hit.
                    //maybe give each weapon an individualized particle effect.
                    //spawn a particle effect facing outward from the normal
                    //at the position that was hit.
                    Debug.Log(sphereHit.point);
                    Instantiate(hitParticles, sphereHit.point, Quaternion.LookRotation(sphereHit.normal));
                }
            }


        }*/

        //set color of debug mesh to show we are in cooldown
        collisionSensor.meshColor = cooldownMeshColor;

        //if we have a cooldown, wait the cooldown time before attacking.
        if (hasCooldown)
        yield return new WaitForSeconds(cooldownTime);

        //restore default color
        collisionSensor.meshColor = ogMeshColor;

        //allow us to attack again.
        canAttack = true;

        yield break;
    }



    /*private void OnCollisionEnter(Collision collision)
    {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            //make the damageable take damage.
            //and tell it we gave it damage.
            damageable.TakeDamage(1, gameObject);

            //TODO:
            //Spawn a particle system burst that destroys itself
            //when we hit a damageable so that the player can see where they hit.
            //maybe give each weapon an individualized particle effect.
            //spawn a particle effect facing outward from the normal
            //at the position that was hit.
            Instantiate(hitParticles, collision.GetContact(0).point, Quaternion.LookRotation(collision.GetContact(0).normal));
        }
    }*/

    //Cooldown coroutine.
    public IEnumerator CooldownCoroutine()
    {
        canAttack = false;

        yield return new WaitForSeconds(cooldownTime);

        canAttack = true;
    }
}
