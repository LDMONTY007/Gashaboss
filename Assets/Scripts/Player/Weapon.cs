using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//LD Montello
//Note: Make sure this
//object's collider excludes
//the player layer.
public class Weapon : MonoBehaviour, ICollectable
{
    //the weapon should handle animations within itself
    //cus it'll make scaling easier. 
    public string weaponName;
    public Sprite icon; // sprite used in collections menu

    public ParticleSystem hitParticles;

    public CollisionSensor collisionSensor;

    //public Collider weaponCollider;

    public bool hasCooldown = false;

    public float cooldownTime = 1f;

    public bool canAttack = true;

    public float attackDistance = 1f;

    LayerMask playerMask;

    public Player player;


    //colors used when debugging the collision sensor mesh
    private Color ogMeshColor;
    //blue with 100 alpha.
    private Color cooldownMeshColor = new Color(0, 0, 1, 100f / 255f);

    //when unity engine
    //does the "Loading" prompt
    //this method gets called.
    //this also happens whenever
    //the attackDistance is modified
    //in the inspector
    //which means that in the editor
    //the radius of the trigger
    //will also be modified without
    //the game running. 
    //this is the ideal behavior.
    private void OnValidate()
    {
        //set the collision sensor 
        //collider radius to be
        //the same as our attack distance
        collisionSensor.triggerCollider.radius = attackDistance / 2;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //set the collision sensor 
        //collider radius to be
        //the same as our attack distance
        collisionSensor.triggerCollider.radius = attackDistance / 2;

        //get original mesh color
        ogMeshColor = collisionSensor.sensorColor;
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
            //Debug.LogWarning("Cannot attack during cooldown");
            return;
        }

        //Start AttackCoroutine
        StartCoroutine(AttackCoroutine());


    }

    public void AltAttack()
    {
        if (!canAttack)
        {
            //Debug.LogWarning("Cannot attack during cooldown");
            return;
        }

        //Start AltAttackCoroutine
        StartCoroutine(AltAttackCoroutine());
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
                        if (boss != null && boss.isDead)
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
        collisionSensor.sensorColor = cooldownMeshColor;

        //if we have a cooldown, wait the cooldown time before attacking.
        if (hasCooldown)
        yield return new WaitForSeconds(cooldownTime);

        //restore default color
        collisionSensor.sensorColor = ogMeshColor;

        //allow us to attack again.
        canAttack = true;

        yield break;
    }


    public IEnumerator AltAttackCoroutine()
    {
        //don't allow other attacks during our current attack.
        canAttack = false;

        //TODO:
        //implement an animation for attacking,
        //but for now enable the collider for an attack.
        //weaponCollider.enabled = true;

        List<GameObject> objs = collisionSensor.ScanForObjects();

        if (objs.Count > 0)
        {
            for (int i = 0; i < objs.Count; i++)
            {
                if (objs[i] != null)
                {
                    IDamageable damageable = objs[i].GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        //if the damageable is a boss, and is dead, skip this object in the loop.
                        BossController boss = objs[i].GetComponent<BossController>();
                        if (boss != null && boss.isDead)
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

                        player.LaunchPlayer(player.transform.up, 30f);
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
        collisionSensor.sensorColor = cooldownMeshColor;

        //if we have a cooldown, wait the cooldown time before attacking.
        if (hasCooldown)
            yield return new WaitForSeconds(cooldownTime);

        //restore default color
        collisionSensor.sensorColor = ogMeshColor;

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
    public void OnCollect(){
        if (name == null || icon == null){
            Debug.LogError("Weapon isn't set up properly, aborting pickup.");
            return;
        }
        CollectionManager.instance.AddToCollection(this.gameObject, weaponName, icon);
        Player.instance.curWeapon = this;
    }
}
